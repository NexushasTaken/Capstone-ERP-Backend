using ERP.Repository.Configuration.Helper;
using ERP.Repository.ViewModel.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using static ERP.Repository.Services.Reports.ReportLayout;

namespace ERP.Repository.Services.Reports
{
    // Purchases received in the period: what came in, from whom, and what it cost.
    public class PurchaseReportDocument(PurchaseReportViewModel report) : IDocument
    {
        public DocumentMetadata GetMetadata() => new() { Title = $"{report.Period} Purchases Report", Author = "Cpro" };

        public void Compose(IDocumentContainer container) =>
            Page(
                container,
                $"{report.Period} Purchases Report",
                ReportPeriods.Text(report.Period, report.From, report.To),
                report,
                Content
            );

        private void Content(IContainer container)
        {
            var purchases = report.Purchases;
            var shortLines = purchases.Sum(p => p.Lines.Count(l => l.Received < l.Ordered));
            var leadTimes = purchases.Select(p => p.ReceivedOn.DayNumber - p.OrderedOn.DayNumber).ToList();

            container.Column(column =>
            {
                column.Spacing(18);

                column
                    .Item()
                    .Element(c =>
                        Summary(
                            c,
                            ("Purchases received", Count(purchases.Count)),
                            ("Total cost", Money(purchases.Sum(p => p.ReceivedTotal))),
                            ("Short lines", Count(shortLines)),
                            ("Average lead time", leadTimes.Count == 0 ? "—" : $"{leadTimes.Average():0.#} days")
                        )
                    );

                if (purchases.Count == 0)
                {
                    Empty(column, "No purchases were received in this period.");
                    return;
                }

                column
                    .Item()
                    .Column(section =>
                    {
                        SectionTitle(section, "Purchases", "By received date; cost is for what arrived");
                        section.Item().Element(PurchasesTable);
                    });

                column
                    .Item()
                    .Column(section =>
                    {
                        SectionTitle(section, "By product");
                        section.Item().Element(ProductsTable);
                    });
            });
        }

        private void PurchasesTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1.5f);
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1.5f);
                    columns.RelativeColumn(2.5f);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("Received");
                    header.Cell().Element(HeaderCell).Text("PO");
                    header.Cell().Element(HeaderCell).Text("Supplier");
                    header.Cell().Element(HeaderCell).Text("Ordered");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Items");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Lead time");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Cost");
                });

                foreach (var purchase in report.Purchases)
                {
                    var days = purchase.ReceivedOn.DayNumber - purchase.OrderedOn.DayNumber;

                    table.Cell().Element(BodyCell).Text($"{purchase.ReceivedOn:MMM d}");
                    table.Cell().Element(BodyCell).Text($"PO-{purchase.Id}");
                    table.Cell().Element(BodyCell).Text(Capitalize(purchase.SupplierName ?? "no supplier"));
                    table.Cell().Element(BodyCell).Text($"{purchase.OrderedOn:MMM d}");
                    table.Cell().Element(BodyCell).AlignRight().Text(Count(purchase.Lines.Count));
                    table.Cell().Element(BodyCell).AlignRight().Text($"{days} d");
                    table.Cell().Element(BodyCell).AlignRight().Text(Money(purchase.ReceivedTotal));
                }

                table.Cell().ColumnSpan(6).Element(TotalCell).Text("Total");
                table.Cell().Element(TotalCell).AlignRight().Text(Money(report.Purchases.Sum(p => p.ReceivedTotal)));
            });
        }

        private void ProductsTable(IContainer container)
        {
            var products = report
                .Purchases.SelectMany(p => p.Lines)
                .GroupBy(l => l.ProductId)
                .Select(g => new
                {
                    Name = g.First().ProductName,
                    Unit = g.First().Unit,
                    Ordered = g.Sum(l => l.Ordered),
                    Received = g.Sum(l => l.Received),
                    Cost = g.Sum(l => l.Cost),
                })
                .OrderByDescending(p => p.Cost)
                .ThenBy(p => p.Name)
                .ToList();

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(5);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("Product");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Ordered");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Received");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Cost");
                });

                foreach (var product in products)
                {
                    table.Cell().Element(BodyCell).Text(Capitalize(product.Name));
                    table.Cell().Element(BodyCell).AlignRight().Text(Quantity(product.Ordered, product.Unit));
                    var received = table
                        .Cell()
                        .Element(BodyCell)
                        .AlignRight()
                        .Text(Quantity(product.Received, product.Unit));
                    if (product.Received < product.Ordered)
                    {
                        received.FontColor(Colors.Orange.Darken3);
                    }
                    table.Cell().Element(BodyCell).AlignRight().Text(Money(product.Cost));
                }
            });
        }
    }
}
