using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel;
using Demo_web_MVC.Models.ViewModel.Product;
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
            var products = await _context.Products.AsNoTracking().Include(p=>p.ProductImages).Select(p => new ProductViewModel
            {
                Id = p.Id,
                CategoryId = p.CategoryId,
                Name = p.Name,
                Description = p.Description,
                Brand = p.Brand,
                CreatedAt = p.CreatedAt,
                imageUrl = p.ProductImages.Select(pi => pi.Url).ToList() ?? new List<string>()
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
        public async Task <ProductViewModel> DetailsAsnyc (int id)
        {
            var product = await _context.Products.AsNoTracking().Where(p => p.Id == id).Select(p => new ProductViewModel
            {
                Id = p.Id,
                CategoryId = p.CategoryId,
                Name = p.Name,
                Description = p.Description,
                Brand = p.Brand,
                CreatedAt = p.CreatedAt,
                imageUrl= p.ProductImages.Select(pi => pi.Url).ToList() ?? new List<string>(),
                Variants = p.ProductVariants
                .Select(v => new ProductVariantsViewModel
                {
                    Id = v.Id,
                    ProductId = v.ProductId,
                    Size = v.Size,
                    Color = v.Color,
                    Price = v.Price,
                    Stock = v.Stock,
                    ImageUrlsVariants = v.ProductVariantImages.Select(pvi => pvi.Url).ToList() ?? new List<string>(),
                })
                .ToList()
            }).FirstOrDefaultAsync();
            return product!;
        }
        public async Task<ProductViewModel> AddAsnyc(ProductViewModel product)
        {
            var newProduct = new Models.Product
            {
                CategoryId = product.CategoryId,
                Name = product.Name,    
                Description = product.Description,
                Brand = product.Brand,
                CreatedAt = DateTime.Now,
                ProductImages = product.imageUrl?.Select(url => new ProductImage
                {
                    Url = $"/uploads/products/{url.Trim()}"  // Thêm tiền tố vào mỗi URL
                })
.ToList() ?? new List<ProductImage>(),
                ProductVariants = product.Variants?.Select(v => new ProductVariant
                {
                    Size = v.Size,
                    Color = v.Color,
                    Price = v.Price,
                    Stock = v.Stock,
                    ProductVariantImages = v.ImageUrlsVariants?.Select(url => new ProductVariantImage { Url = url }).ToList() ?? new List<ProductVariantImage>()
                }).ToList() ?? new List<ProductVariant>()
            };
            _context.Products.Add(newProduct);
            await _context.SaveChangesAsync();
            product.Id = newProduct.Id;
            return product;
        }
        public async Task<ProductViewModel> UpdateAsync(int id, ProductViewModel model)
        {
            var product = await _context.Products
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new Exception("Product not found");

            // Update thông tin chính
            product.CategoryId = model.CategoryId;
            product.Name = model.Name;
            product.Description = model.Description;
            product.Brand = model.Brand;

            var inputVariants = model.Variants ?? new List<ProductVariantsViewModel>();

            // ===== DELETE =====
            var toDelete = product.ProductVariants
                .Where(v => !inputVariants.Any(iv => iv.Id == v.Id))
                .ToList();

            _context.ProductVariants.RemoveRange(toDelete);

            // ===== UPDATE + ADD =====
            foreach (var variant in inputVariants)
            {
                var existing = product.ProductVariants
                    .FirstOrDefault(v => v.Id == variant.Id);

                if (existing != null)
                {
                    existing.Size = variant.Size;
                    existing.Color = variant.Color;
                    existing.Price = variant.Price;
                    existing.Stock = variant.Stock;
                }
                else
                {
                    product.ProductVariants.Add(new ProductVariant
                    {
                        Size = variant.Size,
                        Color = variant.Color,
                        Price = variant.Price,
                        Stock = variant.Stock
                    });
                }
            }

            await _context.SaveChangesAsync();

            return new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name
            };

        }
        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new Exception("Product not found");

            // Xóa variants trước (explicit)
            _context.ProductVariants.RemoveRange(product.ProductVariants);

            // Sau đó xóa product
            _context.Products.Remove(product);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}