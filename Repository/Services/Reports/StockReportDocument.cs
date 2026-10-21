using ERP.Repository.Configuration.Enum;
using ERP.Repository.ViewModel.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using static ERP.Repository.Services.Reports.ReportLayout;

namespace ERP.Repository.Services.Reports
{
    // Stock on hand right now, the items that need restocking first.
    public class StockReportDocument(StockReportViewModel report) : IDocument
    {
        public DocumentMetadata GetMetadata() => new() { Title = "Stock on Hand Report", Author = "Cpro" };

        public void Compose(IDocumentContainer container) =>
            Page(container, "Stock on Hand", $"As of {report.GeneratedAt:MMMM d, yyyy h:mm tt}", report, Content);

        private int CountOf(InventoryStatusEnum status) => report.Rows.Count(r => r.StatusId == (int)status);

        private void Content(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(18);

                column
                    .Item()
                    .Element(c =>
                        Summary(
                            c,
                            ("Items", Count(report.Rows.Count)),
                            ("Critical", Count(CountOf(InventoryStatusEnum.Critical))),
                            ("Low stock", Count(CountOf(InventoryStatusEnum.LowStock))),
                            ("Available", Count(CountOf(InventoryStatusEnum.Available)))
                        )
                    );

                if (report.Rows.Count == 0)
                {
                    Empty(column, "No stock records.");
                    return;
                }

                column
                    .Item()
                    .Column(section =>
                    {
                        SectionTitle(
                            section,
                            "Stock",
                            "Critical: below the reorder point. Low stock: under twice the reorder point."
                        );
                        section.Item().Element(Table);
                    });
            });
        }

        private void Table(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);
                    columns.RelativeColumn(5);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("#");
                    header.Cell().Element(HeaderCell).Text("Product");
                    header.Cell().Element(HeaderCell).Text("Category");
                    header.Cell().Element(HeaderCell).AlignRight().Text("On hand");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Reorder point");
                    header.Cell().Element(HeaderCell).Text("Status");
                });

                var rank = 1;
                foreach (var row in report.Rows)
                {
                    var color = row.StatusId switch
                    {
                        (int)InventoryStatusEnum.Critical => Colors.Red.Darken2,
                        (int)InventoryStatusEnum.LowStock => Colors.Orange.Darken3,
                        _ => Colors.Green.Darken3,
                    };

                    table.Cell().Element(BodyCell).Text($"{rank++}");
                    table.Cell().Element(BodyCell).Text(Capitalize(row.ProductName));
                    table.Cell().Element(BodyCell).Text(Capitalize(row.Category ?? "uncategorized"));
                    table.Cell().Element(BodyCell).AlignRight().Text(Quantity(row.Quantity, row.Unit));
                    table.Cell().Element(BodyCell).AlignRight().Text(Quantity(row.ReorderPoint, row.Unit));
                    table.Cell().Element(BodyCell).Text(Capitalize(row.Status)).FontColor(color).SemiBold();
                }
            });
        }
    }
}
