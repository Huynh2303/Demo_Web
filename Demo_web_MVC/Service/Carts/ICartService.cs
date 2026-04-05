using Demo_web_MVC.Models.ViewModel.Carts;

namespace Demo_web_MVC.Service.Cart
{
    public interface ICartService
    {
        Task<bool> AddToCartAsync(int userId, int variantId, int quantity);
    }
}
