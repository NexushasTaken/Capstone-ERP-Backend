using System.Globalization;
using ERP.Repository.ViewModel.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ERP.Repository.Services.Reports
{
    // The look every Reports PDF shares (the same as the Sales PDF): A4, title block, page numbers, table cells.
    public static class ReportLayout
    {
        public static readonly CultureInfo Peso = CultureInfo.GetCultureInfo("en-PH");

        public static void Page(
            IDocumentContainer container,
            string title,
            string subtitle,
            ReportHeader header,
            Action<IContainer> content
        )
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => Header(c, title, subtitle, header));
                page.Content().PaddingVertical(16).Element(c => content(c));
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

        private static void Header(IContainer container, string title, string subtitle, ReportHeader header)
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
                            column.Item().Text(title).FontSize(18).Bold();
                            column.Item().Text(subtitle).FontSize(11);
                        });

                    row.AutoItem()
                        .AlignBottom()
                        .AlignRight()
                        .Column(column =>
                        {
                            column
                                .Item()
                                .AlignRight()
                                .Text($"Generated {header.GeneratedAt:MMM d, yyyy h:mm tt}")
                                .FontSize(8)
                                .FontColor(Colors.Grey.Darken1);
                            if (header.GeneratedBy != "")
                            {
                                column
                                    .Item()
                                    .AlignRight()
                                    .Text($"by {header.GeneratedBy}")
                                    .FontSize(8)
                                    .FontColor(Colors.Grey.Darken1);
                            }
                        });
                });
        }

        // The grey box of headline numbers at the top of a report.
        public static void Summary(IContainer container, params (string Label, string Value)[] items)
        {
            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Background(Colors.Grey.Lighten4)
                .Padding(10)
                .Row(row =>
                {
                    foreach (var (label, value) in items)
                    {
                        row.RelativeItem()
                            .Column(column =>
                            {
                                column.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Darken1);
                                column.Item().Text(value).FontSize(12).SemiBold();
                            });
                    }
                });
        }

        public static void SectionTitle(ColumnDescriptor column, string title, string? hint = null)
        {
            column.Item().PaddingBottom(hint == null ? 4 : 0).Text(title).FontSize(12).SemiBold();
            if (hint != null)
            {
                column.Item().PaddingBottom(4).Text(hint).FontSize(8).FontColor(Colors.Grey.Darken1);
            }
        }

        public static void Empty(ColumnDescriptor column, string text) =>
            column.Item().Text(text).Italic().FontColor(Colors.Grey.Darken1);

        public static string Money(decimal amount) => amount.ToString("C2", Peso);

        public static string Count(int value) => value.ToString("N0", Peso);

        // "12 box", "-3 pc", "+5 pc" with signed
        public static string Quantity(int quantity, string? unit, bool signed = false)
        {
            var amount = signed && quantity > 0 ? $"+{Count(quantity)}" : Count(quantity);
            return string.IsNullOrEmpty(unit) ? amount : $"{amount} {unit}";
        }

        public static string Capitalize(string? value) =>
            string.IsNullOrEmpty(value) ? "" : char.ToUpperInvariant(value[0]) + value[1..];

        public static IContainer HeaderCell(IContainer container) =>
            container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Darken1)
                .PaddingVertical(4)
                .PaddingHorizontal(2)
                .DefaultTextStyle(x => x.SemiBold().FontSize(9));

        public static IContainer BodyCell(IContainer container) =>
            container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(3).PaddingHorizontal(2);

        public static IContainer TotalCell(IContainer container) =>
            container
                .BorderTop(1)
                .BorderColor(Colors.Grey.Darken1)
                .PaddingVertical(4)
                .PaddingHorizontal(2)
                .DefaultTextStyle(x => x.Bold());
    }
}
