using Demo_web_MVC.Repository.Carts;
using Demo_web_MVC.Service.Cart;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Demo_web_MVC.Controllers
{
    public class CartController : Controller
    {
        public readonly ICartService _cartService;
        public readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;

        }
        public async Task<IActionResult> Index(int userid)
        {
            var cartItems = await _cartService.GetCartItems(userid);
            ViewBag.CartCount = cartItems.Count;

            return View(cartItems);
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
                return StatusCode(500, ex.Message);
            }
        }
        public async Task<IActionResult> RemoveItem( int cartItemId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized("Không xác định được người dùng.");
                }
                var result = await _cartService.RemoveItemAsync(userId, cartItemId);
                if (result)
                {
                    return Ok("Sản phẩm đã được xóa khỏi giỏ hàng.");
                }
                else
                {
                    return BadRequest("Không thể xóa sản phẩm khỏi giỏ hàng.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa sản phẩm khỏi giỏ hàng.");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
