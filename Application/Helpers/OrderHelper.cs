using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Helpers
{
    public static class OrderHelper
    {
        public static string GetPaymentMethodInArabic(string paymentMethod)
        {
            switch (paymentMethod)
            {
                case "Cash":
                    return "نقدي";
                case "CreditCard":
                    return "بطاقة ائتمان";
                case "BankTransfer":
                    return "تحويل بنكي";
                default:
                    return "غير محدد";
            }
        }
        public static string GetOrderStatusInArabic(string status)
        {
            return status switch
            {
                "Delivered" => "تم التسليم",
                "Processing" => "جاري المعالجة",
                "Canceled" => "تم الإلغاء",
                _ => "غير معروف" // Default case if the status is unknown
            };
        }
    }

}
