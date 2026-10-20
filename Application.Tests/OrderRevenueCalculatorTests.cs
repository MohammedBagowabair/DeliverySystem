using Application.Helpers;
using Domain.Enums;
using Domain.Exceptions;
using Xunit;

namespace Application.Tests
{
    public class OrderRevenueCalculatorTests
    {
        [Fact]
        public void Percentage_discount_reduces_company_share_only()
        {
            var result = OrderRevenueCalculator.Calculate(10000m, 0.70m, DiscountType.Percentage, 50m);

            Assert.Equal(7000m, result.DriverAmount);
            Assert.Equal(3000m, result.CompanyGrossAmount);
            Assert.Equal(1500m, result.DiscountAmount);
            Assert.Equal(1500m, result.CompanyNetAmount);
            Assert.Equal(8500m, result.FinalCustomerAmount);
        }

        [Fact]
        public void Fixed_discount_reduces_company_share_only()
        {
            var result = OrderRevenueCalculator.Calculate(10000m, 0.70m, DiscountType.Fixed, 1000m);

            Assert.Equal(7000m, result.DriverAmount);
            Assert.Equal(2000m, result.CompanyNetAmount);
            Assert.Equal(9000m, result.FinalCustomerAmount);
        }

        [Fact]
        public void Discount_cannot_exceed_company_share()
        {
            Assert.Throws<DeliveryCoreException>(() =>
                OrderRevenueCalculator.Calculate(10000m, 0.70m, DiscountType.Fixed, 4000m));
        }

        [Fact]
        public void Zero_discount_keeps_full_split()
        {
            var result = OrderRevenueCalculator.Calculate(10000m, 0.70m, DiscountType.None, 0m);

            Assert.Equal(7000m, result.DriverAmount);
            Assert.Equal(3000m, result.CompanyGrossAmount);
            Assert.Equal(0m, result.DiscountAmount);
            Assert.Equal(3000m, result.CompanyNetAmount);
            Assert.Equal(10000m, result.FinalCustomerAmount);
        }

        [Fact]
        public void One_hundred_percent_company_discount_zeros_company_net()
        {
            var result = OrderRevenueCalculator.Calculate(10000m, 0.70m, DiscountType.Percentage, 100m);

            Assert.Equal(7000m, result.DriverAmount);
            Assert.Equal(3000m, result.DiscountAmount);
            Assert.Equal(0m, result.CompanyNetAmount);
            Assert.Equal(7000m, result.FinalCustomerAmount);
        }

        [Fact]
        public void Fixed_discount_equal_to_company_share_is_allowed()
        {
            var result = OrderRevenueCalculator.Calculate(10000m, 0.70m, DiscountType.Fixed, 3000m);

            Assert.Equal(0m, result.CompanyNetAmount);
            Assert.Equal(7000m, result.FinalCustomerAmount);
        }

        [Fact]
        public void Default_rate_used_when_rate_is_zero()
        {
            var result = OrderRevenueCalculator.Calculate(10000m, 0m, DiscountType.None, 0m);

            Assert.Equal(RevenueSplit.DefaultDriverCommissionRate, result.DriverCommissionRate);
            Assert.Equal(7000m, result.DriverAmount);
        }

        [Fact]
        public void ResolveDriverProfit_prefers_stored_amount()
        {
            var profit = OrderRevenueCalculator.ResolveDriverProfit(6500m, 10000m, 0.70m);
            Assert.Equal(6500m, profit);
        }

        [Fact]
        public void ResolveDriverProfit_falls_back_to_legacy_rate()
        {
            var profit = OrderRevenueCalculator.ResolveDriverProfit(0m, 10000m, 0.70m);
            Assert.Equal(7000m, profit);
        }
    }

    public class IdentityHelpersTests
    {
        [Fact]
        public void Pbkdf2_roundtrip_validates()
        {
            IdentityHelpers.GetPasswordHashAndSalt("Secret@123", out var hash, out var salt);
            Assert.StartsWith("PBKDF2$", hash);
            Assert.True(IdentityHelpers.ValidatePassword("Secret@123", hash, salt));
            Assert.False(IdentityHelpers.ValidatePassword("wrong", hash, salt));
        }

        [Fact]
        public void Legacy_hmac_still_validates()
        {
            using var hmac = new System.Security.Cryptography.HMACSHA256();
            var salt = Convert.ToBase64String(hmac.Key);
            var hash = Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes("Legacy@123")));

            Assert.False(hash.StartsWith("PBKDF2$"));
            Assert.True(IdentityHelpers.ValidatePassword("Legacy@123", hash, salt));
        }

        [Fact]
        public void Reset_token_hash_is_stable()
        {
            var raw = IdentityHelpers.CreateRawResetToken();
            var a = IdentityHelpers.HashResetToken(raw);
            var b = IdentityHelpers.HashResetToken(raw);
            Assert.Equal(a, b);
            Assert.NotEqual(a, IdentityHelpers.HashResetToken(raw + "x"));
        }
    }
}
