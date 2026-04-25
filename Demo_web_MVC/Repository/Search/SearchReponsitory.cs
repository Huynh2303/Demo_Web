using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models.ViewModel.Category;
using Demo_web_MVC.Models.ViewModel.Product;
using Demo_web_MVC.Models.ViewModel.Search;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
namespace Demo_web_MVC.Repository.Search
{  
    public class SearchReponsitory:ISearchReponsitory
    {
        public readonly AppDatabase _context;
        public readonly ILogger<SearchReponsitory> _logger;
        public readonly HttpClient _httpClient;
        public SearchReponsitory(AppDatabase context, ILogger<SearchReponsitory> logger, HttpClient httpClient)
        {
            _context = context;
            _logger = logger;
            _httpClient = httpClient;
        }
        public async Task<SearchViewModel> SearchAsync(string searchQuery)
        {
            // Kiểm tra nếu searchQuery là null hoặc trống
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return new SearchViewModel
                {
                    ErrorMessage = "Please enter a valid search query.",
                    SearchStatus = "Error"
                };
            }

            // Thực hiện tìm kiếm bất đồng bộ mà không sử dụng phân trang
            var products = await _context.Products
                .Where(p => p.Name.Contains(searchQuery) || (p.Description ?? "").Contains(searchQuery))
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    imageUrl = p.ProductImages.Select(pi => pi.Url).ToList() ?? new List<string>(),
                    Variants = p.ProductVariants.Select(v => new ProductVariantsViewModel
                    {
                        Id = v.Id,
                        ProductId = v.ProductId,
                        Color = v.Color,
                        Price = v.Price,
                        Stock = v.Stock,
                        ImageUrlsVariants = v.ProductVariantImages.Select(pvi => pvi.Url).ToList() ?? new List<string>()
                    }).ToList()
                })
                .ToListAsync();  // Lấy toàn bộ kết quả tìm kiếm

            // Tính tổng số sản phẩm (nếu cần để hiển thị số lượng kết quả)
            var totalResults = products.Count;

            // Tạo ViewModel chứa kết quả tìm kiếm
            var viewModel = new SearchViewModel
            {
                SearchQuery = searchQuery,
                ProductVMResults = products,
                TotalResults = totalResults, // Tổng số kết quả tìm kiếm
                SearchStatus = "Success", // Trạng thái tìm kiếm thành công
                ErrorMessage = products.Any() ? null : "No results found for the search query." // Nếu không có kết quả, thông báo lỗi
            };

            return viewModel;
        }

    }
}
