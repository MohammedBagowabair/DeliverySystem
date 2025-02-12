


using Application.DTO.OrderDtos;
using Application.Interfaces;
using Domain.Entities;
using iText.IO.Font;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PdfGeneratorService : IPdfGeneratorService
    {


        public byte[] GenerateOrderPdf(List<Order> orders, string reportMessage)
        {
            try
            {
                using var memoryStream = new MemoryStream();


                // Create a PdfWriter to write the PDF into the memory stream
                using var pdfWriter = new PdfWriter(memoryStream);
                using var pdfDocument = new PdfDocument(pdfWriter);

                // Create a document for adding content to the PDF
                var document = new Document(pdfDocument);

                // Title of the document with smaller font size
                document.Add(new Paragraph(reportMessage)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(14)); // Smaller font size

                // Define the table structure
                var table = new Table(new float[] { 1, 3, 3, 3, 2, 2, 2, 2 })
                    .UseAllAvailableWidth();

                // Add headers with smaller font size
                table.AddHeaderCell(new Cell().Add(new Paragraph("ID")).SetFontSize(10)); // Set font size for header
                table.AddHeaderCell(new Cell().Add(new Paragraph("Customer")).SetFontSize(10));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Driver")).SetFontSize(10));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Time")).SetFontSize(10));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Title")).SetFontSize(10));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Final Price")).SetFontSize(10));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Payment")).SetFontSize(10));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Status")).SetFontSize(10));

                // Add rows with smaller font size
                foreach (var order in orders)
                {
                    table.AddCell(new Cell().Add(new Paragraph(order.Id.ToString())).SetFontSize(9)); // Smaller font size for content
                    table.AddCell(new Cell().Add(new Paragraph(order.Customer?.FullName ?? "N/A")).SetFontSize(9));
                    table.AddCell(new Cell().Add(new Paragraph(order.Driver?.FullName ?? "N/A")).SetFontSize(9));
                    table.AddCell(new Cell().Add(new Paragraph(order.DeliveryTime.ToString())).SetFontSize(9));
                    table.AddCell(new Cell().Add(new Paragraph(order.Title)).SetFontSize(9));
                    table.AddCell(new Cell().Add(new Paragraph($"${order.FinalPrice:F2}")).SetFontSize(9));
                    table.AddCell(new Cell().Add(new Paragraph(order.PaymentMethod.ToString())).SetFontSize(9));
                    table.AddCell(new Cell().Add(new Paragraph(order.orderStatus.ToString())).SetFontSize(9));
                }

                // Add table to the document
                document.Add(table);

                // Close the document to finalize the PDF
                document.Close();

                // Return the byte array representing the PDF
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                // Handle any errors that might occur during PDF generation
                throw new Exception("Error generating PDF: " + ex.Message);
            }
        }

        public byte[] GenerateDriversPdf(List<Order> orders, string reportMessage, string driverName, decimal driverProfit, decimal companyRevenue, decimal companyProfit, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                using var memoryStream = new MemoryStream();

                // Create a PdfWriter to write the PDF into the memory stream
                using var pdfWriter = new PdfWriter(memoryStream);
                using var pdfDocument = new PdfDocument(pdfWriter);

                // Create a document for adding content to the PDF
                var document = new Document(pdfDocument);

                // Add Company Name
                document.Add(new Paragraph("Go Delivery")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(18)
                    );

                // Add Report Title
                document.Add(new Paragraph(reportMessage)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(14)
                    );

                // Add Driver Name and Date Range
                document.Add(new Paragraph($"Driver: {driverName}")
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetFontSize(12));

                document.Add(new Paragraph($"Start Date: {startDate:yyyy-MM-dd}   |   End Date: {endDate:yyyy-MM-dd}   |   Generated On: {DateTime.Now:yyyy-MM-dd}")
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetFontSize(12));

                // Add some space before the table
                document.Add(new Paragraph("\n"));

                // Define the table structure
                var table = new Table(new float[] { 1, 3, 3, 3, 2, 2, 2, 2 })
                    .UseAllAvailableWidth();

                // Add headers with smaller font size
                table.AddHeaderCell(new Cell().Add(new Paragraph("ID")).SetFontSize(10));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Customer")).SetFontSize(10));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Driver")).SetFontSize(10));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Time")).SetFontSize(10));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Title")).SetFontSize(10));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Final Price")).SetFontSize(10));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Payment")).SetFontSize(10));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Status")).SetFontSize(10));

                // Add rows with smaller font size
                foreach (var order in orders)
                {
                    table.AddCell(new Cell().Add(new Paragraph(order.Id.ToString())).SetFontSize(9));
                    table.AddCell(new Cell().Add(new Paragraph(order.Customer?.FullName ?? "N/A")).SetFontSize(9));
                    table.AddCell(new Cell().Add(new Paragraph(order.Driver?.FullName ?? "N/A")).SetFontSize(9));
                    table.AddCell(new Cell().Add(new Paragraph(order.DeliveryTime.ToString("yyyy-MM-dd HH:mm"))).SetFontSize(9));
                    table.AddCell(new Cell().Add(new Paragraph(order.Title)).SetFontSize(9));
                    table.AddCell(new Cell().Add(new Paragraph($"${order.FinalPrice:F2}")).SetFontSize(9));
                    table.AddCell(new Cell().Add(new Paragraph(order.PaymentMethod.ToString())).SetFontSize(9));
                    table.AddCell(new Cell().Add(new Paragraph(order.orderStatus.ToString())).SetFontSize(9));
                }

                // Add table to the document
                document.Add(table);

                // Add a line break before the summary section
                document.Add(new Paragraph("\n"));

                // Add summary section
                var summaryTable = new Table(new float[] { 4, 4 }) // Two-column table
                    .UseAllAvailableWidth();

                summaryTable.AddCell(new Cell().Add(new Paragraph("Driver Profit:")).SetFontSize(10));
                summaryTable.AddCell(new Cell().Add(new Paragraph($"${driverProfit:F2}")).SetFontSize(10));

                summaryTable.AddCell(new Cell().Add(new Paragraph("Company Revenue:")).SetFontSize(10));
                summaryTable.AddCell(new Cell().Add(new Paragraph($"${companyRevenue:F2}")).SetFontSize(10));

                summaryTable.AddCell(new Cell().Add(new Paragraph("Company Profit:")).SetFontSize(10));
                summaryTable.AddCell(new Cell().Add(new Paragraph($"${companyProfit:F2}")).SetFontSize(10));

                document.Add(summaryTable);

                // Close the document to finalize the PDF
                document.Close();

                // Return the byte array representing the PDF
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                // Handle any errors that might occur during PDF generation
                throw new Exception("Error generating PDF: " + ex.Message);
            }
        }

    }
}
