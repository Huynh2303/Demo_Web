using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace Demo_web_MVC.Repository.Product
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDatabase _context;
        public ProductRepository(AppDatabase context)
        {
            _context = context;
        }

        public async Task<List<ProductViewModel>> GetAllAsync()
        {
            var products = await _context.Products.AsNoTracking().Select(p => new ProductViewModel
            {
                Id = p.Id,
                CategoryId = p.CategoryId,
                Name = p.Name,
                Description = p.Description,
                Brand = p.Brand,
                CreatedAt = p.CreatedAt
            }).ToListAsync();
            return products;
        }
        public async Task<ProductViewModel> GetByIdAsync(int id)
        {
            var product = await _context.Products.AsNoTracking().Where(p => p.Id == id).Select(p => new ProductViewModel
            {
                Id = p.Id,
                CategoryId = p.CategoryId,
                Name = p.Name,
                Description = p.Description,
                Brand = p.Brand,
                CreatedAt = p.CreatedAt
                
            }).FirstOrDefaultAsync();
            return product!;
        }
    } 
}
