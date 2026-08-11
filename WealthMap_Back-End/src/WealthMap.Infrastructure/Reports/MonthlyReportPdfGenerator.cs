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

    public byte[] GenerateMonthlyReport(MonthlyReportDto report)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.4f, Unit.Centimetre);
                page.PageColor(Canvas);
                page.DefaultTextStyle(t => t.FontSize(9.5f).FontFamily(Font).FontColor(Ink));

                page.Header().Element(h => ComposeHeader(h, report));
                page.Content().PaddingTop(14).Element(c => ComposeContent(c, report));

                page.Footer().PaddingTop(8).AlignCenter().Text(t =>
                {
                    t.DefaultTextStyle(s => s.FontSize(7.5f).FontColor(Subtle));
                    t.Span($"WealthMap · generated {report.GeneratedAt:yyyy-MM-dd HH:mm} UTC · page ");
                    t.CurrentPageNumber();
                    t.Span(" of ");
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

    private static void ComposeHeader(IContainer container, MonthlyReportDto report)
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
                        .Text("Monthly report").FontSize(21).SemiBold().FontColor(Ink);

                    left.Item().Text($"{report.PeriodStart:MMMM yyyy}")
                        .FontSize(11).FontColor(Muted);
                });

                row.ConstantItem(170).AlignRight().Column(right =>
                {
                    right.Item().AlignRight().Text(report.UserFullName).FontSize(10).SemiBold();
                    right.Item().AlignRight()
                        .Text($"{report.PeriodStart:yyyy-MM-dd} → {report.PeriodEnd:yyyy-MM-dd}")
                        .FontSize(8).FontColor(Muted);
                    right.Item().AlignRight()
                        .Text($"All amounts in {report.Currency}")
                        .FontSize(8).FontColor(Muted);
                });
            });

            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(BorderColor);
        });
    }

    private static void ComposeContent(IContainer container, MonthlyReportDto report)
    {
        container.Column(column =>
        {
            column.Spacing(14);

            column.Item().Element(e => ComposeSummary(e, report));
            column.Item().Element(e => ComposeIncome(e, report));
            column.Item().Element(e => ComposeSpending(e, report));
            column.Item().Element(e => ComposeTopExpenses(e, report));
            column.Item().Element(e => ComposeAccounts(e, report));

            if (report.Cards.Count > 0)
                column.Item().Element(e => ComposeCards(e, report));

            if (report.Goals.Count > 0)
                column.Item().Element(e => ComposeGoals(e, report));
        });
    }

    private static void ComposeSummary(IContainer container, MonthlyReportDto report)
    {
        container.Row(row =>
        {
            row.Spacing(10);
            row.RelativeItem().Element(e => Tile(e, "Income", report.Income.Total, report.Currency, Positive));
            row.RelativeItem().Element(e => Tile(e, "Spending", report.Spending.TotalPurchases, report.Currency, Negative));
            row.RelativeItem().Element(e => Tile(
                e,
                "Net result",
                report.NetResult,
                report.Currency,
                report.NetResult >= 0 ? Positive : Negative));
        });
    }

    private static void Tile(IContainer container, string label, decimal value, string currency, string color)
    {
        container.Element(FlatCard).Padding(11).Column(c =>
        {
            c.Item().Text(Label(label)).FontSize(7).SemiBold().FontColor(Muted).LetterSpacing(0.08f);
            c.Item().PaddingTop(4)
                .Text($"{value:N2} {currency}").FontSize(15).SemiBold().FontColor(color);
        });
    }

    private static void ComposeIncome(IContainer container, MonthlyReportDto report)
    {
        container.Element(FlatCard).Column(column =>
        {
            SectionHeader(column, "Income");

            if (report.Income.Lines.Count == 0)
            {
                Empty(column, "No income recorded this month.");
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

                    HeaderRow(table, ("Type", false), ("Count", true), ("Total", true));

                    foreach (var line in report.Income.Lines)
                    {
                        table.Cell().Element(Body).Text(Humanize(line.Type));
                        table.Cell().Element(Body).AlignRight().Text(line.Count.ToString());
                        table.Cell().Element(Body).AlignRight().Text($"{line.Total:N2}");
                    }

                    table.Cell().Element(TotalRow).Text("Total").SemiBold();
                    table.Cell().Element(TotalRow).Text("");
                    table.Cell().Element(TotalRow).AlignRight().Text($"{report.Income.Total:N2}").SemiBold();
                });
            }

            if (report.Income.ExpectedSalaryNet > 0)
                Footnote(column,
                    $"Expected net salary {report.Income.ExpectedSalaryNet:N2} {report.Currency} per month.");
        });
    }

    private static void ComposeSpending(IContainer container, MonthlyReportDto report)
    {
        container.Element(FlatCard).Column(column =>
        {
            SectionHeader(column, "Spending by category");

            if (report.Spending.ByCategory.Count == 0)
            {
                Empty(column, "No purchases recorded this month.");
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

                    HeaderRow(table, ("Category", false), ("Items", true), ("Total", true), ("Share", true));

                    foreach (var category in report.Spending.ByCategory)
                    {
                        table.Cell().Element(Body).Text(category.Category);
                        table.Cell().Element(Body).AlignRight().Text(category.Count.ToString());
                        table.Cell().Element(Body).AlignRight().Text($"{category.Total:N2}");
                        table.Cell().Element(Body).AlignRight().Text($"{category.SharePercentage:N1}%");
                    }

                    table.Cell().Element(TotalRow).Text("Total").SemiBold();
                    table.Cell().Element(TotalRow).Text("");
                    table.Cell().Element(TotalRow).AlignRight().Text($"{report.Spending.TotalPurchases:N2}").SemiBold();
                    table.Cell().Element(TotalRow).Text("");
                });
            }

            if (report.Spending.TotalCashWithdrawn > 0)
                Footnote(column,
                    $"Cash withdrawn this month: {report.Spending.TotalCashWithdrawn:N2} {report.Currency}. "
                    + "It left your accounts but is excluded from the net result — cash purchases already cover it.");
        });
    }

    private static void ComposeTopExpenses(IContainer container, MonthlyReportDto report)
    {
        container.Element(FlatCard).Column(column =>
        {
            SectionHeader(column, "Largest expenses");

            if (report.Spending.TopExpenses.Count == 0)
            {
                Empty(column, "Nothing to show.");
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
                    ("Date (UTC)", false), ("Item", false), ("Category", false), ("Method", false), ("Amount", true));

                var isFirst = true;

                foreach (var expense in report.Spending.TopExpenses)
                {
                    // The biggest expense is the one worth noticing, as on screen.
                    var tint = isFirst ? CanvasAlt : Surface;

                    // UTC like every other time in this document — the footer says so,
                    // and the report's month is bounded in UTC too, so a local-time
                    // reading here could show a date outside the month it sits in.
                    table.Cell().Background(tint).Element(Body).Text($"{expense.OccurredAt:MM-dd HH:mm}");

                    // The store sits under the item rather than in its own column:
                    // a sixth column would squeeze the four that carry the numbers.
                    table.Cell().Background(tint).Element(Body).Column(item =>
                    {
                        item.Item().Text(expense.ProductName).SemiBold();

                        if (!string.IsNullOrWhiteSpace(expense.StoreName))
                            item.Item().Text(expense.StoreName).FontSize(7.5f).FontColor(Muted);
                    });

                    table.Cell().Background(tint).Element(Body).Text(expense.Category);
                    table.Cell().Background(tint).Element(Body).Text(Humanize(expense.PaymentMethod));
                    table.Cell().Background(tint).Element(Body).AlignRight()
                        .Text($"{expense.Amount:N2}").SemiBold().FontColor(isFirst ? Ink : Ink);

                    isFirst = false;
                }
            });
        });
    }

    private static void ComposeAccounts(IContainer container, MonthlyReportDto report)
    {
        container.Element(FlatCard).Column(column =>
        {
            SectionHeader(column, "Accounts");

            if (report.Accounts.Count == 0)
            {
                Empty(column, "No accounts in this currency.");
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
                    ("Account", false), ("Opening", true), ("In", true), ("Out", true), ("Closing", true));

                foreach (var account in report.Accounts)
                {
                    table.Cell().Element(Body).Column(c =>
                    {
                        c.Item().Text(account.Name).SemiBold();
                        c.Item().Text($"{account.Type} · {account.MovementCount} movement(s)")
                            .FontSize(7.5f).FontColor(Muted);
                    });

                    table.Cell().Element(Body).AlignRight().Text($"{account.OpeningBalance:N2}").FontColor(Muted);
                    table.Cell().Element(Body).AlignRight().Text($"{account.TotalIn:N2}").FontColor(Positive);
                    table.Cell().Element(Body).AlignRight().Text($"{account.TotalOut:N2}").FontColor(Negative);
                    table.Cell().Element(Body).AlignRight().Text($"{account.ClosingBalance:N2}").SemiBold();
                }
            });
        });
    }

    private static void ComposeCards(IContainer container, MonthlyReportDto report)
    {
        container.Element(FlatCard).Column(column =>
        {
            SectionHeader(column, "Credit cards");

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
                    ("Card", false), ("Charged", true), ("Paid", true), ("Owed", true), ("Available", true));

                foreach (var card in report.Cards)
                {
                    table.Cell().Element(Body).Column(c =>
                    {
                        c.Item().Text(card.CardName).SemiBold();
                        c.Item().Text($"Due day {card.PaymentDueDay} · limit {card.CreditLimit:N2}")
                            .FontSize(7.5f).FontColor(Muted);
                    });

                    table.Cell().Element(Body).AlignRight().Text($"{card.ChargedThisMonth:N2}").FontColor(Negative);
                    table.Cell().Element(Body).AlignRight().Text($"{card.PaidThisMonth:N2}").FontColor(Positive);
                    table.Cell().Element(Body).AlignRight().Text($"{card.UsedCredit:N2}").SemiBold();
                    table.Cell().Element(Body).AlignRight().Text($"{card.AvailableCredit:N2}").FontColor(Muted);
                }
            });

            Footnote(column,
                "Card balances are current, not month-end. Paid includes payments from any source, "
                + "including cash and third parties.");
        });
    }

    private static void ComposeGoals(IContainer container, MonthlyReportDto report)
    {
        container.Element(FlatCard).Column(column =>
        {
            SectionHeader(column, "Goals");

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
                    ("Goal", false), ("Kind", false), ("Saved", true), ("Target", true), ("Progress", true));

                foreach (var goal in report.Goals)
                {
                    table.Cell().Element(Body).Column(c =>
                    {
                        c.Item().Text(goal.Name).SemiBold();
                        c.Item().Text(Humanize(goal.Status)).FontSize(7.5f).FontColor(StatusColor(goal.Status));
                    });

                    table.Cell().Element(Body).Text(goal.Kind).FontColor(Muted);
                    table.Cell().Element(Body).AlignRight().Text($"{goal.CurrentAmount:N2}");
                    table.Cell().Element(Body).AlignRight().Text($"{goal.TargetAmount:N2}").FontColor(Muted);
                    table.Cell().Element(Body).AlignRight().Text($"{goal.ProgressPercentage:N1}%").SemiBold();
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
    private static string Label(string value) => value.ToUpperInvariant();

    /// <summary>PascalCase enum names read better spaced out in a document.</summary>
    private static string Humanize(string value) =>
        string.Concat(value.Select((c, i) => i > 0 && char.IsUpper(c) ? $" {c}" : c.ToString()));
}
