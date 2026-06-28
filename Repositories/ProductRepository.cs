using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OMS.Data;
using OMS.Models;

namespace OMS.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product> GetByIdAsync(string sku)
        {
            return await _context.Products.FindAsync(sku);
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string sku)
        {
            var entity = await _context.Products.FindAsync(sku);
            if (entity != null)
            {
                _context.Products.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(List<Product> Data, int TotalCount)> SearchAsync(string? query, int page = 1, int pageSize = 20)
        {
            IQueryable<Product> q = _context.Products;

            if (!string.IsNullOrWhiteSpace(query))
            {
                var term = query.Trim().ToLower();
                q = q.Where(p => 
                    p.Sku.ToLower().Contains(term) || 
                    p.Name.ToLower().Contains(term) || 
                    p.Category.ToLower().Contains(term));
            }

            var totalCount = await q.CountAsync();

            if (page < 1) page = 1;
            var data = await q
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalCount);
        }
    }
}
