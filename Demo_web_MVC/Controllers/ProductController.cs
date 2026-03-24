using Demo_web_MVC.Service;
using Demo_web_MVC.Service.Product;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Demo_web_MVC.Controllers
{
    public class ProductController : Controller
    {
        public readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        public async Task<IActionResult> Index()
        {
            return View( await _productService.getAll());
        }
        public IActionResult Details(int id)
        {
            return View();
        }
        [HttpGet]
        public async Task<ActionResult> Details(int? id)
        {
            // Call the service to get product details by id
            // var productDetails = await _productService.details(id);
            // return View(productDetails);
            if ( id  == null)
            {
                return NotFound("không có id ");
            }
            var productDetails = await _productService.details(id.Value);
            if (productDetails == null)
            {
                return NotFound("không tìm thấy sản phẩm");
            }
            return View(productDetails);
        }
    }
}
