using System.Collections.Generic;
using System.Threading.Tasks;
using OMS.Models;

namespace OMS.Repositories
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetAllAsync();
        Task<Order> GetByIdAsync(string id);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(string id);
        Task BulkUpdateAsync(string id, string newStatus, DateTime? arrivalDate, decimal? importPrice, decimal? paidAmount);
        
        /// <summary>
        /// Searches and filters orders at the database level with server-side pagination.
        /// Uses PostgreSQL GIN full-text search if query is provided.
        /// Returns the paginated data, total count, and a dictionary of counts by Status.
        /// </summary>
        Task<(List<Order> Data, int TotalCount, Dictionary<string, int> StatusCounts)> SearchOrdersAsync(
            string? query, 
            List<string>? statuses, 
            List<string>? carriers, 
            DateTime? fromDate, 
            DateTime? toDate,
            int page = 1,
            int pageSize = 20);
    }
}
