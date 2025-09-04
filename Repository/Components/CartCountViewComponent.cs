using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Models.ViewModels;
using System.Security.Claims;

namespace Shopping_Demo.Repository.Components
{
    public class CartCountViewComponent : ViewComponent
    {
        private readonly DataContext _dataContext;

        public CartCountViewComponent(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int cartItemCount = 0;

            if (User.Identity.IsAuthenticated)
            {
                string userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Lấy giỏ hàng từ database
                var userCart = await _dataContext.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (userCart != null)
                {
                    cartItemCount = userCart.CartItems.Sum(ci => ci.Quantity);
                }
            }
            else
            {
                // Lấy giỏ hàng từ session nếu user chưa đăng nhập
                var sessionCartItems = HttpContext.Session.GetJson<List<CartItemSessionDTO>>("Cart");

                if (sessionCartItems != null)
                {
                    cartItemCount = sessionCartItems.Sum(ci => ci.Quantity);
                }
            }

            return View(cartItemCount);
        }
    }
}
