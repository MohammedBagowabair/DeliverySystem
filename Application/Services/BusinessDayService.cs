using Application.DTO.BusinessDayDtos;
using Application.Helpers;
using Application.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    /// <summary>
    /// Business day follows the calendar, but previous open days must be closed
    /// before the current calendar day becomes active. Never prompts to jump to "tomorrow"
    /// when catching up from a lagged business day.
    /// </summary>
    public class BusinessDayService : IBusinessDayService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IDayCloseService _dayCloseService;
        private readonly IYearCloseService _yearCloseService;

        public BusinessDayService(
            IApplicationDbContext dbContext,
            IDayCloseService dayCloseService,
            IYearCloseService yearCloseService)
        {
            _dbContext = dbContext;
            _dayCloseService = dayCloseService;
            _yearCloseService = yearCloseService;
        }

        public async Task<DateTime> GetCurrentBusinessDateAsync()
        {
            var state = await EnsureStateAsync();
            return state.CurrentBusinessDate.Date;
        }

        public async Task<BusinessDayStateDTO> GetStateAsync()
        {
            var state = await EnsureStateAsync();
            return await MapStateAsync(state);
        }

        /// <summary>
        /// Close the lagged open business day and move current business day to calendar today
        /// (or to the next day only when already aligned with today — day-end close).
        /// </summary>
        public async Task<AdvanceBusinessDayResultDTO> AdvanceToNextDayAsync(
            int userId,
            string? notes = null,
            bool continueAnyway = false)
        {
            _ = continueAnyway;

            if (userId <= 0)
            {
                throw new DeliveryCoreException(
                    "تعذر تحديد المستخدم الذي ينقل يوم العمل.",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }

            var state = await EnsureStateAsync();
            var current = state.CurrentBusinessDate.Date;
            var today = YemenTime.Today;
            var pending = await CountPendingAsync(current);

            if (pending > 0)
            {
                var targetLabel = current < today
                    ? today.ToString("yyyy/MM/dd")
                    : current.AddDays(1).ToString("yyyy/MM/dd");

                return new AdvanceBusinessDayResultDTO
                {
                    Success = false,
                    PreviousBusinessDate = current,
                    PendingOrderCount = pending,
                    Message =
                        $"توجد طلبات تحتاج إلى إجراء قبل إغلاق يوم العمل.{Environment.NewLine}" +
                        $"يوم العمل: {current:yyyy/MM/dd}{Environment.NewLine}" +
                        $"عدد الطلبات: {pending}{Environment.NewLine}" +
                        $"يرجى مراجعة هذه الطلبات قبل البدء بيوم {targetLabel}.",
                    State = await MapStateAsync(state)
                };
            }

            // Close + archive only when catching up: business day is before calendar today.
            // While aligned with calendar today, keep the day open — do not close/advance to tomorrow.
            if (current >= today)
            {
                return new AdvanceBusinessDayResultDTO
                {
                    Success = false,
                    PreviousBusinessDate = current,
                    NewBusinessDate = current,
                    PendingOrderCount = pending,
                    Message = current == today
                        ? "يوم العمل الحالي هو اليوم — يبقى مفتوحاً. يُغلق ويُؤرشف عند الانتقال لليوم التالي بعد تغيير التقويم."
                        : $"يوم العمل الحالي هو {current:yyyy/MM/dd}.",
                    State = await MapStateAsync(state)
                };
            }

            var target = today;

            if (await _yearCloseService.IsClosedAsync(target.Year))
            {
                throw new DeliveryCoreException(
                    $"لا يمكن الانتقال إلى {target:yyyy/MM/dd} لأن سنة {target.Year} مغلقة ومؤرشفة.",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }

            if (!await _dayCloseService.IsClosedAsync(current))
            {
                await _dayCloseService.CloseDayAsync(current, userId, notes, allowOpenBusinessDay: true);
            }

            // If lagging more than one day, close intermediate empty days without orders? 
            // Spec: close yesterday → start today. Intermediate days get closed as we step.
            var cursor = current;
            while (cursor < target)
            {
                var step = cursor.AddDays(1);
                if (step > target)
                    break;
                if (step < target && !await _dayCloseService.IsClosedAsync(step))
                {
                    // Skip creating empty closed days for gaps with no activity — jump to today
                    break;
                }
                cursor = step;
            }

            state.CurrentBusinessDate = target;
            state.UpdatedAt = YemenTime.Now;
            state.UpdatedByUserId = userId;
            await _dbContext._dbContext.SaveChangesAsync();

            var mapped = await MapStateAsync(state);

            return new AdvanceBusinessDayResultDTO
            {
                Success = true,
                PreviousBusinessDate = current,
                NewBusinessDate = target,
                PendingOrderCount = 0,
                Message = $"تم إغلاق وأرشفة يوم {current:yyyy/MM/dd} وبدء يوم العمل {target:yyyy/MM/dd}.",
                State = mapped
            };
        }

        private async Task<BusinessDayState> EnsureStateAsync()
        {
            var set = _dbContext._dbContext.Set<BusinessDayState>();
            var state = await set.OrderBy(x => x.Id).FirstOrDefaultAsync();
            if (state != null)
                return state;

            state = new BusinessDayState
            {
                CurrentBusinessDate = YemenTime.Today,
                UpdatedAt = YemenTime.Now
            };
            await set.AddAsync(state);
            await _dbContext._dbContext.SaveChangesAsync();
            return state;
        }

        private async Task<int> CountPendingAsync(DateTime businessDate)
        {
            var start = YemenTime.StartOfDay(businessDate);
            var end = YemenTime.EndOfDay(businessDate);
            return await _dbContext._dbContext.Set<Order>()
                .CountAsync(o =>
                    o.orderStatus == OrderStatus.Processing &&
                    o.DeliveryTime >= start &&
                    o.DeliveryTime <= end);
        }

        private async Task<BusinessDayStateDTO> MapStateAsync(BusinessDayState state)
        {
            var day = state.CurrentBusinessDate.Date;
            var today = YemenTime.Today;
            var pending = await CountPendingAsync(day);
            var isClosed = await _dayCloseService.IsClosedAsync(day);

            var isLagging = day < today;
            var needsCatchUp = isLagging;
            var isAligned = day == today;

            string? block = null;
            // Advance/close only when lagging behind calendar (and no pending). Never close "today" while it is still today.
            var canAdvance = pending == 0 && needsCatchUp;

            DateTime? suggestedTarget = null;
            string? alertTitle = null;
            string? alertBody = null;

            if (pending > 0)
            {
                canAdvance = false;
                block = $"توجد طلبات تحتاج إلى إجراء قبل إغلاق يوم العمل.";
                alertTitle = "طلبات معلّقة";
                alertBody =
                    $"توجد طلبات تحتاج إلى إجراء قبل إغلاق يوم العمل.{Environment.NewLine}" +
                    $"يوم العمل: {day:d MMMM yyyy}{Environment.NewLine}" +
                    $"عدد الطلبات: {pending}";
                suggestedTarget = isLagging ? today : null;
            }
            else if (needsCatchUp)
            {
                suggestedTarget = today;
                if (await _yearCloseService.IsClosedAsync(today.Year))
                {
                    canAdvance = false;
                    block = $"السنة الحالية ({today.Year}) مغلقة.";
                    suggestedTarget = null;
                }
                else
                {
                    alertTitle = "يوم عمل سابق مفتوح";
                    alertBody =
                        $"يوجد يوم عمل سابق لم يتم إغلاقه.{Environment.NewLine}" +
                        $"يوم العمل: {day:d MMMM yyyy}{Environment.NewLine}" +
                        $"سيتم إغلاقه وأرشفته تلقائياً عند البدء بيوم {today:d MMMM yyyy}.";
                }
            }
            else if (isAligned)
            {
                // Keep today open. Closing + archive happens only after calendar moves to the next day.
                canAdvance = false;
                suggestedTarget = null;
            }
            else
            {
                // day > today — unusual; no prompt
                canAdvance = false;
                block = $"يوم العمل ({day:yyyy/MM/dd}) بعد تاريخ اليوم.";
            }

            string? userName = null;
            if (state.UpdatedByUserId is int uid && uid > 0)
            {
                userName = await _dbContext._dbContext.Set<User>()
                    .AsNoTracking()
                    .Where(u => u.Id == uid)
                    .Select(u => u.FullName)
                    .FirstOrDefaultAsync();
            }

            return new BusinessDayStateDTO
            {
                CurrentBusinessDate = day,
                CalendarToday = today,
                UpdatedAt = state.UpdatedAt,
                UpdatedByUserId = state.UpdatedByUserId,
                UpdatedByUserName = userName,
                IsClosed = isClosed,
                PendingOrderCount = pending,
                CanAdvance = canAdvance,
                BlockReason = block,
                IsLaggingBehindCalendar = needsCatchUp,
                NeedsCatchUp = needsCatchUp && pending == 0,
                SuggestedTargetDate = suggestedTarget,
                AlertTitle = alertTitle,
                AlertBody = alertBody,
                PrimaryActionLabel = needsCatchUp && canAdvance
                    ? $"إغلاق وأرشفة يوم {day:d MMMM} وبدء {today:d MMMM}"
                    : null
            };
        }
    }
}
