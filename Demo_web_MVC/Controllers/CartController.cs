using Demo_web_MVC.Repository.Carts;
using Demo_web_MVC.Service.Cart;
using Microsoft.AspNetCore.Mvc;

namespace Demo_web_MVC.Controllers
{
    public class CartController : Controller
    {
        public readonly ICartService _cartService;
        public readonly ILogger<CartController> _logger;
        public readonly ICartRepository _cartRepository;
        public CartController(ICartService cartService, ILogger<CartController> logger, ICartRepository cartRepository)
        {
            _cartService = cartService;
            _logger = logger;
            _cartRepository = cartRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> AddToCart(int userId, int variantId, int quantity)
        {
            try
            {
                var result = await _cartService.AddToCartAsync(userId, variantId, quantity);
                if (result)
                {
                    return Ok("Sản phẩm đã được thêm vào giỏ hàng.");
                }
                else
                {
                    return BadRequest("Không thể thêm sản phẩm vào giỏ hàng.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm sản phẩm vào giỏ hàng.");
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu.");
            }
        }
    }
}
