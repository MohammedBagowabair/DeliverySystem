using Domain.Common.Models;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAll();
        Task<Order> GetById(int id);
        Task<Order> Create(Order order);
        Task Update(Order order);
        Task<bool> Delete(int id);
        Task<PagedList<Order>> GetAllPagedAsync(int page, int PageSize);
        Task<PagedList<Order>> GetAllOrdersByDriverId(int driverId, string searchTerm, int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null);
        Task<PagedList<Order>> TodayOrdersAsync(string searchTerm, int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null);

        Task<PagedList<Order>> GetLastWeekOrdersAsync(string searchTerm, int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null);
        Task<PagedList<Order>> GetAllOrdersAsync(string searchTerm, int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null);
        Task<PagedList<Order>> GetPDFLastWeekOrdersAsync();
        Task<PagedList<Order>> GetPDFMonthWeekOrdersAsync();

        Task<PagedList<Order>> LastMonthOrdersAsync(string searchTerm, int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null);


        // Count Today Orders
        Task<int> GetTotalProcessingOrdersAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetTotalDileveredOrdersAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetTotalCanceledOrdersAsync(DateTime? startDate = null, DateTime? endDate = null);



    }
}
