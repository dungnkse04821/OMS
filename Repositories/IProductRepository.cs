using System.Collections.Generic;
using System.Threading.Tasks;
using OMS.Models;

namespace OMS.Repositories
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(string sku);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(string sku);
        
        Task<(List<Product> Data, int TotalCount)> SearchAsync(string? query, int page = 1, int pageSize = 20);
    }
}
