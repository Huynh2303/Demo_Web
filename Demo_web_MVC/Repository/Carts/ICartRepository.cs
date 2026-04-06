

using Demo_web_MVC.Models.ViewModel.Carts;
namespace Demo_web_MVC.Repository.Carts
    
{
    public interface ICartRepository
    {
        Task<bool> AddToCartAsync(int userId, int variantId, int quantity);
        
        Task<List<CartItemViewModel>> GetCartItemsAsync(int userId);
        //Task UpdateQuantityAsync(int userId, int cartItemId, int quantity);
        Task<bool> RemoveItemAsync(int userId, int cartItemId);
        //Task ClearCartAsync(int userId);
        //Task<int> GetCartCountAsync(int userId);
    }
}
