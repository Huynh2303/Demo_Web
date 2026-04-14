using Demo_web_MVC.Controllers;
using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Demo_web_MVC.Repository.Addresss;
using Microsoft.EntityFrameworkCore;


namespace Demo_web_MVC.Repository.Oder
{
    public class OderRepository: IOderRepository
    {
        public readonly AppDatabase _context;
        public readonly ILogger<OderRepository> _logger;  
        public readonly IAddressRepository _addressRepository;
        public OderRepository(AppDatabase context, ILogger<OderRepository> logger, IAddressRepository addressRepository)
        {
            _context = context;
            _logger = logger;
            _addressRepository = addressRepository;
        }
        public async Task<int> CreateOrderFromCartAsync(int userId, string paymentMethod)
        {
                        
            var cart = await _context.Carts
                             .Where(c => c.UserId == userId && c.Status == "active")
                             .Include(c => c.CartItems)
                             .FirstOrDefaultAsync();
            if ( cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                _logger.LogWarning("No active cart found for user {UserId}", userId);
                throw new InvalidOperationException("No active cart found for the user.");
            }
            if (Enum.TryParse(paymentMethod, out PaymentMethod method))
            {
                // Tiến hành tạo đơn hàng nếu parsing thành công
                var order = new Order
                {
                    UserId = userId,
                    TotalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.Variant.Price),
                    Status = "Pending",
                    PaymentMethod = method,  // Gán giá trị enum vào PaymentMethod
                    CreatedAt = DateTime.Now
                };

                // Lưu đơn hàng vào cơ sở dữ liệu
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Lưu các mục trong đơn hàng
                foreach (var item in cart.CartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        VariantId = item.VariantId,
                        Quantity = item.Quantity,
                        Price = item.Variant.Price
                    };
                    _context.OrderItems.Add(orderItem);
                }

                await _context.SaveChangesAsync(); // Lưu các mục trong đơn hàng

                // Trả về ID của đơn hàng vừa tạo
                return order.Id;
            }   
            else
            {
                // Trường hợp parsing thất bại (chẳng hạn "COD" không đúng)
                return -1;  // Hoặc xử lý lỗi khác
            }

        }
    }
}
