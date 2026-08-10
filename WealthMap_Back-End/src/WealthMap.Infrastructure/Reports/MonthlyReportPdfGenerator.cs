using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Application.Features.Reports.DTOs;

namespace WealthMap.Infrastructure.Reports;

/// <summary>
/// Renders an already-assembled report. All figures arrive computed — this class
/// decides layout only, never what a number means.
/// </summary>
public class MonthlyReportPdfGenerator : IPdfReportGenerator
{
    private static readonly string Accent = Colors.Blue.Darken2;
    private static readonly string Muted = Colors.Grey.Darken1;
    private static readonly string Rule = Colors.Grey.Lighten2;

    public byte[] GenerateMonthlyReport(MonthlyReportDto report)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(t => t.FontSize(10).FontFamily(Fonts.Calibri));

                page.Header().Element(h => ComposeHeader(h, report));
                page.Content().Element(c => ComposeContent(c, report));

                page.Footer().AlignCenter().Text(t =>
                {
                    t.DefaultTextStyle(s => s.FontSize(8).FontColor(Muted));
                    t.Span($"WealthMap · generated {report.GeneratedAt:yyyy-MM-dd HH:mm} UTC · page ");
                    t.CurrentPageNumber();
                    t.Span(" of ");
                    t.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, MonthlyReportDto report)
    {
        container.PaddingBottom(12).Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    left.Item().Text("Monthly report").FontSize(20).SemiBold().FontColor(Accent);
                    left.Item().Text($"{report.PeriodStart:MMMM yyyy}").FontSize(12).FontColor(Muted);
                });

                row.ConstantItem(180).AlignRight().Column(right =>
                {
                    right.Item().AlignRight().Text(report.UserFullName).SemiBold();
                    right.Item().AlignRight().Text($"{report.PeriodStart:yyyy-MM-dd} → {report.PeriodEnd:yyyy-MM-dd}")
                        .FontSize(9).FontColor(Muted);
                    right.Item().AlignRight().Text($"All amounts in {report.Currency}")
                        .FontSize(9).FontColor(Muted);
                });
            });

            column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Accent);
        });
    }

    private static void ComposeContent(IContainer container, MonthlyReportDto report)
    {
        container.Column(column =>
        {
            column.Spacing(16);

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
            row.RelativeItem().Element(e => Tile(e, "Income", report.Income.Total, report.Currency, Colors.Green.Darken2));
            row.RelativeItem().Element(e => Tile(e, "Spending", report.Spending.TotalPurchases, report.Currency, Colors.Red.Darken2));
            row.RelativeItem().Element(e => Tile(
                e,
                "Net result",
                report.NetResult,
                report.Currency,
                report.NetResult >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2));
        });
    }

    private static void Tile(IContainer container, string label, decimal value, string currency, string color)
    {
        container
            .Background(Colors.Grey.Lighten4)
            .Border(1).BorderColor(Rule)
            .Padding(10)
            .Column(c =>
            {
                c.Item().Text(label).FontSize(9).FontColor(Muted);
                c.Item().Text($"{value:N2} {currency}").FontSize(14).SemiBold().FontColor(color);
            });
    }

    private static void ComposeIncome(IContainer container, MonthlyReportDto report)
    {
        container.Column(column =>
        {
            SectionTitle(column, "Income");

            if (report.Income.Lines.Count == 0)
            {
                Empty(column, "No income recorded this month.");
            }
            else
            {
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        c.RelativeColumn(1);
                        c.RelativeColumn(2);
                    });

                    HeaderRow(table, "Type", "Count", "Total");

                    foreach (var line in report.Income.Lines)
                    {
                        table.Cell().Element(Body).Text(Humanize(line.Type));
                        table.Cell().Element(Body).AlignRight().Text(line.Count.ToString());
                        table.Cell().Element(Body).AlignRight().Text($"{line.Total:N2}");
                    }

                    table.Cell().Element(Total).Text("Total").SemiBold();
                    table.Cell().Element(Total).Text("");
                    table.Cell().Element(Total).AlignRight().Text($"{report.Income.Total:N2}").SemiBold();
                });
            }

            if (report.Income.ExpectedSalaryNet > 0)
                column.Item().PaddingTop(4).Text(
                    $"Expected net salary per month: {report.Income.ExpectedSalaryNet:N2} {report.Currency}")
                    .FontSize(8).FontColor(Muted);
        });
    }

    private static void ComposeSpending(IContainer container, MonthlyReportDto report)
    {
        container.Column(column =>
        {
            SectionTitle(column, "Spending by category");

            if (report.Spending.ByCategory.Count == 0)
            {
                Empty(column, "No purchases recorded this month.");
            }
            else
            {
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        c.RelativeColumn(1);
                        c.RelativeColumn(2);
                        c.RelativeColumn(1);
                    });

                    HeaderRow(table, "Category", "Items", "Total", "Share");

                    foreach (var category in report.Spending.ByCategory)
                    {
                        table.Cell().Element(Body).Text(category.Category);
                        table.Cell().Element(Body).AlignRight().Text(category.Count.ToString());
                        table.Cell().Element(Body).AlignRight().Text($"{category.Total:N2}");
                        table.Cell().Element(Body).AlignRight().Text($"{category.SharePercentage:N1}%");
                    }

                    table.Cell().Element(Total).Text("Total").SemiBold();
                    table.Cell().Element(Total).Text("");
                    table.Cell().Element(Total).AlignRight().Text($"{report.Spending.TotalPurchases:N2}").SemiBold();
                    table.Cell().Element(Total).Text("");
                });
            }

            if (report.Spending.TotalCashWithdrawn > 0)
                column.Item().PaddingTop(4).Text(
                    $"Cash withdrawn this month: {report.Spending.TotalCashWithdrawn:N2} {report.Currency} "
                    + "(left your accounts; excluded from the net result to avoid double counting cash purchases)")
                    .FontSize(8).FontColor(Muted);
        });
    }

    private static void ComposeTopExpenses(IContainer container, MonthlyReportDto report)
    {
        container.Column(column =>
        {
            SectionTitle(column, "Largest expenses");

            if (report.Spending.TopExpenses.Count == 0)
            {
                Empty(column, "Nothing to show.");
                return;
            }

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.ConstantColumn(70);
                    c.RelativeColumn(3);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                });

                HeaderRow(table, "Date", "Item", "Category", "Method", "Amount");

                foreach (var expense in report.Spending.TopExpenses)
                {
                    table.Cell().Element(Body).Text($"{expense.OccurredOn:MM-dd}");
                    table.Cell().Element(Body).Text(expense.ProductName).SemiBold();
                    table.Cell().Element(Body).Text(expense.Category);
                    table.Cell().Element(Body).Text(Humanize(expense.PaymentMethod));
                    table.Cell().Element(Body).AlignRight().Text($"{expense.Amount:N2}");
                }
            });
        });
    }

    private static void ComposeAccounts(IContainer container, MonthlyReportDto report)
    {
        container.Column(column =>
        {
            SectionTitle(column, "Accounts");

            if (report.Accounts.Count == 0)
            {
                Empty(column, "No accounts in this currency.");
                return;
            }

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(3);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                });

                HeaderRow(table, "Account", "Opening", "In", "Out", "Closing");

                foreach (var account in report.Accounts)
                {
                    table.Cell().Element(Body).Column(c =>
                    {
                        c.Item().Text(account.Name).SemiBold();
                        c.Item().Text($"{account.Type} · {account.MovementCount} movement(s)")
                            .FontSize(8).FontColor(Muted);
                    });

                    table.Cell().Element(Body).AlignRight().Text($"{account.OpeningBalance:N2}");
                    table.Cell().Element(Body).AlignRight().Text($"{account.TotalIn:N2}")
                        .FontColor(Colors.Green.Darken2);
                    table.Cell().Element(Body).AlignRight().Text($"{account.TotalOut:N2}")
                        .FontColor(Colors.Red.Darken2);
                    table.Cell().Element(Body).AlignRight().Text($"{account.ClosingBalance:N2}").SemiBold();
                }
            });
        });
    }

    private static void ComposeCards(IContainer container, MonthlyReportDto report)
    {
        container.Column(column =>
        {
            SectionTitle(column, "Credit cards");

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(3);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                });

                HeaderRow(table, "Card", "Charged", "Paid", "Owed", "Available");

                foreach (var card in report.Cards)
                {
                    table.Cell().Element(Body).Column(c =>
                    {
                        c.Item().Text(card.CardName).SemiBold();
                        c.Item().Text($"Due day {card.PaymentDueDay} · limit {card.CreditLimit:N2}")
                            .FontSize(8).FontColor(Muted);
                    });

                    table.Cell().Element(Body).AlignRight().Text($"{card.ChargedThisMonth:N2}");
                    table.Cell().Element(Body).AlignRight().Text($"{card.PaidThisMonth:N2}");
                    table.Cell().Element(Body).AlignRight().Text($"{card.UsedCredit:N2}").SemiBold();
                    table.Cell().Element(Body).AlignRight().Text($"{card.AvailableCredit:N2}");
                }
            });

            column.Item().PaddingTop(4).Text(
                "Card balances are current, not month-end. Paid includes payments from any source, "
                + "including cash and third parties.")
                .FontSize(8).FontColor(Muted);
        });
    }

    private static void ComposeGoals(IContainer container, MonthlyReportDto report)
    {
        container.Column(column =>
        {
            SectionTitle(column, "Goals");

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(3);
                    c.RelativeColumn(1);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                });

                HeaderRow(table, "Goal", "Kind", "Saved", "Target", "Progress");

                foreach (var goal in report.Goals)
                {
                    table.Cell().Element(Body).Column(c =>
                    {
                        c.Item().Text(goal.Name).SemiBold();
                        c.Item().Text(Humanize(goal.Status)).FontSize(8).FontColor(StatusColor(goal.Status));
                    });

                    table.Cell().Element(Body).Text(goal.Kind);
                    table.Cell().Element(Body).AlignRight().Text($"{goal.CurrentAmount:N2}");
                    table.Cell().Element(Body).AlignRight().Text($"{goal.TargetAmount:N2}");
                    table.Cell().Element(Body).AlignRight().Text($"{goal.ProgressPercentage:N1}%").SemiBold();
                }
            });
        });
    }

    private static string StatusColor(string status) => status switch
    {
        "Completed" => Colors.Green.Darken2,
        "BehindSchedule" => Colors.Orange.Darken2,
        "DeadlinePassed" => Colors.Red.Darken2,
        _ => Muted
    };

    private static void SectionTitle(ColumnDescriptor column, string title)
    {
        column.Item().PaddingBottom(4).Text(title).FontSize(12).SemiBold().FontColor(Accent);
    }

    private static void Empty(ColumnDescriptor column, string message)
    {
        column.Item().Text(message).FontSize(9).Italic().FontColor(Muted);
    }

    private static void HeaderRow(TableDescriptor table, params string[] headers)
    {
        table.Header(header =>
        {
            foreach (var title in headers)
            {
                var cell = header.Cell()
                    .BorderBottom(1).BorderColor(Rule)
                    .PaddingVertical(4).PaddingHorizontal(2);

                if (title is "Category" or "Account" or "Card" or "Goal" or "Type" or "Item" or "Date" or "Method" or "Kind")
                    cell.Text(title).FontSize(9).SemiBold().FontColor(Muted);
                else
                    cell.AlignRight().Text(title).FontSize(9).SemiBold().FontColor(Muted);
            }
        });
    }

    private static IContainer Body(IContainer container) =>
        container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(4).PaddingHorizontal(2);

    private static IContainer Total(IContainer container) =>
        container.BorderTop(1).BorderColor(Rule).PaddingVertical(4).PaddingHorizontal(2);

    /// <summary>PascalCase enum names read better spaced out in a document.</summary>
    private static string Humanize(string value) =>
        string.Concat(value.Select((c, i) => i > 0 && char.IsUpper(c) ? $" {c}" : c.ToString()));
}
