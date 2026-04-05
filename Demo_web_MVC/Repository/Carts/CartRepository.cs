
using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Product;
using Microsoft.EntityFrameworkCore;
using System;
namespace Demo_web_MVC.Repository.Carts
{
    public class CartRepository: ICartRepository
    {
        public readonly AppDatabase _context;
        public CartRepository(AppDatabase context)
        {
            _context = context;
        }
        public async Task<bool> AddToCartAsync(int userId, int variantId, int quantity)
        {
            if (quantity <= 0)
                throw new Exception("Số lượng phải lớn hơn 0.");

            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == variantId);

            if (variant == null)
                throw new Exception("Biến thể sản phẩm không tồn tại.");

            if (variant.Stock < quantity)
                throw new Exception("Số lượng vượt quá tồn kho.");

            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    Status = "Active"
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.VariantId == variantId);

            if (cartItem != null)
            {
                var totalQuantity = cartItem.Quantity + quantity;

                if (totalQuantity > variant.Stock)
                    throw new Exception("Tổng số lượng trong giỏ vượt quá tồn kho.");

                cartItem.Quantity = totalQuantity;
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    VariantId = variantId,
                    Quantity = quantity
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

    }
    }

