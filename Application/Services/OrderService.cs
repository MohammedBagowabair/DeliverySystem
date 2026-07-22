
using Application.DTO.ReportsDto;
using Application.Helpers;
using Application.Interfaces;
using Domain.Common.Models;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class OrderService : IOrderService
    {
        public  IApplicationDbContext _dbContext;
        public OrderService(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task<IEnumerable<Order>> GetAll()
        {
            return await _dbContext.GetAsync<Order>();
        }

        public async Task<Order> GetById(int id)
        {
            return (await _dbContext.GetAsync<Order>(x => x.Id == id))?.FirstOrDefault();
        }
        public async Task<Order> Create(Order order)
        {
            order.orderStatus = OrderStatus.Processing;
            // Keep client-provided DeliveryTime when set; otherwise stamp create time
            if (order.DeliveryTime == default)
            {
                order.DeliveryTime = DateTime.Now;
            }
            return await _dbContext.AddAsync<Order>(order);
        }
        public async Task Update(Order order)
        {
            var existing = await GetById(order.Id)
                ?? throw new DeliveryCoreException(ErrorCodes.DATABASE_RECORD_NOT_FOUND_CODE);

            if (!OrderStatusTransitions.CanTransition(existing.orderStatus, order.orderStatus))
            {
                throw new DeliveryCoreException(
                    $"Cannot change order status from {existing.orderStatus} to {order.orderStatus}. Allowed: {OrderStatusTransitions.DescribeAllowed(existing.orderStatus)}.",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }

            // DeliveryTime represents completion time once delivered
            if (existing.orderStatus != OrderStatus.Delivered && order.orderStatus == OrderStatus.Delivered)
            {
                order.DeliveryTime = DateTime.Now;
            }
            else if (order.orderStatus == OrderStatus.Processing && order.DeliveryTime == default)
            {
                order.DeliveryTime = existing.DeliveryTime;
            }
            else if (order.orderStatus != OrderStatus.Processing)
            {
                // Preserve original create/completion time for non-processing edits
                order.DeliveryTime = existing.DeliveryTime;
            }

            order.CustomerId = order.CustomerId == 0 ? existing.CustomerId : order.CustomerId;
            order.DriverId = order.DriverId == 0 ? existing.DriverId : order.DriverId;

            _dbContext._dbContext.Set<Order>().Update(order);
            await _dbContext._dbContext.SaveChangesAsync();
        }
        public async Task<bool> Delete(int id)
        {
            return await _dbContext.DeleteAsync<Order>(id);
        }
        public async Task<int> CountAsync()
        {
            return await _dbContext.CountAsync<Order>();
        }
        public async Task<PagedList<Order>> GetAllPagedAsync(int page, int PageSize)
        {
            return await _dbContext.GetPagedAsync<Order>(page, PageSize, null, ["Driver", "Customer"]);
        }

        public async Task<PagedList<Order>> TodayOrdersAsync(string searchTerm,int page,int pageSize,DateTime? startDate = null,DateTime? endDate = null)
        {
            searchTerm = searchTerm?.Trim()?.ToLower();

            var today = OrderDateRanges.Today();
            DateTime start = startDate ?? today.Start;
            DateTime end = endDate ?? today.End;

            var query = _dbContext._dbContext.Set<Order>()
                .Include(order => order.Driver)
                .Include(order => order.Customer)
                .Where(order =>
                    order.DeliveryTime >= start &&
                    order.DeliveryTime <= end
                    );

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(order =>
                    order.Customer.FullName.ToLower().Contains(searchTerm) ||
                    order.Driver.FullName.ToLower().Contains(searchTerm) ||
                    order.Title.ToLower().Contains(searchTerm));
            }

            int totalCount = await query.CountAsync();

            var orders = await query
                .OrderBy(order => order.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<Order>(totalCount, orders, page, pageSize);
        }
        public async Task<PagedList<Order>> GetAllOrdersByDriverId(int driverId,string searchTerm,int page,int pageSize,DateTime? startDate = null,DateTime? endDate = null)
        {
            // Normalize search term
            searchTerm = searchTerm?.Trim()?.ToLower();

            // Set default date range
            DateTime start = startDate ?? DateTime.Today;
            DateTime end = endDate ?? DateTime.Now.AddDays(1);

            // Build the query
            var query = _dbContext._dbContext.Set<Order>()
                .Include(order => order.Driver)
                .Include(order => order.Customer)
                .Where(order =>
                    order.DriverId == driverId &&
                    order.DeliveryTime >= start &&
                    order.DeliveryTime <= end&&
                    order.orderStatus==OrderStatus.Delivered &&
                    order.orderStatus == OrderStatus.Delivered);

            // Apply search term if provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(order =>
                    order.Customer.FullName.ToLower().Contains(searchTerm) ||
                    order.Driver.FullName.ToLower().Contains(searchTerm) ||
                    order.Title.ToLower().Contains(searchTerm));
            }

            // Get total count for pagination
            int totalCount = await query.CountAsync();

            // Apply pagination
            var orders = await query
                .OrderBy(order => order.Id) // or use OrderByDescending if required
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Return the paged list
            return new PagedList<Order>(totalCount, orders, page, pageSize);
        }

        public async Task<PagedList<Order>> GetLastWeekOrdersAsync(string searchTerm,int page,int pageSize,DateTime? startDate = null,DateTime? endDate = null)
        {
            searchTerm = searchTerm?.Trim()?.ToLower();

            var week = OrderDateRanges.Last7Days();
            DateTime start = startDate ?? week.Start;
            DateTime end = endDate ?? week.End;

            var query = _dbContext._dbContext.Set<Order>()
                .Include(order => order.Driver)
                .Include(order => order.Customer)
                .Where(order =>
                    order.DeliveryTime >= start &&
                    order.DeliveryTime <= end);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(order =>
                    order.Customer.FullName.ToLower().Contains(searchTerm) ||
                    order.Driver.FullName.ToLower().Contains(searchTerm) ||
                    order.Title.ToLower().Contains(searchTerm));
            }

            int totalCount = await query.CountAsync();

            var orders = await query
                .OrderBy(order => order.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<Order>(totalCount, orders, page, pageSize);
        }
        public async Task<PagedList<Order>> GetAllOrdersAsync(string searchTerm, int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                searchTerm = searchTerm?.Trim()?.ToLower();

                if (startDate == null && endDate == null)
                {
                    if (string.IsNullOrEmpty(searchTerm))
                    {
                        return await _dbContext.GetPagedAsync<Order>(page, pageSize, null, ["Driver", "Customer"], true);
                    }

                    return await _dbContext.GetPagedAsync<Order>(
                        page,
                        pageSize,
                        d => d.Customer.FullName.ToLower().Contains(searchTerm)
                             || d.Driver.FullName.ToLower().Contains(searchTerm)
                             || d.Title.ToLower().Contains(searchTerm),
                        ["Driver", "Customer"],
                        true);
                }

                DateTime start = startDate ?? DateTime.MinValue;
                DateTime end = endDate ?? DateTime.MaxValue;

                if (string.IsNullOrEmpty(searchTerm))
                {
                    return await _dbContext.GetPagedAsync<Order>(
                        page,
                        pageSize,
                        x => x.DeliveryTime >= start && x.DeliveryTime <= end,
                        ["Driver", "Customer"],
                        true);
                }

                return await _dbContext.GetPagedAsync<Order>(
                    page,
                    pageSize,
                    d => d.DeliveryTime >= start
                         && d.DeliveryTime <= end
                         && (d.Customer.FullName.ToLower().Contains(searchTerm)
                             || d.Driver.FullName.ToLower().Contains(searchTerm)
                             || d.Title.ToLower().Contains(searchTerm)),
                    ["Driver", "Customer"],
                    true);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<PagedList<Order>> GetPDFLastWeekOrdersAsync()
        {
            try
            {
                var week = OrderDateRanges.Last7Days();
                var query = _dbContext._dbContext.Set<Order>()
                    .Include(order => order.Driver)
                    .Include(order => order.Customer)
                    .Where(order =>
                        order.DeliveryTime >= week.Start &&
                        order.DeliveryTime <= week.End &&
                        order.orderStatus == OrderStatus.Delivered);

                var orders = await query.ToListAsync();
                return new PagedList<Order>(orders.Count, orders, 1, Math.Max(orders.Count, 1));
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching orders: " + ex.Message);
            }
        }
        public async Task<PagedList<Order>> GetPDFMonthWeekOrdersAsync()
        {
            try
            {
                var month = OrderDateRanges.CurrentMonthMtd();
                var query = _dbContext._dbContext.Set<Order>()
                    .Include(order => order.Driver)
                    .Include(order => order.Customer)
                    .Where(order =>
                        order.DeliveryTime >= month.Start &&
                        order.DeliveryTime <= month.End &&
                        order.orderStatus == OrderStatus.Delivered);

                var orders = await query.ToListAsync();
                return new PagedList<Order>(orders.Count, orders, 1, Math.Max(orders.Count, 1));
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching orders: " + ex.Message);
            }
        }
        public async Task<PagedList<Order>> LastMonthOrdersAsync(string searchTerm, int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null)
        {
            searchTerm = searchTerm?.Trim()?.ToLower();

            var month = OrderDateRanges.CurrentMonthMtd();
            DateTime start = startDate ?? month.Start;
            DateTime end = endDate ?? month.End;

            var query = _dbContext._dbContext.Set<Order>()
                .Include(order => order.Driver)
                .Include(order => order.Customer)
                .Where(order =>
                    order.DeliveryTime >= start &&
                    order.DeliveryTime <= end
                    );

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(order =>
                    order.Customer.FullName.ToLower().Contains(searchTerm) ||
                    order.Driver.FullName.ToLower().Contains(searchTerm) ||
                    order.Title.ToLower().Contains(searchTerm));
            }

            int totalCount = await query.CountAsync();

            var orders = await query
                .OrderBy(order => order.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<Order>(totalCount, orders, page, pageSize);
        }


        public async Task<int> GetTotalProcessingOrdersAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            DateTime? Start = startDate;
            DateTime? End = endDate;

            return await _dbContext.CountAsync<Order>(
                order => order.orderStatus == OrderStatus.Processing &&
                         order.DeliveryTime >= Start &&
                         order.DeliveryTime <= End);
        }
        public async Task<int> GetTotalDileveredOrdersAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            DateTime? Start = startDate;
            DateTime? End = endDate;

            return await _dbContext.CountAsync<Order>(
                order => order.orderStatus == OrderStatus.Delivered &&
                          order.DeliveryTime >= Start &&
                         order.DeliveryTime <= End);
        }
        public async Task<int> GetTotalCanceledOrdersAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            DateTime? Start = startDate;
            DateTime? End = endDate;

            return await _dbContext.CountAsync<Order>(
                order => order.orderStatus == OrderStatus.Canceled &&
                          order.DeliveryTime >= Start &&
                         order.DeliveryTime <= End);
        }

        // Filtered Data

        public async Task<PagedList<Order>> GetAllOrdersFiltredAsync(
    int page,
    int pageSize,
    string searchTerm = null,
    DateTime? startDate = null,
    DateTime? endDate = null,
    string SelectedDriver = null,
    OrderStatus? SelectedStatus = null)
        {
            try
            {
                // Default to MinValue and MaxValue if dates are null
                DateTime start = startDate ?? DateTime.MinValue;
                DateTime end = endDate ?? DateTime.MaxValue;

                // Normalize search term
                searchTerm = searchTerm?.Trim()?.ToLower();

                // Build filter predicate dynamically
                var predicate = PredicateBuilder.New<Order>(true);

                // Apply filters based on provided parameters
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    predicate = predicate.And(order =>
                        order.Customer.FullName.ToLower().Contains(searchTerm) ||
                        order.Driver.FullName.ToLower().Contains(searchTerm) ||
                        order.Title.ToLower().Contains(searchTerm));
                }

                predicate = predicate.And(order => order.DeliveryTime >= start && order.DeliveryTime <= end);

                if (!string.IsNullOrEmpty(SelectedDriver))
                {
                    predicate = predicate.And(order => order.Driver.Id.ToString() == SelectedDriver);
                }

                if (SelectedStatus.HasValue)
                {
                    predicate = predicate.And(order => order.orderStatus == SelectedStatus.Value);
                }

                // Query the database using the predicate and include related entities
                var pagedOrders = await _dbContext.GetPagedAsync<Order>(
                    page,
                    pageSize,
                    predicate,
                    new[] { "Driver", "Customer" }, // Include related entities
                    true
                );

                // Map to DTOs
                var pagedOrderDTOs = new PagedList<Order>(
                    pagedOrders.TotalCount,
                    pagedOrders.Entities.Select(order => new Order
                    {
                        Id = order.Id,
                        Title = order.Title,
                        FinalPrice = order.FinalPrice,
                        DeliveryTime = order.DeliveryTime,
                        DeliveryFee = order.DeliveryFee,
                        CouponDiscount = order.CouponDiscount,
                        orderStatus = order.orderStatus,
                        PaymentMethod = order.PaymentMethod,
                        Notice=order.Notice,
                        
                        Customer = new Customer
                        {
                            Id = order.Customer.Id,
                            FullName = order.Customer.FullName,
                            PhoneNumber1 = order.Customer.PhoneNumber1,
                            PhoneNumber2 = order.Customer.PhoneNumber2,
                            Address=order.Customer.Address
                        },
                        Driver = new Driver
                        {
                            Id = order.Driver.Id,
                            FullName = order.Driver.FullName,
                            PhoneNumber1 = order.Driver.PhoneNumber1,
                            PhoneNumber2 = order.Driver.PhoneNumber2,
                            Address=order.Driver.Address
                        }
                    }).ToList(),
                    page,
                    pageSize
                );

                return pagedOrderDTOs;
            }
            catch (Exception ex)
            {
                throw new Exception("jh");
            }
        }




        public async Task<DriverReportResult> GetDriverReportsAsync(
        int? driverId,
        int page,
        int pageSize,
        string searchTerm = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? SelectedDriver = null,
        OrderStatus? SelectedStatus = null)
        {
            try
            {
                DateTime start = startDate ?? DateTime.MinValue;
                DateTime end = endDate ?? DateTime.MaxValue;
                searchTerm = searchTerm?.Trim()?.ToLower();

                // Prefer explicit SelectedDriver; fall back to driverId
                int? effectiveDriverId = SelectedDriver ?? driverId;

                var predicate = PredicateBuilder.New<Order>(true);

                if (effectiveDriverId.HasValue)
                {
                    predicate = predicate.And(order => order.DriverId == effectiveDriverId.Value);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    predicate = predicate.And(order =>
                        order.Customer.FullName.ToLower().Contains(searchTerm) ||
                        order.Driver.FullName.ToLower().Contains(searchTerm) ||
                        order.Title.ToLower().Contains(searchTerm));
                }

                predicate = predicate.And(order => order.DeliveryTime >= start && order.DeliveryTime <= end);

                if (SelectedStatus.HasValue)
                {
                    predicate = predicate.And(order => order.orderStatus == SelectedStatus.Value);
                }

                var pagedOrders = await _dbContext.GetPagedAsync<Order>(
                    page,
                    pageSize,
                    predicate,
                    new[] { "Driver", "Customer" },
                    true
                );

                var query = _dbContext._dbContext.Set<Order>()
                    .Where(order =>
                        order.orderStatus == OrderStatus.Delivered &&
                        order.DeliveryTime >= start &&
                        order.DeliveryTime <= end
                    );

                if (effectiveDriverId.HasValue)
                {
                    query = query.Where(order => order.DriverId == effectiveDriverId.Value);
                }

                decimal totalRevenue = await query.SumAsync(order => order.FinalPrice);
                decimal driverProfit = RevenueSplit.DriverProfit(totalRevenue);
                decimal companyProfit = RevenueSplit.CompanyProfit(totalRevenue);
                decimal expenses = RevenueSplit.Expenses(totalRevenue);

                var pagedOrderDTOs = new PagedList<Order>(
                   pagedOrders.TotalCount,
                   pagedOrders.Entities.Select(order => new Order
                   {
                       Id = order.Id,
                       Title = order.Title,
                       FinalPrice = order.FinalPrice,
                       DeliveryTime = order.DeliveryTime,
                       orderStatus = order.orderStatus,
                       PaymentMethod = order.PaymentMethod,
                       Notice = order.Notice,
                       Customer = new Customer
                       {
                           Id = order.Customer.Id,
                           FullName = order.Customer.FullName,
                       },
                       Driver = new Driver
                       {
                           Id = order.Driver.Id,
                           FullName = order.Driver.FullName,
                       }
                   }).ToList(),
                   page,
                   pageSize
               );

                return new DriverReportResult
                {
                    Orders = pagedOrderDTOs,
                    DriverProfit = driverProfit,
                    CompanyProfit = companyProfit,
                    CompanyRevenue = totalRevenue,
                    expinses = expenses,
                };
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching driver reports.", ex);
            }
        }


    }
}
