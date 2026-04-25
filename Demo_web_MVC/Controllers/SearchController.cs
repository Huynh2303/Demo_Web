using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Product;
using Demo_web_MVC.Models.ViewModel.Search;
using Demo_web_MVC.Service.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Demo_web_MVC.Controllers
{
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<SearchController> _logger;
        public SearchController(ISearchService searchService, ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Search(string searchQuery)
        {
            // Log khi bắt đầu tìm kiếm
            _logger.LogInformation("Search started for query: {SearchQuery}", searchQuery);

            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                // Log khi không có query đầu vào
                _logger.LogWarning("Search query is empty or whitespace.");

                ModelState.AddModelError(string.Empty, "Please enter a search query.");
                return View("Index", new SearchViewModel { ErrorMessage = "Please enter a search query." });
            }

            var ProductVMResults = await _searchService.SearchAsync(searchQuery);

            // Log kết quả tìm kiếm
            if (ProductVMResults?.ProductVMResults?.Any() != true)
            {
                _logger.LogInformation("No results found for query: {SearchQuery}", searchQuery);
            }
            else
            {
                _logger.LogInformation("{ResultsCount} results found for query: {SearchQuery}", ProductVMResults.ProductVMResults.Count(), searchQuery);
            }

            var viewModel = new SearchViewModel
            {
                SearchQuery = searchQuery,
                ProductVMResults = ProductVMResults?.ProductVMResults ?? new List<ProductViewModel>(),
                ErrorMessage = ProductVMResults?.ProductVMResults?.Any() != true ? "No results found." : null
            };

            // Log khi trả về kết quả
            _logger.LogInformation("Returning results for query: {SearchQuery}", searchQuery);

            return View("Index", viewModel);
        }
       
    }
}
