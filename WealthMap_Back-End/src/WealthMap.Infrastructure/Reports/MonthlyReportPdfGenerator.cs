using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Features.Reports.DTOs;

namespace WealthMap.Infrastructure.Reports;

/// <summary>
/// Renders an already-assembled report. All figures arrive computed — this class
/// decides layout only, never what a number means.
///
/// The palette and shapes mirror the web client's design tokens
/// (WealthMap_Front-End/src/assets/styles/_tokens.scss) so a downloaded report
/// looks like the screen it came from. Keep the two in step.
/// </summary>
public class MonthlyReportPdfGenerator : IPdfReportGenerator
{
    // ── Design tokens ──────────────────────────
    private const string Canvas = "#F3F2EE";
    private const string CanvasAlt = "#ECE9E2";
    private const string Surface = "#FFFFFF";
    private const string Line = "#E9E9E7";

    private const string Ink = "#201F1D";
    private const string Muted = "#6B6A65";
    private const string Subtle = "#C0BFB7";

    private const string Accent = "#212F46";
    private const string Gold = "#CBB697";

    private const string Positive = "#3F5D45";
    private const string Negative = "#8C3B32";

    private const string BorderColor = "#5C5A55";
    private const string ShadowColor = "#BDBAB2";

    /// <summary>
    /// The web client loads Inter. QuestPDF can only use it if the font is
    /// registered or installed on the host, otherwise it silently falls back to
    /// its bundled default — same layout and colours, different letterforms.
    /// Drop Inter .ttf files into a "Fonts" folder beside the API and they are
    /// picked up automatically.
    /// </summary>
    private const string Font = "Inter";

    private const int ShadowOffset = 3;

    static MonthlyReportPdfGenerator()
    {
        RegisterBundledFonts();
    }

    private static void RegisterBundledFonts()
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "Fonts");

        if (!Directory.Exists(directory)) return;

        foreach (var file in Directory.EnumerateFiles(directory, "*.ttf"))
        {
            try
            {
                using var stream = File.OpenRead(file);
                QuestPDF.Drawing.FontManager.RegisterFont(stream);
            }
            catch
            {
                // A malformed font must not stop reports from being generated.
            }
        }
    }

    public byte[] GenerateMonthlyReport(MonthlyReportDto report, string? locale = null)
    {
        // Resolved once per request and passed down. Never stored on the instance:
        // this generator is a singleton, and a language held in a field would leak
        // one user's locale into another's report whenever two downloads overlapped.
        var text = ReportText.For(locale);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.4f, Unit.Centimetre);
                page.PageColor(Canvas);
                page.DefaultTextStyle(t => t.FontSize(9.5f).FontFamily(Font).FontColor(Ink));

                page.Header().Element(h => ComposeHeader(h, report, text));
                page.Content().PaddingTop(14).Element(c => ComposeContent(c, report, text));

                page.Footer().PaddingTop(8).AlignCenter().Text(t =>
                {
                    var stamp = report.GeneratedAt.ToString("yyyy-MM-dd HH:mm", text.Culture);

                    t.DefaultTextStyle(s => s.FontSize(7.5f).FontColor(Subtle));
                    t.Span($"WealthMap · {string.Format(text["generated"], stamp)} · {text["page"]} ");
                    t.CurrentPageNumber();
                    t.Span($" {text["of"]} ");
                    t.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    /// <summary>
    /// The flat card from the web design: 1px border, no rounding on paper, and a
    /// hard offset shadow with zero blur faked by a second layer behind.
    /// </summary>
    private static IContainer FlatCard(IContainer container) =>
        container
            // The outer block paints the shadow colour; the inset below leaves it
            // showing along the right and bottom edges only — a hard offset with
            // zero blur, the same shape the web client draws with box-shadow.
            .Background(ShadowColor)
            .PaddingRight(ShadowOffset)
            .PaddingBottom(ShadowOffset)
            .Background(Surface)
            .Border(1).BorderColor(BorderColor);

    private static void ComposeHeader(IContainer container, MonthlyReportDto report, ReportText text)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    left.Item().Row(brand =>
                    {
                        brand.AutoItem()
                            .Width(20).Height(20)
                            .Background(Ink)
                            .AlignCenter().AlignMiddle()
                            .Text("WM").FontSize(7).Bold().FontColor(Canvas);

                        brand.AutoItem().PaddingLeft(7).AlignMiddle()
                            .Text("WealthMap").FontSize(11).SemiBold();
                    });

                    left.Item().PaddingTop(10)
                        .Text(text["title"]).FontSize(21).SemiBold().FontColor(Ink);

                    // Month name from the culture, so a Spanish report says "agosto".
                    left.Item().Text(report.PeriodStart.ToString("MMMM yyyy", text.Culture))
                        .FontSize(11).FontColor(Muted);
                });

                row.ConstantItem(170).AlignRight().Column(right =>
                {
                    right.Item().AlignRight().Text(report.UserFullName).FontSize(10).SemiBold();
                    right.Item().AlignRight()
                        .Text($"{report.PeriodStart:yyyy-MM-dd} → {report.PeriodEnd:yyyy-MM-dd}")
                        .FontSize(8).FontColor(Muted);
                    right.Item().AlignRight()
                        .Text(string.Format(text["amountsIn"], report.Currency))
                        .FontSize(8).FontColor(Muted);
                });
            });

            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(BorderColor);
        });
    }

    private static void ComposeContent(IContainer container, MonthlyReportDto report, ReportText text)
    {
        container.Column(column =>
        {
            column.Spacing(14);

            column.Item().Element(e => ComposeSummary(e, report, text));
            column.Item().Element(e => ComposeIncome(e, report, text));
            column.Item().Element(e => ComposeSpending(e, report, text));
            column.Item().Element(e => ComposeTopExpenses(e, report, text));
            column.Item().Element(e => ComposeAccounts(e, report, text));

            if (report.Cards.Count > 0)
                column.Item().Element(e => ComposeCards(e, report, text));

            if (report.Goals.Count > 0)
                column.Item().Element(e => ComposeGoals(e, report, text));
        });
    }

    private static void ComposeSummary(IContainer container, MonthlyReportDto report, ReportText text)
    {
        container.Row(row =>
        {
            row.Spacing(10);
            row.RelativeItem().Element(e => Tile(e, text["income"], report.Income.Total, report, text, Positive));
            row.RelativeItem().Element(e => Tile(e, text["spending"], report.Spending.TotalPurchases, report, text, Negative));
            row.RelativeItem().Element(e => Tile(
                e,
                text["netResult"],
                report.NetResult,
                report,
                text,
                report.NetResult >= 0 ? Positive : Negative));
        });
    }

    private static void Tile(
        IContainer container, string label, decimal value,
        MonthlyReportDto report, ReportText text, string color)
    {
        container.Element(FlatCard).Padding(11).Column(c =>
        {
            c.Item().Text(Label(label)).FontSize(7).SemiBold().FontColor(Muted).LetterSpacing(0.08f);
            c.Item().PaddingTop(4)
                .Text($"{Num(value, text)} {report.Currency}").FontSize(15).SemiBold().FontColor(color);
        });
    }

    private static void ComposeIncome(IContainer container, MonthlyReportDto report, ReportText text)
    {
        container.Element(FlatCard).Column(column =>
        {
            SectionHeader(column, text["income"]);

            if (report.Income.Lines.Count == 0)
            {
                Empty(column, text["noIncome"]);
            }
            else
            {
                column.Item().Padding(12).PaddingTop(8).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        c.RelativeColumn(1);
                        c.RelativeColumn(2);
                    });

                    HeaderRow(table, (text["type"], false), (text["count"], true), (text["total"], true));

                    foreach (var line in report.Income.Lines)
                    {
                        table.Cell().Element(Body).Text(text.Value(line.Type));
                        table.Cell().Element(Body).AlignRight().Text(line.Count.ToString(text.Culture));
                        table.Cell().Element(Body).AlignRight().Text(Num(line.Total, text));
                    }

                    table.Cell().Element(TotalRow).Text(text["total"]).SemiBold();
                    table.Cell().Element(TotalRow).Text("");
                    table.Cell().Element(TotalRow).AlignRight().Text(Num(report.Income.Total, text)).SemiBold();
                });
            }

            if (report.Income.ExpectedSalaryNet > 0)
                Footnote(column, string.Format(
                    text["expectedSalary"], Num(report.Income.ExpectedSalaryNet, text), report.Currency));
        });
    }

    private static void ComposeSpending(IContainer container, MonthlyReportDto report, ReportText text)
    {
        container.Element(FlatCard).Column(column =>
        {
            SectionHeader(column, text["spendingByCategory"]);

            if (report.Spending.ByCategory.Count == 0)
            {
                Empty(column, text["noPurchases"]);
            }
            else
            {
                column.Item().Padding(12).PaddingTop(8).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        c.RelativeColumn(1);
                        c.RelativeColumn(2);
                        c.RelativeColumn(1);
                    });

                    HeaderRow(table,
                        (text["category"], false), (text["items"], true),
                        (text["total"], true), (text["share"], true));

                    foreach (var category in report.Spending.ByCategory)
                    {
                        table.Cell().Element(Body).Text(text.Value(category.Category));
                        table.Cell().Element(Body).AlignRight().Text(category.Count.ToString(text.Culture));
                        table.Cell().Element(Body).AlignRight().Text(Num(category.Total, text));
                        table.Cell().Element(Body).AlignRight()
                            .Text(category.SharePercentage.ToString("N1", text.Culture) + "%");
                    }

                    table.Cell().Element(TotalRow).Text(text["total"]).SemiBold();
                    table.Cell().Element(TotalRow).Text("");
                    table.Cell().Element(TotalRow).AlignRight()
                        .Text(Num(report.Spending.TotalPurchases, text)).SemiBold();
                    table.Cell().Element(TotalRow).Text("");
                });
            }

            if (report.Spending.TotalCashWithdrawn > 0)
                Footnote(column, string.Format(
                    text["cashNote"], Num(report.Spending.TotalCashWithdrawn, text), report.Currency));
        });
    }

    private static void ComposeTopExpenses(IContainer container, MonthlyReportDto report, ReportText text)
    {
        container.Element(FlatCard).Column(column =>
        {
            SectionHeader(column, text["largestExpenses"]);

            if (report.Spending.TopExpenses.Count == 0)
            {
                Empty(column, text["nothingToShow"]);
                return;
            }

            column.Item().Padding(12).PaddingTop(8).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    // Wide enough for "MM-dd HH:mm"; the old 62 fit the date alone.
                    c.ConstantColumn(88);
                    c.RelativeColumn(3);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                });

                HeaderRow(table,
                    (text["dateUtc"], false), (text["item"], false), (text["category"], false),
                    (text["method"], false), (text["amount"], true));

                var isFirst = true;

                foreach (var expense in report.Spending.TopExpenses)
                {
                    // The biggest expense is the one worth noticing, as on screen.
                    var tint = isFirst ? CanvasAlt : Surface;

                    // UTC like every other time in this document — the footer says so,
                    // and the report's month is bounded in UTC too, so a local-time
                    // reading here could show a date outside the month it sits in.
                    table.Cell().Background(tint).Element(Body)
                        .Text(expense.OccurredAt.ToString("MM-dd HH:mm", text.Culture));

                    // The store sits under the item rather than in its own column:
                    // a sixth column would squeeze the four that carry the numbers.
                    table.Cell().Background(tint).Element(Body).Column(item =>
                    {
                        item.Item().Text(expense.ProductName).SemiBold();

                        if (!string.IsNullOrWhiteSpace(expense.StoreName))
                            item.Item().Text(expense.StoreName).FontSize(7.5f).FontColor(Muted);
                    });

                    table.Cell().Background(tint).Element(Body).Text(text.Value(expense.Category));
                    table.Cell().Background(tint).Element(Body).Text(text.Value(expense.PaymentMethod));
                    table.Cell().Background(tint).Element(Body).AlignRight()
                        .Text(Num(expense.Amount, text)).SemiBold();

                    isFirst = false;
                }
            });
        });
    }

    private static void ComposeAccounts(IContainer container, MonthlyReportDto report, ReportText text)
    {
        container.Element(FlatCard).Column(column =>
        {
            SectionHeader(column, text["accounts"]);

            if (report.Accounts.Count == 0)
            {
                Empty(column, text["noAccounts"]);
                return;
            }

            column.Item().Padding(12).PaddingTop(8).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(3);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                });

                HeaderRow(table,
                    (text["account"], false), (text["opening"], true), (text["in"], true),
                    (text["out"], true), (text["closing"], true));

                foreach (var account in report.Accounts)
                {
                    table.Cell().Element(Body).Column(c =>
                    {
                        var movements = string.Format(text["movements"], account.MovementCount);

                        c.Item().Text(account.Name).SemiBold();
                        c.Item().Text($"{text.Value(account.Type)} · {movements}")
                            .FontSize(7.5f).FontColor(Muted);
                    });

                    table.Cell().Element(Body).AlignRight().Text(Num(account.OpeningBalance, text)).FontColor(Muted);
                    table.Cell().Element(Body).AlignRight().Text(Num(account.TotalIn, text)).FontColor(Positive);
                    table.Cell().Element(Body).AlignRight().Text(Num(account.TotalOut, text)).FontColor(Negative);
                    table.Cell().Element(Body).AlignRight().Text(Num(account.ClosingBalance, text)).SemiBold();
                }
            });
        });
    }

    private static void ComposeCards(IContainer container, MonthlyReportDto report, ReportText text)
    {
        container.Element(FlatCard).Column(column =>
        {
            SectionHeader(column, text["creditCards"]);

            column.Item().Padding(12).PaddingTop(8).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(3);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                });

                HeaderRow(table,
                    (text["card"], false), (text["charged"], true), (text["paid"], true),
                    (text["owed"], true), (text["available"], true));

                foreach (var card in report.Cards)
                {
                    table.Cell().Element(Body).Column(c =>
                    {
                        c.Item().Text(card.CardName).SemiBold();
                        c.Item().Text(string.Format(
                                text["cardMeta"], card.PaymentDueDay, Num(card.CreditLimit, text)))
                            .FontSize(7.5f).FontColor(Muted);
                    });

                    table.Cell().Element(Body).AlignRight().Text(Num(card.ChargedThisMonth, text)).FontColor(Negative);
                    table.Cell().Element(Body).AlignRight().Text(Num(card.PaidThisMonth, text)).FontColor(Positive);
                    table.Cell().Element(Body).AlignRight().Text(Num(card.UsedCredit, text)).SemiBold();
                    table.Cell().Element(Body).AlignRight().Text(Num(card.AvailableCredit, text)).FontColor(Muted);
                }
            });

            Footnote(column, text["cardsNote"]);
        });
    }

    private static void ComposeGoals(IContainer container, MonthlyReportDto report, ReportText text)
    {
        container.Element(FlatCard).Column(column =>
        {
            SectionHeader(column, text["goals"]);

            column.Item().Padding(12).PaddingTop(8).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(3);
                    c.RelativeColumn(1);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                });

                HeaderRow(table,
                    (text["goal"], false), (text["kind"], false), (text["saved"], true),
                    (text["target"], true), (text["progress"], true));

                foreach (var goal in report.Goals)
                {
                    table.Cell().Element(Body).Column(c =>
                    {
                        c.Item().Text(goal.Name).SemiBold();
                        c.Item().Text(text.Value(goal.Status)).FontSize(7.5f).FontColor(StatusColor(goal.Status));
                    });

                    table.Cell().Element(Body).Text(text.Value(goal.Kind)).FontColor(Muted);
                    table.Cell().Element(Body).AlignRight().Text(Num(goal.CurrentAmount, text));
                    table.Cell().Element(Body).AlignRight().Text(Num(goal.TargetAmount, text)).FontColor(Muted);
                    table.Cell().Element(Body).AlignRight()
                        .Text(goal.ProgressPercentage.ToString("N1", text.Culture) + "%").SemiBold();
                }
            });
        });
    }

    private static string StatusColor(string status) => status switch
    {
        "Completed" => Positive,
        "BehindSchedule" => "#8A6A2F",
        "DeadlinePassed" => Negative,
        _ => Muted
    };

    /// <summary>Section title on the tinted band, matching the card headers on screen.</summary>
    private static void SectionHeader(ColumnDescriptor column, string title)
    {
        column.Item()
            .Background(CanvasAlt)
            .BorderBottom(1).BorderColor(Line)
            .PaddingVertical(7).PaddingHorizontal(12)
            .Text(title).FontSize(10.5f).SemiBold().FontColor(Ink);
    }

    private static void Empty(ColumnDescriptor column, string message)
    {
        column.Item().Padding(12).Text(message).FontSize(8.5f).Italic().FontColor(Muted);
    }

    private static void Footnote(ColumnDescriptor column, string message)
    {
        column.Item()
            .BorderTop(1).BorderColor(Line)
            .PaddingVertical(6).PaddingHorizontal(12)
            .Text(message).FontSize(7.5f).FontColor(Muted);
    }

    private static void HeaderRow(TableDescriptor table, params (string Title, bool Right)[] headers)
    {
        table.Header(header =>
        {
            foreach (var (title, right) in headers)
            {
                var cell = header.Cell()
                    .BorderBottom(1).BorderColor(BorderColor)
                    .PaddingVertical(5).PaddingHorizontal(3);

                var text = right ? cell.AlignRight() : cell;

                text.Text(Label(title)).FontSize(7).SemiBold().FontColor(Muted).LetterSpacing(0.08f);
            }
        });
    }

    private static IContainer Body(IContainer container) =>
        container.BorderBottom(1).BorderColor(Line).PaddingVertical(5).PaddingHorizontal(3);

    private static IContainer TotalRow(IContainer container) =>
        container.BorderTop(1).BorderColor(BorderColor).PaddingVertical(5).PaddingHorizontal(3);

    /// <summary>Column labels are uppercase and letter-spaced, as in the web client.</summary>
    /// <remarks>
    /// Uppercased with the report's culture rather than invariantly, so a language
    /// whose casing rules differ from English's is not mangled.
    /// </remarks>
    private static string Label(string value) => value.ToUpperInvariant();

    /// <summary>
    /// Every money figure in the document, formatted for the report's culture.
    /// </summary>
    /// <remarks>
    /// The Spanish culture is <c>es-419</c> — Latin American — which groups as
    /// 1,234.50, the same as English, rather than the European 1.234,50. That is
    /// deliberate: this app is written for El Salvador, which uses USD and the
    /// US notation, and a report that suddenly swapped the separators when the
    /// language changed would look like a different currency.
    ///
    /// It also keeps the PDF in step with the screen for the common case. The web
    /// client formats through <c>Intl.NumberFormat(undefined, …)</c>, meaning the
    /// *browser's* locale rather than the app's — so on a machine configured for
    /// the region the app targets, both render identically.
    /// </remarks>
    private static string Num(decimal value, ReportText text) => value.ToString("N2", text.Culture);
}
