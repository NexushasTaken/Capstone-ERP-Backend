using System.Globalization;
using ERP.Repository.Configuration.Helper;
using ERP.Repository.ViewModel.Sales;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ERP.Repository.Services.Sales
{
    // Layout of the Sales PDF report. It only draws what SaleService already worked out.
    public class SalesReportDocument(SaleReportViewModel report) : IDocument
    {
        private static readonly CultureInfo Peso = CultureInfo.GetCultureInfo("en-PH");

        public DocumentMetadata GetMetadata() => new() { Title = $"{report.Period} Sales Report", Author = "Cpro" };

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(ComposeHeader);
                page.Content().PaddingVertical(16).Element(ComposeContent);
                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Darken1));
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten1)
                .PaddingBottom(8)
                .Row(row =>
                {
                    row.RelativeItem()
                        .Column(column =>
                        {
                            column.Item().Text("Cpro").FontSize(9).FontColor(Colors.Grey.Darken1);
                            column.Item().Text($"{report.Period} Sales Report").FontSize(18).Bold();
                            column.Item().Text(PeriodText()).FontSize(11);
                        });

                    row.AutoItem()
                        .AlignBottom()
                        .AlignRight()
                        .Column(column =>
                        {
                            column
                                .Item()
                                .AlignRight()
                                .Text($"Generated {report.GeneratedAt:MMM d, yyyy h:mm tt}")
                                .FontSize(8)
                                .FontColor(Colors.Grey.Darken1);
                            if (report.GeneratedBy != "")
                            {
                                column
                                    .Item()
                                    .AlignRight()
                                    .Text($"by {report.GeneratedBy}")
                                    .FontSize(8)
                                    .FontColor(Colors.Grey.Darken1);
                            }
                        });
                });
        }

        private void ComposeContent(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(18);

                column.Item().Element(ComposeSummary);

                if (report.SalesCount == 0)
                {
                    column.Item().Text("No sales in this period.").Italic().FontColor(Colors.Grey.Darken1);
                    return;
                }

                if (report.Period == SaleReportPeriod.Daily)
                {
                    column.Item().Element(ComposeSalesTable);
                }
                else
                {
                    column.Item().Element(ComposePeriodTable);
                }

                column.Item().Element(ComposeTopProducts);
            });
        }

        private void ComposeSummary(IContainer container)
        {
            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Background(Colors.Grey.Lighten4)
                .Padding(10)
                .Row(row =>
                {
                    SummaryItem(row, "Sales", report.SalesCount.ToString("N0", Peso));
                    SummaryItem(row, "Subtotal", Money(report.Gross));
                    SummaryItem(row, "Discounts", Money(report.Discount));
                    SummaryItem(row, "Total", Money(report.Net));
                    SummaryItem(row, "Refunded", Money(report.Refunded));
                    SummaryItem(row, "Total after refunds", Money(report.AfterRefunds));
                    SummaryItem(row, "Average sale", Money(report.Average));
                });
        }

        private static void SummaryItem(RowDescriptor row, string label, string value)
        {
            row.RelativeItem()
                .Column(column =>
                {
                    column.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Darken1);
                    column.Item().Text(value).FontSize(12).SemiBold();
                });
        }

        // Daily report: one row per sale.
        private void ComposeSalesTable(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().PaddingBottom(4).Text("Sales").FontSize(12).SemiBold();
                column
                    .Item()
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50);
                            columns.ConstantColumn(50);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCell).Text("Time");
                            header.Cell().Element(HeaderCell).Text("Sale ID");
                            header.Cell().Element(HeaderCell).Text("Customer");
                            header.Cell().Element(HeaderCell).Text("Type");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Subtotal");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Discount");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Total");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Refunded");
                            header.Cell().Element(HeaderCell).AlignRight().Text("After refunds");
                        });

                        foreach (var sale in report.Sales)
                        {
                            var soldAt = sale.Created_At is { } utc
                                ? (utc + DemandWeek.PhOffset).ToString("h:mm tt")
                                : "";

                            table.Cell().Element(BodyCell).Text(soldAt);
                            table.Cell().Element(BodyCell).Text($"SAL-{sale.Id}");
                            table.Cell().Element(BodyCell).Text(sale.CustomerName ?? "");
                            table.Cell().Element(BodyCell).Text(Capitalize(sale.OrderType));
                            table.Cell().Element(BodyCell).AlignRight().Text(Money(sale.Subtotal));
                            table.Cell().Element(BodyCell).AlignRight().Text(Money(sale.Subtotal - sale.Total));
                            table.Cell().Element(BodyCell).AlignRight().Text(Money(sale.Total));
                            table.Cell().Element(BodyCell).AlignRight().Text(Money(sale.Refunded));
                            table.Cell().Element(BodyCell).AlignRight().Text(Money(sale.Total - sale.Refunded));
                        }

                        table.Cell().ColumnSpan(4).Element(TotalCell).Text("Total");
                        table.Cell().Element(TotalCell).AlignRight().Text(Money(report.Gross));
                        table.Cell().Element(TotalCell).AlignRight().Text(Money(report.Discount));
                        table.Cell().Element(TotalCell).AlignRight().Text(Money(report.Net));
                        table.Cell().Element(TotalCell).AlignRight().Text(Money(report.Refunded));
                        table.Cell().Element(TotalCell).AlignRight().Text(Money(report.AfterRefunds));
                    });
            });
        }

        // Weekly report: one row per day. Monthly report: one row per week.
        private void ComposePeriodTable(IContainer container)
        {
            container.Column(column =>
            {
                var title = report.Period == SaleReportPeriod.Weekly ? "By day" : "By week";
                column.Item().PaddingBottom(4).Text(title).FontSize(12).SemiBold();
                column
                    .Item()
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header
                                .Cell()
                                .Element(HeaderCell)
                                .Text(report.Period == SaleReportPeriod.Weekly ? "Day" : "Week");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Sales");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Subtotal");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Discounts");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Total");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Refunded");
                            header.Cell().Element(HeaderCell).AlignRight().Text("After refunds");
                        });

                        foreach (var row in report.Rows)
                        {
                            table.Cell().Element(BodyCell).Text(row.Label);
                            table.Cell().Element(BodyCell).AlignRight().Text(row.Sales.ToString("N0", Peso));
                            table.Cell().Element(BodyCell).AlignRight().Text(Money(row.Gross));
                            table.Cell().Element(BodyCell).AlignRight().Text(Money(row.Discount));
                            table.Cell().Element(BodyCell).AlignRight().Text(Money(row.Net));
                            table.Cell().Element(BodyCell).AlignRight().Text(Money(row.Refunded));
                            table.Cell().Element(BodyCell).AlignRight().Text(Money(row.AfterRefunds));
                        }

                        table.Cell().Element(TotalCell).Text("Total");
                        table.Cell().Element(TotalCell).AlignRight().Text(report.SalesCount.ToString("N0", Peso));
                        table.Cell().Element(TotalCell).AlignRight().Text(Money(report.Gross));
                        table.Cell().Element(TotalCell).AlignRight().Text(Money(report.Discount));
                        table.Cell().Element(TotalCell).AlignRight().Text(Money(report.Net));
                        table.Cell().Element(TotalCell).AlignRight().Text(Money(report.Refunded));
                        table.Cell().Element(TotalCell).AlignRight().Text(Money(report.AfterRefunds));
                    });
            });
        }

        private void ComposeTopProducts(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("Top products").FontSize(12).SemiBold();
                column
                    .Item()
                    .PaddingBottom(4)
                    .Text("By revenue, before order discounts")
                    .FontSize(8)
                    .FontColor(Colors.Grey.Darken1);
                column
                    .Item()
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(25);
                            columns.RelativeColumn(5);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCell).Text("#");
                            header.Cell().Element(HeaderCell).Text("Product");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Qty sold");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Revenue");
                        });

                        var rank = 1;
                        foreach (var product in report.TopProducts)
                        {
                            table.Cell().Element(BodyCell).Text($"{rank++}");
                            table.Cell().Element(BodyCell).Text(product.Name ?? "");
                            table.Cell().Element(BodyCell).AlignRight().Text(product.Quantity.ToString("N0", Peso));
                            table.Cell().Element(BodyCell).AlignRight().Text(Money(product.Revenue));
                        }
                    });
            });
        }

        private string PeriodText() =>
            report.Period switch
            {
                SaleReportPeriod.Daily => $"{report.From:dddd, MMMM d, yyyy}",
                SaleReportPeriod.Monthly => $"{report.From:MMMM yyyy}",
                _ when report.From.Year == report.To.Year => $"{report.From:MMM d} – {report.To:MMM d, yyyy}",
                _ => $"{report.From:MMM d, yyyy} – {report.To:MMM d, yyyy}",
            };

        private static string Money(decimal amount) => amount.ToString("C2", Peso);

        private static string Capitalize(string? value) =>
            string.IsNullOrEmpty(value) ? "" : char.ToUpperInvariant(value[0]) + value[1..];

        private static IContainer HeaderCell(IContainer container) =>
            container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Darken1)
                .PaddingVertical(4)
                .PaddingHorizontal(2)
                .DefaultTextStyle(x => x.SemiBold().FontSize(9));

        private static IContainer BodyCell(IContainer container) =>
            container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(3).PaddingHorizontal(2);

        private static IContainer TotalCell(IContainer container) =>
            container
                .BorderTop(1)
                .BorderColor(Colors.Grey.Darken1)
                .PaddingVertical(4)
                .PaddingHorizontal(2)
                .DefaultTextStyle(x => x.Bold());
    }
}
