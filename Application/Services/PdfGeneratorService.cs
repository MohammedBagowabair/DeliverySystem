using Application.Helpers;
using Application.Interfaces;
using Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace Application.Services
{
    public class PdfGeneratorService : IPdfGeneratorService
    {

        public byte[] GenerateOrderPdf(List<Order> orders, string reportMessage)
        {
            // Set QuestPDF license (Community version is free)
            QuestPDF.Settings.License = LicenseType.Community;

            try
            {
                // Get current date and time
                string currentDateTime = DateTime.Now.ToString("dddd, dd MMMM yyyy - hh:mm tt", new CultureInfo("en-US"));

                // Calculate totals
                int totalOrders = orders.Count;
                decimal totalRevenue = orders.Sum(o => o.FinalPrice);
                decimal companyProfit = totalRevenue * 0.7m; // Example calculation
                decimal driversProfit = totalRevenue * 0.3m; // Example calculation

                return Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A3); // Set page size to A3
                        page.Margin(30);
                        page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(14)); // Default text style

                        // Header Section
                        page.Header().Row(row =>
                        {
                            row.RelativeColumn().AlignLeft().Column(col =>
                            {
                                col.Item().Text("\u200F" + currentDateTime).FontSize(14).Italic().FontColor(Colors.Grey.Darken1);
                                col.Item().Text("\u200F" + "السائق: " + "Mohammed").FontSize(16).Bold().FontColor(Colors.Black);
                            });

                            row.RelativeColumn().AlignRight().Column(col =>
                            {
                                //col.Item().Image("path/to/company/logo.png", ImageScaling.FitArea); // Add company logo
                                col.Item().Text("جو دليفري").FontSize(30).Bold().FontColor(Colors.Blue.Darken3);
                            });
                        });

                        // Table Section
                        page.Content().Column(col =>
                        {
                            col.Item().Table(table =>
                            {
                                // Define table columns
                                table.ColumnsDefinition(columns =>
                                {
                                    for (int i = 0; i < 8; i++) columns.RelativeColumn();
                                });

                                // Table Header
                                table.Header(header =>
                                {
                                    header.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(4).AlignRight().Text("\u200Fالعنوان").Bold();
                                    header.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(4).AlignRight().Text("\u200Fطريقة الدفع").Bold();
                                    header.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(4).AlignRight().Text("\u200Fالسعر النهائي").Bold();
                                    header.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(4).AlignRight().Text("\u200Fالوقت").Bold();
                                    header.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(4).AlignRight().Text("\u200Fالحالة").Bold();
                                    header.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(4).AlignRight().Text("\u200Fالعميل").Bold();
                                    header.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(4).AlignRight().Text("\u200Fالسائق").Bold();
                                    header.Cell().Border(0.5f).BorderColor(Colors.Black).Padding(4).AlignRight().Text("\u200Fرقم").Bold();
                                });

                                // Table Body (Order Data) with alternating row colors
                                for (int i = 0; i < orders.Count; i++)
                                {
                                    var order = orders[i];
                                    var backgroundColor = i % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White;

                                    table.Cell().Background(backgroundColor).Border(0.5f).BorderColor(Colors.Black).Padding(3).AlignRight()
                                        .Text("\u200F" + order.Title).FontSize(10);
                                    table.Cell().Background(backgroundColor).Border(0.5f).BorderColor(Colors.Black).Padding(3).AlignRight()
                                        .Text("\u200F" + OrderHelper.GetPaymentMethodInArabic(order.PaymentMethod.ToString())).FontSize(10);
                                    table.Cell().Background(backgroundColor).Border(0.5f).BorderColor(Colors.Black).Padding(3).AlignRight()
                                        .Text("\u200F$" + order.FinalPrice.ToString("F2")).FontSize(10);
                                    table.Cell().Background(backgroundColor).Border(0.5f).BorderColor(Colors.Black).Padding(3).AlignRight()
                                        .Text("\u200F" + order.DeliveryTime.ToString("dd/MM/yyyy", new CultureInfo("en-US"))).FontSize(10); // Updated to English calendar
                                    table.Cell().Background(backgroundColor).Border(0.5f).BorderColor(Colors.Black).Padding(3).AlignRight()
                                        .Text("\u200F" + OrderHelper.GetOrderStatusInArabic(order.orderStatus.ToString())).FontSize(10);
                                    table.Cell().Background(backgroundColor).Border(0.5f).BorderColor(Colors.Black).Padding(3).AlignRight()
                                        .Text("\u200F" + (order.Customer?.FullName ?? "غير متوفر")).FontSize(10);
                                    table.Cell().Background(backgroundColor).Border(0.5f).BorderColor(Colors.Black).Padding(3).AlignRight()
                                        .Text("\u200F" + (order.Driver?.FullName ?? "غير متوفر")).FontSize(10);
                                    table.Cell().Background(backgroundColor).Border(0.5f).BorderColor(Colors.Black).Padding(3).AlignRight()
                                        .Text("\u200F" + order.Id.ToString()).FontSize(10);
                                }
                            });

                            // Summary Section
                            col.Item().PaddingTop(20).Column(summary =>
                            {
                                summary.Item().AlignCenter().Text("إجمالي الطلبات: " + totalOrders).FontSize(16).Bold();
                                summary.Item().AlignCenter().Text("إجمالي الإيرادات: $" + totalRevenue.ToString("F2")).FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                                summary.Item().AlignCenter().Text("إجمالي ربح الشركة: $" + companyProfit.ToString("F2")).FontSize(16).Bold().FontColor(Colors.Black);
                                summary.Item().AlignCenter().Text("إجمالي ربح السائقين: $" + driversProfit.ToString("F2")).FontSize(16).Bold().FontColor(Colors.Red.Darken2);
                            });
                        });
                    });
                }).GeneratePdf();
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating PDF: " + ex.Message);
            }
        }


        //public byte[] GenerateDriversPdf(List<Order> orders, string reportMessage, string driverName, decimal driverProfit, decimal companyRevenue, decimal companyProfit, DateTime? startDate = null, DateTime? endDate = null)
        //        {
        //            try
        //            {
        //                using var memoryStream = new MemoryStream();

        //                // Create a PdfWriter to write the PDF into the memory stream
        //                using var pdfWriter = new PdfWriter(memoryStream);
        //                using var pdfDocument = new PdfDocument(pdfWriter);

        //                // Create a document for adding content to the PDF
        //                var document = new Document(pdfDocument);

        //                // Load Arabic font (replace with the path to your Arabic font file)
        //                var fontPath = "path/to/arabic-font.ttf"; // Example: "fonts/arial.ttf"
        //                var arabicFont = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);

        //                // Set document direction to RTL
        //                document.SetProperty(Property.TEXT_ALIGNMENT, TextAlignment.RIGHT);

        //                // Add Company Name
        //                document.Add(new Paragraph("جو دليفري") // "Go Delivery" in Arabic
        //                    .SetTextAlignment(TextAlignment.CENTER)
        //                    .SetFontSize(18)
        //                    .SetFont(arabicFont));

        //                // Add Report Title
        //                document.Add(new Paragraph(reportMessage) // Assuming reportMessage is already in Arabic
        //                    .SetTextAlignment(TextAlignment.CENTER)
        //                    .SetFontSize(14)
        //                    .SetFont(arabicFont));

        //                // Add Driver Name and Date Range
        //                document.Add(new Paragraph($"السائق: {driverName}") // "Driver: {driverName}" in Arabic
        //                    .SetTextAlignment(TextAlignment.RIGHT)
        //                    .SetFontSize(12)
        //                    .SetFont(arabicFont));

        //                document.Add(new Paragraph($"تاريخ البدء: {startDate:yyyy-MM-dd}   |   تاريخ الانتهاء: {endDate:yyyy-MM-dd}   |   تم الإنشاء في: {DateTime.Now:yyyy-MM-dd}")
        //                    .SetTextAlignment(TextAlignment.RIGHT)
        //                    .SetFontSize(12)
        //                    .SetFont(arabicFont));

        //                // Add some space before the table
        //                document.Add(new Paragraph("\n"));

        //                // Define the table structure
        //                var table = new Table(new float[] { 1, 3, 3, 3, 2, 2, 2, 2 })
        //                    .UseAllAvailableWidth();

        //                // Add headers with smaller font size
        //                table.AddHeaderCell(new Cell().Add(new Paragraph("المعرف").SetFont(arabicFont)).SetFontSize(10));
        //                table.AddHeaderCell(new Cell().Add(new Paragraph("العميل").SetFont(arabicFont)).SetFontSize(10));
        //                table.AddHeaderCell(new Cell().Add(new Paragraph("السائق").SetFont(arabicFont)).SetFontSize(10));
        //                table.AddHeaderCell(new Cell().Add(new Paragraph("الوقت").SetFont(arabicFont)).SetFontSize(10));
        //                table.AddHeaderCell(new Cell().Add(new Paragraph("العنوان").SetFont(arabicFont)).SetFontSize(10));
        //                table.AddHeaderCell(new Cell().Add(new Paragraph("السعر النهائي").SetFont(arabicFont)).SetFontSize(10));
        //                table.AddHeaderCell(new Cell().Add(new Paragraph("طريقة الدفع").SetFont(arabicFont)).SetFontSize(10));
        //                table.AddHeaderCell(new Cell().Add(new Paragraph("الحالة").SetFont(arabicFont)).SetFontSize(10));

        //                // Add rows with smaller font size
        //                foreach (var order in orders)
        //                {
        //                    table.AddCell(new Cell().Add(new Paragraph(order.Id.ToString()).SetFont(arabicFont)).SetFontSize(9));
        //                    table.AddCell(new Cell().Add(new Paragraph(order.Customer?.FullName ?? "غير متاح").SetFont(arabicFont)).SetFontSize(9));
        //                    table.AddCell(new Cell().Add(new Paragraph(order.Driver?.FullName ?? "غير متاح").SetFont(arabicFont)).SetFontSize(9));
        //                    table.AddCell(new Cell().Add(new Paragraph(order.DeliveryTime.ToString("yyyy-MM-dd HH:mm")).SetFont(arabicFont)).SetFontSize(9));
        //                    table.AddCell(new Cell().Add(new Paragraph(order.Title).SetFont(arabicFont)).SetFontSize(9));
        //                    table.AddCell(new Cell().Add(new Paragraph($"${order.FinalPrice:F2}").SetFont(arabicFont)).SetFontSize(9));
        //                    table.AddCell(new Cell().Add(new Paragraph(order.PaymentMethod.ToString()).SetFont(arabicFont)).SetFontSize(9));
        //                    table.AddCell(new Cell().Add(new Paragraph(order.orderStatus.ToString()).SetFont(arabicFont)).SetFontSize(9));
        //                }

        //                // Add table to the document
        //                document.Add(table);

        //                // Add a line break before the summary section
        //                document.Add(new Paragraph("\n"));

        //                // Add summary section
        //                var summaryTable = new Table(new float[] { 4, 4 }) // Two-column table
        //                    .UseAllAvailableWidth();

        //                summaryTable.AddCell(new Cell().Add(new Paragraph("ربح السائق:").SetFont(arabicFont)).SetFontSize(10));
        //                summaryTable.AddCell(new Cell().Add(new Paragraph($"${driverProfit:F2}").SetFont(arabicFont)).SetFontSize(10));

        //                summaryTable.AddCell(new Cell().Add(new Paragraph("إيرادات الشركة:").SetFont(arabicFont)).SetFontSize(10));
        //                summaryTable.AddCell(new Cell().Add(new Paragraph($"${companyRevenue:F2}").SetFont(arabicFont)).SetFontSize(10));

        //                summaryTable.AddCell(new Cell().Add(new Paragraph("ربح الشركة:").SetFont(arabicFont)).SetFontSize(10));
        //                summaryTable.AddCell(new Cell().Add(new Paragraph($"${companyProfit:F2}").SetFont(arabicFont)).SetFontSize(10));

        //                document.Add(summaryTable);

        //                // Close the document to finalize the PDF
        //                document.Close();

        //                // Return the byte array representing the PDF
        //                return memoryStream.ToArray();
        //            }
        //            catch (Exception ex)
        //            {
        //                // Handle any errors that might occur during PDF generation
        //                throw new Exception("خطأ في إنشاء ملف PDF: " + ex.Message); // "Error generating PDF" in Arabic
        //            }
        //        }
    }
}
