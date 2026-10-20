using Application.DTO.CompanyFundDtos;
using Application.Helpers;
using Application.Interfaces;
using Domain.Common.Models;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class CompanyFundService : ICompanyFundService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICompanySettingsService _companySettingsService;

        public CompanyFundService(IApplicationDbContext dbContext, ICompanySettingsService companySettingsService)
        {
            _dbContext = dbContext;
            _companySettingsService = companySettingsService;
        }

        public async Task EnsureSeededAsync()
        {
            var any = await _dbContext._dbContext.Set<CompanyFund>().AnyAsync();
            if (any)
                return;

            var settings = await _companySettingsService.GetAsync();
            var defaults = RevenueSplit.DefaultFundsFromSettings(settings);
            var now = YemenTime.Now;
            var entities = defaults.Select((f, i) => new CompanyFund
            {
                Name = f.Name,
                Description = f.Description,
                Share = RoundShare(f.Share),
                SortOrder = i,
                IsActive = true,
                CreatedAt = now
            }).ToList();

            NormalizeLastShare(entities);
            ValidateFundSet(entities.Select(e => (e.Name, e.Share)));

            await _dbContext._dbContext.Set<CompanyFund>().AddRangeAsync(entities);
            await _dbContext._dbContext.SaveChangesAsync();
        }

        public async Task<List<CompanyFundDTO>> GetActiveAsync()
        {
            await EnsureSeededAsync();

            var withUsers = await _dbContext._dbContext.Set<CompanyFund>()
                .AsNoTracking()
                .Include(f => f.CreatedByUser)
                .Include(f => f.UpdatedByUser)
                .Where(f => f.IsActive)
                .OrderBy(f => f.SortOrder)
                .ThenBy(f => f.Id)
                .ToListAsync();

            return withUsers.Select(MapFund).ToList();
        }

        public async Task<List<CompanyFund>> GetActiveEntitiesAsync()
        {
            await EnsureSeededAsync();

            return await _dbContext._dbContext.Set<CompanyFund>()
                .AsNoTracking()
                .Where(f => f.IsActive)
                .OrderBy(f => f.SortOrder)
                .ThenBy(f => f.Id)
                .ToListAsync();
        }

        public async Task<List<CompanyFundDTO>> ReplaceAllAsync(ReplaceCompanyFundsDTO dto, int changedByUserId)
        {
            if (changedByUserId <= 0)
            {
                throw new DeliveryCoreException(
                    "تعذر تحديد المستخدم الذي يقوم بالتعديل.",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }

            if (dto?.Funds == null || dto.Funds.Count == 0)
            {
                throw new DeliveryCoreException(
                    "يجب تعريف صندوق واحد على الأقل.",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }

            var incoming = dto.Funds
                .Select((f, i) => new UpsertCompanyFundDTO
                {
                    Id = f.Id,
                    Name = (f.Name ?? string.Empty).Trim(),
                    Description = string.IsNullOrWhiteSpace(f.Description) ? null : f.Description.Trim(),
                    Share = RoundShare(f.Share),
                    SortOrder = f.SortOrder > 0 ? f.SortOrder : i
                })
                .ToList();

            ValidateFundSet(incoming.Select(f => (f.Name, f.Share)));

            await using var tx = await _dbContext._dbContext.Database.BeginTransactionAsync();
            try
            {
                var existing = await _dbContext._dbContext.Set<CompanyFund>()
                    .Where(f => f.IsActive)
                    .ToListAsync();

                var now = YemenTime.Now;
                var keptIds = incoming.Where(f => f.Id.HasValue && f.Id.Value > 0)
                    .Select(f => f.Id!.Value)
                    .ToHashSet();

                var logs = new List<CompanyFundChangeLog>();

                // Soft-delete removed funds
                foreach (var fund in existing.Where(f => !keptIds.Contains(f.Id)))
                {
                    fund.IsActive = false;
                    fund.UpdatedAt = now;
                    fund.UpdatedByUserId = changedByUserId;
                    logs.Add(new CompanyFundChangeLog
                    {
                        FundId = fund.Id,
                        FundName = fund.Name,
                        Action = "Deleted",
                        OldShare = fund.Share,
                        NewShare = null,
                        Details = BuildReason(dto.ChangeReason, $"تم حذف الصندوق «{fund.Name}»"),
                        ChangedAt = now,
                        ChangedByUserId = changedByUserId
                    });
                }

                foreach (var item in incoming.OrderBy(f => f.SortOrder))
                {
                    if (item.Id.HasValue && item.Id.Value > 0)
                    {
                        var fund = existing.FirstOrDefault(f => f.Id == item.Id.Value)
                            ?? throw new DeliveryCoreException(
                                $"الصندوق رقم {item.Id} غير موجود.",
                                ErrorCodes.DATABASE_RECORD_NOT_FOUND_CODE,
                                null);

                        var oldShare = fund.Share;
                        var oldName = fund.Name;
                        var changed = !string.Equals(oldName, item.Name, StringComparison.Ordinal)
                            || oldShare != item.Share
                            || (fund.Description ?? "") != (item.Description ?? "")
                            || fund.SortOrder != item.SortOrder;

                        fund.Name = item.Name;
                        fund.Description = item.Description;
                        fund.Share = item.Share;
                        fund.SortOrder = item.SortOrder;
                        fund.IsActive = true;
                        fund.UpdatedAt = now;
                        fund.UpdatedByUserId = changedByUserId;

                        if (changed)
                        {
                            logs.Add(new CompanyFundChangeLog
                            {
                                FundId = fund.Id,
                                FundName = fund.Name,
                                Action = "Updated",
                                OldShare = oldShare,
                                NewShare = fund.Share,
                                Details = BuildReason(dto.ChangeReason,
                                    $"تعديل «{oldName}» ({Percent(oldShare)}) → «{fund.Name}» ({Percent(fund.Share)})"),
                                ChangedAt = now,
                                ChangedByUserId = changedByUserId
                            });
                        }
                    }
                    else
                    {
                        var fund = new CompanyFund
                        {
                            Name = item.Name,
                            Description = item.Description,
                            Share = item.Share,
                            SortOrder = item.SortOrder,
                            IsActive = true,
                            CreatedAt = now,
                            CreatedByUserId = changedByUserId
                        };
                        await _dbContext._dbContext.Set<CompanyFund>().AddAsync(fund);
                        await _dbContext._dbContext.SaveChangesAsync();

                        logs.Add(new CompanyFundChangeLog
                        {
                            FundId = fund.Id,
                            FundName = fund.Name,
                            Action = "Created",
                            OldShare = null,
                            NewShare = fund.Share,
                            Details = BuildReason(dto.ChangeReason,
                                $"إضافة صندوق «{fund.Name}» بنسبة {Percent(fund.Share)}"),
                            ChangedAt = now,
                            ChangedByUserId = changedByUserId
                        });
                    }
                }

                // Snapshot of final allocation
                var snapshot = string.Join(" | ",
                    incoming.OrderBy(f => f.SortOrder).Select(f => $"{f.Name}={Percent(f.Share)}"));
                logs.Add(new CompanyFundChangeLog
                {
                    FundId = null,
                    FundName = "*",
                    Action = "Replaced",
                    OldShare = null,
                    NewShare = 1m,
                    Details = BuildReason(dto.ChangeReason, $"التوزيع النهائي بعد الخصم من عمولة الموصل: {snapshot}"),
                    ChangedAt = now,
                    ChangedByUserId = changedByUserId
                });

                await _dbContext._dbContext.Set<CompanyFundChangeLog>().AddRangeAsync(logs);
                await _dbContext._dbContext.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }

            return await GetActiveAsync();
        }

        public async Task<PagedList<CompanyFundChangeLogDTO>> GetChangeHistoryAsync(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var query = _dbContext._dbContext.Set<CompanyFundChangeLog>()
                .AsNoTracking()
                .Include(x => x.ChangedByUser)
                .OrderByDescending(x => x.ChangedAt)
                .ThenByDescending(x => x.Id);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CompanyFundChangeLogDTO
                {
                    Id = x.Id,
                    FundId = x.FundId,
                    FundName = x.FundName,
                    Action = x.Action,
                    OldShare = x.OldShare,
                    NewShare = x.NewShare,
                    Details = x.Details,
                    ChangedAt = x.ChangedAt,
                    ChangedByUserId = x.ChangedByUserId,
                    ChangedByUserName = x.ChangedByUser != null ? x.ChangedByUser.FullName : null
                })
                .ToListAsync();

            return new PagedList<CompanyFundChangeLogDTO>(total, items, page, pageSize);
        }

        private static void ValidateFundSet(IEnumerable<(string Name, decimal Share)> funds)
        {
            var list = funds.ToList();
            if (list.Count == 0)
            {
                throw new DeliveryCoreException(
                    "يجب تعريف صندوق واحد على الأقل.",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }

            foreach (var fund in list)
            {
                if (string.IsNullOrWhiteSpace(fund.Name) || fund.Name.Length < 2 || fund.Name.Length > 100)
                {
                    throw new DeliveryCoreException(
                        "اسم الصندوق يجب أن يكون بين 2 و 100 حرف.",
                        ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                        null);
                }

                if (fund.Share <= 0 || fund.Share > 1)
                {
                    throw new DeliveryCoreException(
                        $"نسبة الصندوق «{fund.Name}» يجب أن تكون أكبر من 0% ولا تتجاوز 100%.",
                        ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                        null);
                }
            }

            var duplicate = list
                .GroupBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);
            if (duplicate != null)
            {
                throw new DeliveryCoreException(
                    $"اسم الصندوق مكرر: «{duplicate.Key}».",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }

            if (!RevenueSplit.SharesSumToOne(list.Select(f => f.Share)))
            {
                var totalPct = Math.Round(list.Sum(f => f.Share) * 100m, 2);
                throw new DeliveryCoreException(
                    $"مجموع نسب الصناديق يجب أن يساوي 100% من المتبقي بعد عمولة الموصل. المجموع الحالي: {totalPct}%.",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }
        }

        private static void NormalizeLastShare(List<CompanyFund> funds)
        {
            if (funds.Count == 0) return;
            var head = funds.Take(funds.Count - 1).Sum(f => f.Share);
            funds[^1].Share = RoundShare(Math.Max(0, 1m - head));
        }

        private static decimal RoundShare(decimal share) =>
            Math.Round(share, 6, MidpointRounding.AwayFromZero);

        private static string Percent(decimal share) =>
            $"{Math.Round(share * 100m, 2)}%";

        private static string BuildReason(string? reason, string detail)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return detail;
            return $"{detail} — السبب: {reason.Trim()}";
        }

        private static CompanyFundDTO MapFund(CompanyFund fund) => new()
        {
            Id = fund.Id,
            Name = fund.Name,
            Description = fund.Description,
            Share = fund.Share,
            SortOrder = fund.SortOrder,
            IsActive = fund.IsActive,
            CreatedAt = fund.CreatedAt,
            CreatedByUserName = fund.CreatedByUser?.FullName,
            UpdatedAt = fund.UpdatedAt,
            UpdatedByUserName = fund.UpdatedByUser?.FullName
        };
    }
}
