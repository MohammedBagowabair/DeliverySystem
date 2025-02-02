using Application.DTO.OrderDtos;
using Application.DTO.ReportsDto;
using Domain.Common.Models;
using Domain.Constants;
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

        //Filtered Data
        Task<PagedList<Order>> GetAllOrdersFiltredAsync(int page,int pageSize,string? searchTerm = null,DateTime? startDate = null,DateTime? endDate = null,string? SelectedDriver = null,OrderStatus? SelectedStatus = null);
        Task<DriverReportResult> GetDriverReportsAsync(int? id ,int page,int pageSize,string? searchTerm = null,DateTime? startDate = null,DateTime? endDate = null,int? SelectedDriver = null,OrderStatus? SelectedStatus = null);




    }
}
