using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Models;

namespace OMS.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetAllAsync()
        {
            return await _context.Orders.ToListAsync();
        }

        public async Task<Order> GetByIdAsync(string id)
        {
            return await _context.Orders.FindAsync(id);
        }

        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await _context.Orders.FindAsync(id);
            if (entity != null)
            {
                _context.Orders.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task BulkUpdateAsync(string id, string newStatus, System.DateTime? arrivalDate, decimal? importPrice, decimal? paidAmount)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return;
            if (!string.IsNullOrEmpty(newStatus)) order.Status = newStatus;
            if (arrivalDate.HasValue) order.ArrivalDate = arrivalDate;
            if (importPrice.HasValue) order.ImportPrice = importPrice.Value;
            // paidAmount could be stored in Deposit field as an example
            if (paidAmount.HasValue) order.Deposit = paidAmount.Value;
            await _context.SaveChangesAsync();
        }
    }
}
