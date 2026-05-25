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
    }
}
