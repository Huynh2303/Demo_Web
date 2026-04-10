using Demo_web_MVC.Models.ViewModel.Address;
using Demo_web_MVC.Service.Address;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Demo_web_MVC.Controllers
{
    public class AdressController : Controller
    {
        public readonly IAddressService _addressService;
        public readonly ILogger<AdressController> _logger;
        public AdressController(IAddressService addressService, ILogger<AdressController> logger)
        {
            _addressService = addressService;
            _logger = logger;
        }
        private int? GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return null;
            }
            return userId;
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId == null)
                {
                    return Unauthorized("Không xác định được người dùng.");
                }

                var result = (await _addressService.GetAllByUserId(userId.Value)).ToList();

                _logger.LogInformation(
                    "Lấy danh sách địa chỉ thành công cho userId: {UserId}, Số lượng địa chỉ: {Count}",
                    userId.Value,
                    result.Count
                );

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách địa chỉ.");
                return StatusCode(500, "Đã xảy ra lỗi khi tải danh sách địa chỉ.");
            }
        }
        public IActionResult Create()
        {
            return View();
        }
    }
}
