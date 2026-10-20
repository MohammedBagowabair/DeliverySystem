using Domain.Enums;
using Domain.Exceptions;

namespace Application.Helpers
{
    /// <summary>
    /// Driver commission from the original amount first; discount reduces company share only.
    /// </summary>
    public static class OrderRevenueCalculator
    {
        public static OrderRevenueBreakdown Calculate(
            decimal originalAmount,
            decimal driverCommissionRate,
            DiscountType discountType,
            decimal discountValue)
        {
            if (originalAmount < 0)
            {
                throw new DeliveryCoreException(
                    "مبلغ الطلب الأصلي لا يمكن أن يكون سالباً.",
                    ErrorCodes.DB_UNABLE_TO_INSERT_RECORD_CODE,
                    null);
            }

            var rate = driverCommissionRate > 0
                ? driverCommissionRate
                : RevenueSplit.DefaultDriverCommissionRate;

            if (rate <= 0 || rate > 1m)
            {
                throw new DeliveryCoreException(
                    "نسبة عمولة الموصل يجب أن تكون بين 0 و 100٪.",
                    ErrorCodes.DB_UNABLE_TO_INSERT_RECORD_CODE,
                    null);
            }

            var companyRate = Math.Max(0, 1m - rate);
            var driverAmount = RoundMoney(originalAmount * rate);
            var companyGross = RoundMoney(originalAmount - driverAmount);

            var discountAmount = ComputeDiscountAmount(companyGross, discountType, discountValue);

            if (discountAmount > companyGross)
            {
                throw new DeliveryCoreException(
                    $"الخصم ({discountAmount:N0}) يتجاوز حصة الشركة ({companyGross:N0}). لا يمكن أن تصبح إيرادات الشركة سالبة.",
                    ErrorCodes.DB_UNABLE_TO_INSERT_RECORD_CODE,
                    null);
            }

            var companyNet = RoundMoney(companyGross - discountAmount);
            var finalAmount = RoundMoney(driverAmount + companyNet);

            if (finalAmount < 0 || companyNet < 0)
            {
                throw new DeliveryCoreException(
                    "المبلغ النهائي أو صافي حصة الشركة لا يمكن أن يكون سالباً.",
                    ErrorCodes.DB_UNABLE_TO_INSERT_RECORD_CODE,
                    null);
            }

            return new OrderRevenueBreakdown
            {
                OriginalAmount = RoundMoney(originalAmount),
                DriverCommissionRate = rate,
                DriverAmount = driverAmount,
                CompanyShareRate = companyRate,
                CompanyGrossAmount = companyGross,
                DiscountType = discountType,
                DiscountValue = discountValue,
                DiscountAmount = discountAmount,
                CompanyNetAmount = companyNet,
                FinalCustomerAmount = finalAmount
            };
        }

        public static void ApplyToOrder(
            Domain.Entities.Order order,
            decimal driverCommissionRate)
        {
            var type = order.DiscountType;
            var value = order.DiscountValue;

            // Legacy: CouponDiscount was a % of delivery fee; map when new fields are empty.
            if (type == DiscountType.None && order.CouponDiscount > 0 && value <= 0)
            {
                type = DiscountType.Percentage;
                value = order.CouponDiscount;
            }

            var breakdown = Calculate(order.DeliveryFee, driverCommissionRate, type, value);

            order.DiscountType = breakdown.DiscountType;
            order.DiscountValue = breakdown.DiscountValue;
            order.DiscountAmount = breakdown.DiscountAmount;
            order.DriverCommissionRate = breakdown.DriverCommissionRate;
            order.DriverAmount = breakdown.DriverAmount;
            order.CompanyGrossAmount = breakdown.CompanyGrossAmount;
            order.CompanyNetAmount = breakdown.CompanyNetAmount;
            order.FinalPrice = breakdown.FinalCustomerAmount;

            // Keep CouponDiscount in sync for older clients (percentage only).
            order.CouponDiscount = breakdown.DiscountType == DiscountType.Percentage
                ? breakdown.DiscountValue
                : 0;
        }

        /// <summary>
        /// Driver profit for reporting: prefer stored DriverAmount; else legacy FinalPrice × rate.
        /// </summary>
        public static decimal ResolveDriverProfit(decimal driverAmount, decimal finalPrice, decimal commissionRate)
        {
            if (driverAmount > 0)
                return driverAmount;

            return RevenueSplit.DriverProfitForOrder(finalPrice, commissionRate);
        }

        private static decimal ComputeDiscountAmount(
            decimal companyGross,
            DiscountType discountType,
            decimal discountValue)
        {
            if (discountType == DiscountType.None || discountValue <= 0)
                return 0;

            if (discountType == DiscountType.Percentage)
            {
                if (discountValue > 100m)
                {
                    throw new DeliveryCoreException(
                        "نسبة الخصم يجب أن تكون بين 0 و 100.",
                        ErrorCodes.DB_UNABLE_TO_INSERT_RECORD_CODE,
                        null);
                }

                return RoundMoney(companyGross * (discountValue / 100m));
            }

            if (discountType == DiscountType.Fixed)
            {
                return RoundMoney(discountValue);
            }

            return 0;
        }

        private static decimal RoundMoney(decimal value)
            => Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
