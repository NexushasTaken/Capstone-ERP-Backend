using ERP.Repository.Configuration.Enum;
using ERP.Repository.Configuration.Helper;
using ERP.Repository.ViewModel.Reports;
using ERP.Repository.ViewModel.Sales;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using static ERP.Repository.Services.Reports.ReportLayout;

namespace ERP.Repository.Services.Reports
{
    // Stock adjustments made in the period, by reason. Damaged customer returns are marked with their sale.
    public class AdjustmentReportDocument(AdjustmentReportViewModel report) : IDocument
    {
        private static readonly AdjustmentReasonEnum[] Reasons =
        [
            AdjustmentReasonEnum.Damaged,
            AdjustmentReasonEnum.Lost,
            AdjustmentReasonEnum.Found,
            AdjustmentReasonEnum.CountCorrection,
            AdjustmentReasonEnum.OpeningStock,
        ];

        public DocumentMetadata GetMetadata() =>
            new() { Title = $"{report.Period} Adjustments Report", Author = "Cpro" };

        public void Compose(IDocumentContainer container) =>
            Page(
                container,
                $"{report.Period} Adjustments Report",
                ReportPeriods.Text(report.Period, report.From, report.To),
                report,
                Content
            );

        private void Content(IContainer container)
        {
            var rows = report.Rows;
            var damaged = rows.Where(r => r.ReasonId == (int)AdjustmentReasonEnum.Damaged).ToList();

            container.Column(column =>
            {
                column.Spacing(18);

                column
                    .Item()
                    .Element(c =>
                        Summary(
                            c,
                            ("Adjustments", Count(rows.Count)),
                            ("Damaged", Count(damaged.Count)),
                            ("Lost", Count(rows.Count(r => r.ReasonId == (int)AdjustmentReasonEnum.Lost))),
                            ("Found", Count(rows.Count(r => r.ReasonId == (int)AdjustmentReasonEnum.Found))),
                            (
                                "Count corrections",
                                Count(rows.Count(r => r.ReasonId == (int)AdjustmentReasonEnum.CountCorrection))
                            )
                        )
                    );

                if (rows.Count == 0)
                {
                    Empty(column, "No stock adjustments in this period.");
                    return;
                }

                foreach (var reason in Reasons)
                {
                    var inReason = rows.Where(r => r.ReasonId == (int)reason).ToList();
                    if (inReason.Count == 0)
                    {
                        continue;
                    }

                    column
                        .Item()
                        .Column(section =>
                        {
                            SectionTitle(
                                section,
                                Capitalize(AdjustmentReasons.Label((int)reason)),
                                Hint(reason, inReason)
                            );
                            section.Item().Element(c => Table(c, inReason));
                        });
                }
            });
        }

        private static string? Hint(AdjustmentReasonEnum reason, List<AdjustmentReportRow> rows) =>
            reason switch
            {
                AdjustmentReasonEnum.Damaged =>
                    $"{Count(rows.Count(r => r.SaleId != null))} of these came back damaged from customer returns (marked with their sale)",
                AdjustmentReasonEnum.CountCorrection => "The difference between the stock and what was counted",
                AdjustmentReasonEnum.OpeningStock => "Starting quantity of new stock records",
                _ => null,
            };

        private void Table(IContainer container, List<AdjustmentReportRow> rows)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2.2f);
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(2.5f);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("When");
                    header.Cell().Element(HeaderCell).Text("Product");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Quantity");
                    header.Cell().Element(HeaderCell).Text("Note");
                    header.Cell().Element(HeaderCell).Text("By");
                });

                foreach (var row in rows)
                {
                    var when = row.Created_At is { } utc ? utc + DemandWeek.PhOffset : (DateTime?)null;
                    var whenText =
                        when == null ? ""
                        : report.Period == SaleReportPeriod.Daily ? $"{when:h:mm tt}"
                        : $"{when:MMM d, h:mm tt}";
                    var note = row.SaleId is int saleId
                        ? $"Customer return, SAL-{saleId}" + (string.IsNullOrEmpty(row.Note) ? "" : $": {row.Note}")
                        : row.Note ?? "";

                    table.Cell().Element(BodyCell).Text(whenText);
                    table.Cell().Element(BodyCell).Text(Capitalize(row.ProductName));
                    table
                        .Cell()
                        .Element(BodyCell)
                        .AlignRight()
                        .Text(Quantity(row.Quantity, row.Unit, signed: true))
                        .FontColor(row.Quantity < 0 ? Colors.Red.Darken2 : Colors.Green.Darken3);
                    table.Cell().Element(BodyCell).Text(note);
                    table.Cell().Element(BodyCell).Text(row.By ?? "");
                }
            });
        }
    }
}
