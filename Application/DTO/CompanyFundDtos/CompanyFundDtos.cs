using Domain.Entities;

namespace Application.DTO.CompanyFundDtos
{
    public class CompanyFundDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        /// <summary>Fraction 0–1 of post-driver remainder.</summary>
        public decimal Share { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByUserName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedByUserName { get; set; }
    }

    public class UpsertCompanyFundDTO
    {
        public int? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        /// <summary>Fraction 0–1 of post-driver remainder.</summary>
        public decimal Share { get; set; }

        public int SortOrder { get; set; }
    }

    public class ReplaceCompanyFundsDTO
    {
        public List<UpsertCompanyFundDTO> Funds { get; set; } = new();
        public string? ChangeReason { get; set; }
    }

    public class CompanyFundChangeLogDTO
    {
        public int Id { get; set; }
        public int? FundId { get; set; }
        public string FundName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public decimal? OldShare { get; set; }
        public decimal? NewShare { get; set; }
        public string? Details { get; set; }
        public DateTime ChangedAt { get; set; }
        public int ChangedByUserId { get; set; }
        public string? ChangedByUserName { get; set; }
    }

    public class FundAmountDTO
    {
        public int FundId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Share { get; set; }
        public decimal Amount { get; set; }
    }
}
