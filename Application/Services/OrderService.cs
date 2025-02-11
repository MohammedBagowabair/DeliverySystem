
using Application.DTO.ReportsDto;
using Application.Interfaces;
using Domain.Common.Models;
using Domain.Constants;
using Domain.Entities;
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
            order.DeliveryTime = DateTime.Now;
            order.orderStatus = OrderStatus.Processing;
            return await _dbContext.AddAsync<Order>(order);
        }
        public async Task Update(Order order)
        {
            //try
            //{

            //    if (order.orderStatus == OrderStatus.Processing)
            //    {
            //        order.orderStatus = OrderStatus.Processing;
            //    }
            //    if (order.orderStatus == OrderStatus.Delivered)
            //    {
            //        order.orderStatus = OrderStatus.Delivered;

            //    }
            //    if (order.orderStatus == OrderStatus.Canceled)
            //    {
            //        order.orderStatus = OrderStatus.Canceled;
            //    }

              _dbContext._dbContext.Set<Order>().Update(order);
              await  _dbContext._dbContext.SaveChangesAsync();
               // throw new Exception("nkn");

            //}
            //catch (Exception ex)
            //{
            //    throw new Exception(ex.Message);s
            //}
            
           
        }
        public async Task<bool> Delete(int id)
        {
            return await _dbContext.DeleteAsync<Order>(id);
        }
        public async Task<int> CountAsync()
        {
            var totalRecord = await GetAll();
            int total = 0;
            foreach (var record in totalRecord)
            {
                total += 1;
            }
            return total;
        }
        public async Task<PagedList<Order>> GetAllPagedAsync(int page, int PageSize)
        {
            return await _dbContext.GetPagedAsync<Order>(page, PageSize, null, ["Driver", "Customer"]);
        }

        public async Task<PagedList<Order>> TodayOrdersAsync(string searchTerm,int page,int pageSize,DateTime? startDate = null,DateTime? endDate = null)
        {
            // Normalize search term
            searchTerm = searchTerm?.Trim()?.ToLower();

            // Default to today's orders
            DateTime start = startDate ?? DateTime.Today; // Start of the current day
            DateTime end = endDate ?? DateTime.Today.AddDays(1); // Start of the next day

            // Build the query
            var query = _dbContext._dbContext.Set<Order>()
                .Include(order => order.Driver)
                .Include(order => order.Customer)
                .Where(order =>
                    order.DeliveryTime >= start &&
                    order.DeliveryTime < end 
                    );

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
                .OrderBy(order => order.Id) // Use OrderByDescending if needed
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Return the paged list
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
            // Normalize search term
            searchTerm = searchTerm?.Trim()?.ToLower();

            DateTime start = startDate ?? DateTime.Today.AddDays(-7);
            DateTime end = DateTime.Today.AddDays(1);

            var query = _dbContext._dbContext.Set<Order>()
                .Include(order => order.Driver)
                .Include(order => order.Customer)
                .Where(order =>
                    order.DeliveryTime >= start &&
                    order.DeliveryTime < end &&
                    order.orderStatus == OrderStatus.Delivered);

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
                .OrderBy(order => order.Id) // Use OrderByDescending if needed
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Return the paged list
            return new PagedList<Order>(totalCount, orders, page, pageSize);
        }
        public async Task<PagedList<Order>> GetAllOrdersAsync(string searchTerm, int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                DateTime start = startDate ?? startDate.Value;
                DateTime end = endDate ?? endDate.Value;
                if (startDate == null && endDate == null)
                {
                    searchTerm = searchTerm?.Trim()?.ToLower();
                    if (string.IsNullOrEmpty(searchTerm))
                    {
                        //return await _dbContext._dbContext.Set<Order>().Where(x=>x.CustomerId>10).ToListAsync();
                        return await _dbContext.GetPagedAsync<Order>(page, pageSize, null, ["Driver", "Customer"], true);
                    }


                    // Use predicate for search
                    return await _dbContext.GetPagedAsync<Order>(
                        page,
                        pageSize, d => d.Customer.FullName.ToLower().Contains(searchTerm) || d.Driver.FullName.Contains(searchTerm) || d.Title.Contains(searchTerm), ["Driver", "Customer"], true
                    );
                }
                else
                {
                    // Normalize search term
                    searchTerm = searchTerm?.Trim()?.ToLower();
                    if (string.IsNullOrEmpty(searchTerm))
                    {
                        //return await _dbContext._dbContext.Set<Order>().Where(x=>x.CustomerId>10).ToListAsync();
                        return await _dbContext.GetPagedAsync<Order>(page, pageSize, x => x.DeliveryTime >= start && x.DeliveryTime <= end, ["Driver", "Customer"], true);
                    }


                    // Use predicate for search
                    return await _dbContext.GetPagedAsync<Order>(
                        page,
                        pageSize, d => d.DeliveryTime >= start && d.DeliveryTime <= end || d.Customer.FullName.ToLower().Contains(searchTerm) || d.Driver.FullName.Contains(searchTerm) || d.Title.Contains(searchTerm), ["Driver", "Customer"], true
                    );
                }
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
                // Set default date range for the last 7 days
                DateTime start = DateTime.Today.AddDays(-7); // 7 days ago from the current moment
                DateTime end = DateTime.Today.AddDays(2).AddTicks(-1);
                // Build the query
                var query = _dbContext._dbContext.Set<Order>()
                    .Include(order => order.Driver)
                    .Include(order => order.Customer)
                    .Where(order =>
                        order.DeliveryTime >= start &&
                        order.DeliveryTime <= end &&
                        order.orderStatus == OrderStatus.Delivered);

                // Get all the orders within this period (no search term or pagination)
                var orders = await query.ToListAsync();

                // Return the list of orders in a paged format (with one page, containing all the records)
                return new PagedList<Order>(orders.Count, orders, 1, orders.Count); // Return all orders as a single page
            }
            catch (Exception ex)
            {
                // Handle errors, if any
                throw new Exception("Error fetching orders: " + ex.Message);
            }
        }
        public async Task<PagedList<Order>> GetPDFMonthWeekOrdersAsync()
        {
            try
            {
                DateTime start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); // First day of the current month
                DateTime end = DateTime.Today.AddDays(1);
                // Build the query
                var query = _dbContext._dbContext.Set<Order>()
                    .Include(order => order.Driver)
                    .Include(order => order.Customer)
                    .Where(order =>
                        order.DeliveryTime >= start &&
                        order.DeliveryTime <= end &&
                        order.orderStatus == OrderStatus.Delivered);

                // Get all the orders within this period (no search term or pagination)
                var orders = await query.ToListAsync();

                // Return the list of orders in a paged format (with one page, containing all the records)
                return new PagedList<Order>(orders.Count, orders, 1, orders.Count); // Return all orders as a single page
            }
            catch (Exception ex)
            {
                // Handle errors, if any
                throw new Exception("Error fetching orders: " + ex.Message);
            }
        }
        public async Task<PagedList<Order>> LastMonthOrdersAsync(string searchTerm, int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null)
        {
            // Normalize search term
            searchTerm = searchTerm?.Trim()?.ToLower();

            // Default to today's orders
            DateTime start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); // First day of the current month
            DateTime end = DateTime.Today.AddDays(1);

            // Build the query
            var query = _dbContext._dbContext.Set<Order>()
                .Include(order => order.Driver)
                .Include(order => order.Customer)
                .Where(order =>
                    order.DeliveryTime >= start &&
                    order.DeliveryTime < end
                    );

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
                .OrderBy(order => order.Id) // Use OrderByDescending if needed
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Return the paged list
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

                return pagedOrderDTOs;
            }
            catch (Exception ex)
            {
                throw new Exception("jh");
            }
        }




        public async Task<DriverReportResult> GetDriverReportsAsync(
        int? driverId, // Filter by Driver ID
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
                // Default to MinValue and MaxValue if dates are null
                DateTime start = startDate ?? DateTime.MinValue;
                DateTime end = endDate ?? DateTime.MaxValue;

                // Normalize search term
                searchTerm = searchTerm?.Trim()?.ToLower();

                // Build filter predicate dynamically
                var predicate = PredicateBuilder.New<Order>(true);

                // Filter by Driver ID if provided
                if (driverId.HasValue)
                {
                    predicate = predicate.And(order => order.Driver.Id == SelectedDriver.Value);
                }

                // Apply search filters
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    predicate = predicate.And(order =>
                        order.Customer.FullName.ToLower().Contains(searchTerm) ||
                        order.Driver.FullName.ToLower().Contains(searchTerm) ||
                        order.Title.ToLower().Contains(searchTerm));
                }

                predicate = predicate.And(order => order.DeliveryTime >= start && order.DeliveryTime <= end);

                if (!string.IsNullOrEmpty(SelectedDriver.ToString()))
                {
                    predicate = predicate.And(order => order.Driver.Id == SelectedDriver);
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

                // Query to get delivered orders within the given day
                var query = _dbContext._dbContext.Set<Order>()
                    .Where(order =>
                        order.orderStatus == OrderStatus.Delivered &&
                        order.DeliveryTime >= start &&
                        order.DeliveryTime <= end
                    );

                // If a specific driver is selected, filter orders by that driver
                if (SelectedDriver.HasValue)
                {
                    query = query.Where(order => order.DriverId == SelectedDriver);
                }

                // Calculate total revenue from the filtered orders
                decimal totalRevenue = await query.SumAsync(order => order.FinalPrice);

                // Calculate driver profit (70% of revenue from their orders)
                decimal driverProfit = totalRevenue * 0.7m;

                // Calculate company profit (30% of revenue from the same orders)
                decimal companyProfit = totalRevenue * 0.3m;





                //Map to DTOs
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
                    CompanyRevenue = totalRevenue
                };
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching driver reports.", ex);
            }
        }


    }
}
