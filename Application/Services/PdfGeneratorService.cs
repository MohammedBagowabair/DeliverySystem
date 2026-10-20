using Application.Helpers;
using Application.Interfaces;
using Domain.Constants;
using Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace Application.Services
{
    public class PdfGeneratorService : IPdfGeneratorService
    {
        private static readonly CultureInfo YemenCulture = CreateYemenMoneyCulture();
        private static readonly CultureInfo ArabicCulture = new("ar-SA");
        private static readonly CultureInfo EnglishCulture = new("en-US");

        private static CultureInfo CreateYemenMoneyCulture()
        {
            var culture = (CultureInfo)CultureInfo.GetCultureInfo("ar-YE").Clone();
            culture.NumberFormat.CurrencySymbol = "ر.ي";
            culture.NumberFormat.CurrencyDecimalDigits = 0;
            culture.NumberFormat.CurrencyPositivePattern = 3;
            culture.NumberFormat.CurrencyNegativePattern = 8;
            return culture;
        }

        // Print-friendly brand palette (deep navy + soft slate — readable on paper)
        private static readonly string Brand = "#1E3A5F";
        private static readonly string BrandSoft = "#E8EEF5";
        private static readonly string Accent = "#2E7D6F";
        private static readonly string AccentSoft = "#E6F4F1";
        private static readonly string Danger = "#B42318";
        private static readonly string DangerSoft = "#FCEBEA";
        private static readonly string Line = "#D0D7E2";
        private static readonly string Muted = "#5B6B7C";
        private static readonly string Zebra = "#F7F9FC";

        private readonly ICompanyFundService _companyFundService;

        public PdfGeneratorService(ICompanyFundService companyFundService)
        {
            _companyFundService = companyFundService;
        }

        public byte[] GenerateOrderPdf(List<Order> orders, string reportMessage, string? driverName = null)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            try
            {
                var now = YemenTime.Now;
                var arabicDateTime = now.ToString("dddd، dd MMMM yyyy - hh:mm tt", ArabicCulture);
                var englishDateTime = now.ToString("yyyy/MM/dd - hh:mm tt", EnglishCulture);

                var deliveredOrders = orders
                    .Where(o => o.orderStatus == OrderStatus.Delivered)
                    .OrderBy(o => o.DeliveryTime)
                    .ThenBy(o => o.Id)
                    .ToList();

                var totalOrders = deliveredOrders.Count;
                var totalRevenue = deliveredOrders.Sum(o => o.FinalPrice);
                var driversProfit = deliveredOrders.Sum(o =>
                    OrderRevenueCalculator.ResolveDriverProfit(
                        o.DriverAmount,
                        o.FinalPrice,
                        o.Driver?.CommissionRate ?? o.DriverCommissionRate));
                var fundEntities = _companyFundService.GetActiveEntitiesAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                var breakdown = RevenueSplit.Calculate(totalRevenue, driversProfit, fundEntities);

                var title = string.IsNullOrWhiteSpace(reportMessage) || reportMessage == "Orders"
                    ? "تقرير الطلبات المسلّمة"
                    : reportMessage.Trim();
                var resolvedDriver = ResolveDriverLabel(driverName);

                var periodStart = deliveredOrders.FirstOrDefault()?.DeliveryTime;
                var periodEnd = deliveredOrders.LastOrDefault()?.DeliveryTime;

                return Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(24);
                        page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10).FontColor(Brand));
                        page.ContentFromRightToLeft();

                        page.Header().Element(h => ComposeHeader(
                            h, title, resolvedDriver, arabicDateTime, englishDateTime, periodStart, periodEnd, totalOrders));

                        page.Content().PaddingTop(14).Column(col =>
                        {
                            col.Spacing(14);

                            if (deliveredOrders.Count == 0)
                            {
                                col.Item()
                                    .Border(1).BorderColor(Line)
                                    .Background(BrandSoft)
                                    .Padding(24)
                                    .AlignCenter()
                                    .Text("لا توجد طلبات مسلّمة ضمن هذه الفترة.")
                                    .FontSize(13).FontColor(Muted);
                            }
                            else
                            {
                                col.Item().Element(e => ComposeOrdersTable(e, deliveredOrders));
                            }

                            col.Item().Element(e => ComposeSummary(e, totalOrders, totalRevenue, driversProfit, breakdown));
                        });

                        page.Footer().Element(ComposeFooter);
                    });
                }).GeneratePdf();
            }
            catch (Exception ex)
            {
                throw new Exception("خطأ في إنشاء ملف PDF: " + ex.Message, ex);
            }
        }

        private static void ComposeHeader(
            IContainer container,
            string title,
            string driverLabel,
            string arabicDateTime,
            string englishDateTime,
            DateTime? periodStart,
            DateTime? periodEnd,
            int totalOrders)
        {
            container.Column(col =>
            {
                col.Item().Background(Brand).PaddingVertical(14).PaddingHorizontal(16).Row(row =>
                {
                    row.RelativeItem().AlignMiddle().Column(c =>
                    {
                        c.Item().Text("Go Delivery · جو دليفري")
                            .FontSize(20).Bold().FontColor(Colors.White);
                        c.Item().PaddingTop(2).Text(title)
                            .FontSize(12).FontColor("#B8C9DC");
                    });

                    row.ConstantItem(210).AlignMiddle().AlignLeft().Column(c =>
                    {
                        c.Item().AlignLeft().Text(arabicDateTime)
                            .FontSize(9).FontColor("#D7E2EF");
                        c.Item().AlignLeft().Text(englishDateTime)
                            .FontSize(9).FontColor("#D7E2EF");
                    });
                });

                col.Item().Background(BrandSoft).BorderBottom(1).BorderColor(Line)
                    .PaddingVertical(10).PaddingHorizontal(16)
                    .Row(row =>
                    {
                        row.RelativeItem().Text(text =>
                        {
                            text.Span("الموصل: ").FontColor(Muted).FontSize(10);
                            text.Span(driverLabel).Bold().FontSize(11).FontColor(Brand);
                        });

                        row.RelativeItem().AlignCenter().Text(text =>
                        {
                            text.Span("الفترة: ").FontColor(Muted).FontSize(10);
                            text.Span(FormatPeriod(periodStart, periodEnd)).Bold().FontSize(10).FontColor(Brand);
                        });

                        row.RelativeItem().AlignLeft().Text(text =>
                        {
                            text.Span("عدد المسلّم: ").FontColor(Muted).FontSize(10);
                            text.Span(totalOrders.ToString("N0")).Bold().FontSize(11).FontColor(Accent);
                        });
                    });
            });
        }

        private static void ComposeOrdersTable(IContainer container, List<Order> deliveredOrders)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2.2f); // عنوان
                    columns.RelativeColumn(1.1f); // دفع
                    columns.RelativeColumn(1.3f); // سعر
                    columns.RelativeColumn(1.1f); // عمولة
                    columns.RelativeColumn(1.2f); // تاريخ
                    columns.RelativeColumn(1.0f); // حالة
                    columns.RelativeColumn(1.6f); // عميل
                    columns.RelativeColumn(1.6f); // موصل
                    columns.ConstantColumn(48);   // رقم
                });

                table.Header(header =>
                {
                    HeaderCell(header.Cell(), "عنوان الطلب");
                    HeaderCell(header.Cell(), "طريقة الدفع");
                    HeaderCell(header.Cell(), "السعر");
                    HeaderCell(header.Cell(), "عمولة الموصل");
                    HeaderCell(header.Cell(), "التاريخ");
                    HeaderCell(header.Cell(), "الحالة");
                    HeaderCell(header.Cell(), "العميل");
                    HeaderCell(header.Cell(), "الموصل");
                    HeaderCell(header.Cell(), "#");
                });

                for (var i = 0; i < deliveredOrders.Count; i++)
                {
                    var order = deliveredOrders[i];
                    var bg = i % 2 == 0 ? Zebra : "#FFFFFF";
                    var commission = OrderRevenueCalculator.ResolveDriverProfit(
                        order.DriverAmount,
                        order.FinalPrice,
                        order.Driver?.CommissionRate ?? order.DriverCommissionRate);

                    BodyCell(table.Cell(), bg, order.Title);
                    BodyCell(table.Cell(), bg, OrderHelper.GetPaymentMethodInArabic(order.PaymentMethod.ToString()));
                    BodyCell(table.Cell(), bg, order.FinalPrice.ToString("C0", YemenCulture), bold: true);
                    BodyCell(table.Cell(), bg, commission.ToString("C0", YemenCulture));
                    BodyCell(table.Cell(), bg, order.DeliveryTime.ToString("dd/MM/yyyy", EnglishCulture));
                    BodyCell(table.Cell(), bg, OrderHelper.GetOrderStatusInArabic(order.orderStatus.ToString()), accent: true);
                    BodyCell(table.Cell(), bg, !string.IsNullOrWhiteSpace(order.CustomerName)
                        ? order.CustomerName
                        : (order.Customer?.FullName ?? "—"));
                    BodyCell(table.Cell(), bg, !string.IsNullOrWhiteSpace(order.DriverName)
                        ? order.DriverName
                        : (order.Driver?.FullName ?? "—"));
                    BodyCell(table.Cell(), bg, order.Id.ToString(), muted: true);
                }
            });
        }

        private static void ComposeSummary(
            IContainer container,
            int totalOrders,
            decimal totalRevenue,
            decimal driversProfit,
            CompanyProfitBreakdown breakdown)
        {
            container.Column(col =>
            {
                col.Item().PaddingBottom(8).Text("الملخص المالي")
                    .FontSize(13).Bold().FontColor(Brand);

                col.Item().Row(row =>
                {
                    row.Spacing(10);
                    SummaryCard(row.RelativeItem(), "عدد الطلبات", totalOrders.ToString("N0"), Brand, BrandSoft);
                    SummaryCard(row.RelativeItem(), "إجمالي الإيرادات (100٪)", totalRevenue.ToString("C0", YemenCulture), Brand, BrandSoft);
                    SummaryCard(row.RelativeItem(), "نسبة المندوب (70٪)",
                        driversProfit.ToString("C0", YemenCulture), Danger, DangerSoft);
                    SummaryCard(row.RelativeItem(), "نسبة الإدارة (30٪)",
                        breakdown.RemainingAfterDrivers.ToString("C0", YemenCulture), Accent, AccentSoft);
                });

            });
        }

        private static void ComposeFooter(IContainer container)
        {
            container.BorderTop(1).BorderColor(Line).PaddingTop(8).Row(row =>
            {
                row.RelativeItem().AlignMiddle().Text("Go Delivery — تقرير داخلي سري")
                    .FontSize(8).FontColor(Muted);

                row.RelativeItem().AlignCenter().AlignMiddle().Text(text =>
                {
                    text.Span("صفحة ").FontSize(8).FontColor(Muted);
                    text.CurrentPageNumber().FontSize(8).FontColor(Brand).Bold();
                    text.Span(" / ").FontSize(8).FontColor(Muted);
                    text.TotalPages().FontSize(8).FontColor(Brand).Bold();
                });

                row.RelativeItem().AlignLeft().AlignMiddle()
                    .Text("بتوقيت اليمن (UTC+3)")
                    .FontSize(8).FontColor(Muted);
            });
        }

        private static void HeaderCell(IContainer container, string text)
        {
            container
                .Background(Brand)
                .BorderBottom(2).BorderColor(Accent)
                .PaddingVertical(7).PaddingHorizontal(6)
                .AlignRight()
                .Text(text)
                .FontSize(9).Bold().FontColor(Colors.White);
        }

        private static void BodyCell(
            IContainer container,
            string background,
            string text,
            bool bold = false,
            bool muted = false,
            bool accent = false)
        {
            var cell = container
                .Background(background)
                .BorderBottom(0.5f).BorderColor(Line)
                .PaddingVertical(5).PaddingHorizontal(6)
                .AlignRight();

            var style = cell.Text(text ?? "—").FontSize(9);
            if (bold) style.Bold().FontColor(Brand);
            else if (accent) style.FontColor(Accent).Bold();
            else if (muted) style.FontColor(Muted);
            else style.FontColor("#243447");
        }

        private static void SummaryCard(IContainer container, string label, string value, string color, string soft)
        {
            container.Background(soft).Border(1).BorderColor(Line).Padding(12).Column(c =>
            {
                c.Item().Text(label).FontSize(9).FontColor(Muted);
                c.Item().PaddingTop(4).Text(value).FontSize(13).Bold().FontColor(color);
            });
        }

        private static void MiniStat(IContainer container, string label, string value)
        {
            container.Column(c =>
            {
                c.Item().AlignCenter().Text(label).FontSize(8).FontColor(Muted);
                c.Item().AlignCenter().PaddingTop(2).Text(value).FontSize(11).Bold().FontColor(Brand);
            });
        }

        private static string FormatPeriod(DateTime? start, DateTime? end)
        {
            if (start == null || end == null)
                return "—";
            if (start.Value.Date == end.Value.Date)
                return start.Value.ToString("dd/MM/yyyy", EnglishCulture);
            return $"{start.Value:dd/MM/yyyy} – {end.Value:dd/MM/yyyy}";
        }

        private static string ResolveDriverLabel(string? driverName)
        {
            if (string.IsNullOrWhiteSpace(driverName) || driverName.Contains('?'))
                return "جميع الموصلين";
            return driverName.Trim();
        }

        private static bool IsAllDrivers(string driverLabel)
            => string.Equals(driverLabel, "جميع الموصلين", StringComparison.Ordinal);
    }
}
