using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shopping_Demo.Models;
using Shopping_Demo.Models.ViewModels;
using Shopping_Demo.Repository;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Shopping_Demo.Controllers
{
    public class CartController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<AppUserModel> _userManager;

        public CartController(DataContext dataContext, UserManager<AppUserModel> userManager)
        {
            _dataContext = dataContext;
            _userManager = userManager;
        }

        private async Task<CartModel> GetOrCreateUserCartAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var userCart = await _dataContext.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (userCart == null)
                {
                    userCart = new CartModel
                    {
                        UserId = userId,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        CartItems = new List<CartItemModel>()
                    };
                    _dataContext.Carts.Add(userCart);
                    await _dataContext.SaveChangesAsync();
                }

                return userCart;
            }

            return null;
        }

        private void SyncCartToSession(List<CartItemModel> cartItems)
        {
            if (cartItems != null && cartItems.Any())
            {
                var sessionCartItems = cartItems.Select(item => new CartItemSessionDTO(item)).ToList();
                HttpContext.Session.SetJson("Cart", sessionCartItems);
            }
            else
            {
                HttpContext.Session.Remove("Cart");
            }
        }

        private async Task SyncSessionToDatabaseAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                var sessionCartItems = HttpContext.Session.GetJson<List<CartItemSessionDTO>>("Cart");
                if (sessionCartItems != null && sessionCartItems.Any())
                {
                    var userCart = await GetOrCreateUserCartAsync();
                    
                    // Clear existing cart items
                    _dataContext.CartItems.RemoveRange(userCart.CartItems);
                    await _dataContext.SaveChangesAsync();

                    // Add session items to database
                    foreach (var sessionItem in sessionCartItems)
                    {
                        var cartItem = new CartItemModel
                        {
                            CartId = userCart.Id,
                            ProductId = sessionItem.ProductId,
                            Quantity = sessionItem.Quantity,
                            Price = sessionItem.Price,
                            ColorName = sessionItem.ColorName,
                            SizeName = sessionItem.SizeName,
                            Image = sessionItem.Image
                        };
                        _dataContext.CartItems.Add(cartItem);
                    }
                    await _dataContext.SaveChangesAsync();
                }
            }
        }

        public async Task<IActionResult> Index()
        {
            List<CartItemModel> cartItems;
            decimal subtotal = 0;
            List<int> outOfStockProductIds = new List<int>(); // Store IDs of out-of-stock products

            if (User.Identity.IsAuthenticated)
            {
                // Sync session to database first
                await SyncSessionToDatabaseAsync();
                
                var userCart = await GetOrCreateUserCartAsync();
                cartItems = userCart.CartItems.ToList();

                // Check inventory for each cart item and update image info
                foreach (var item in cartItems)
                {
                    var product = await _dataContext.Products.FindAsync(item.ProductId);
                    if (product == null || product.Quantity == 0)
                    {
                        outOfStockProductIds.Add(item.ProductId);
                    }
                    else
                    {
                        subtotal += item.Quantity * item.Price;
                        // Update image info from product
                        if (string.IsNullOrEmpty(item.Image) && !string.IsNullOrEmpty(product.Image))
                        {
                            item.Image = product.Image;
                            _dataContext.Update(item);
                        }
                    }
                }

                // Save any updates
                await _dataContext.SaveChangesAsync();
                SyncCartToSession(cartItems);
            }
            else
            {
                var sessionCartItems = HttpContext.Session.GetJson<List<CartItemSessionDTO>>("Cart");

                if (sessionCartItems != null)
                {
                    cartItems = sessionCartItems.Select(dto => dto.ToCartItemModel()).ToList();
                    foreach (var item in cartItems)
                    {
                        var product = await _dataContext.Products.FindAsync(item.ProductId);
                        if (product == null || product.Quantity == 0)
                        {
                            outOfStockProductIds.Add(item.ProductId);
                        }
                        else
                        {
                            subtotal += item.Quantity * item.Price;
                            // Update image info from product
                            if (string.IsNullOrEmpty(item.Image) && !string.IsNullOrEmpty(product.Image))
                            {
                                item.Image = product.Image;
                            }
                        }
                    }

                    // Update session with updated cart items
                    var updatedSessionItems = cartItems.Select(item => new CartItemSessionDTO(item)).ToList();
                    HttpContext.Session.SetJson("Cart", updatedSessionItems);
                }
                else
                {
                    cartItems = new List<CartItemModel>();
                }
            }

            var shippingPriceCookie = Request.Cookies["ShippingPrice"];
            decimal shippingPrice = 0;

            var coupon_code = Request.Cookies["CouponTitle"];
            var discountAmountCookie = Request.Cookies["DiscountAmount"];
            decimal discountAmount = 0;

            if (shippingPriceCookie != null)
            {
                var shippingPriceJson = shippingPriceCookie;
                shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);

                if (subtotal >= 1000000)
                {
                    shippingPrice = 0;
                    var shippingPriceJsonNew = JsonConvert.SerializeObject(shippingPrice);
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                        Secure = true
                    };
                    Response.Cookies.Append("ShippingPrice", shippingPriceJsonNew, cookieOptions);
                }
            }

            if (discountAmountCookie != null)
            {
                decimal.TryParse(discountAmountCookie, out discountAmount);
            }

            decimal finalTotal = subtotal - discountAmount + shippingPrice;
            if (finalTotal < 0) finalTotal = 0;

            CartItemViewModel cartVM = new()
            {
                CartItems = cartItems,
                GrandTotal = subtotal,
                ShippingPrice = shippingPrice,
                CouponCode = coupon_code,
                DiscountAmount = discountAmount,
                FinalTotal = finalTotal
            };

            ViewBag.OutOfStockProductIds = outOfStockProductIds;

            // Lấy thông tin user nếu đã đăng nhập
            if (User.Identity.IsAuthenticated)
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user != null)
                {
                    ViewBag.UserEmail = user.Email;
                    ViewBag.UserPhone = user.PhoneNumber;
                    ViewBag.UserFullName = user.FullName;
                    ViewBag.UserAddress = user.Address;
                }
            }

            return View(cartVM);
        }

        public async Task<IActionResult> Add(int Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);

            if (product == null)
            {
                return NotFound();
            }

            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    var userCart = await GetOrCreateUserCartAsync();
                    var cartItem = userCart.CartItems.FirstOrDefault(x => x.ProductId == Id);
                    var currentCartQuantity = cartItem?.Quantity ?? 0;

                    // Kiểm tra tổng số lượng trong giỏ hàng không được vượt quá số lượng sản phẩm có sẵn
                    if (currentCartQuantity >= product.Quantity)
                    {
                        TempData["error"] = $"Sản phẩm chỉ còn {product.Quantity} chiếc trong kho";
                        return Redirect(Request.Headers["Referer"].ToString());
                    }

                    if (cartItem == null)
                    {
                        cartItem = new CartItemModel(product)
                        {
                            CartId = userCart.Id
                        };
                        userCart.CartItems.Add(cartItem);
                    }
                    else
                    {
                        cartItem.Quantity += 1;
                    }

                    userCart.UpdatedDate = DateTime.Now;
                    await _dataContext.SaveChangesAsync();

                    SyncCartToSession(userCart.CartItems.ToList());
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Error adding item to cart: " + ex.Message;
                    return Redirect(Request.Headers["Referer"].ToString());
                }
            }
            else
            {
                List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
                CartItemModel cartItem = cart.Where(x => x.ProductId == Id).FirstOrDefault();
                var currentCartQuantity = cartItem?.Quantity ?? 0;

                // Kiểm tra tổng số lượng trong giỏ hàng không được vượt quá số lượng sản phẩm có sẵn
                if (currentCartQuantity >= product.Quantity)
                {
                    TempData["error"] = $"Sản phẩm chỉ còn {product.Quantity} chiếc trong kho";
                    return Redirect(Request.Headers["Referer"].ToString());
                }

                if (cartItem == null)
                {
                    cart.Add(new CartItemModel(product));
                }
                else
                {
                    cartItem.Quantity += 1;
                }

                HttpContext.Session.SetJson("Cart", cart);
            }

            TempData["success"] = "Add Item to Cart successfully";
            return Redirect(Request.Headers["Referer"].ToString());
        }

        public async Task<IActionResult> Decrease(int Id)
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    var userCart = await GetOrCreateUserCartAsync();
                    var cartItem = userCart.CartItems.FirstOrDefault(c => c.ProductId == Id);

                    if (cartItem != null)
                    {
                        if (cartItem.Quantity > 1)
                        {
                            cartItem.Quantity--;
                        }
                        else
                        {
                            userCart.CartItems.Remove(cartItem);
                            _dataContext.Remove(cartItem);
                        }

                        userCart.UpdatedDate = DateTime.Now;
                        await _dataContext.SaveChangesAsync();

                        SyncCartToSession(userCart.CartItems.ToList());
                    }
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Error decreasing item quantity: " + ex.Message;
                }
            }
            else
            {
                List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");

                if (cart != null)
                {
                    CartItemModel cartItem = cart.FirstOrDefault(c => c.ProductId == Id);
                    if (cartItem != null)
                    {
                        if (cartItem.Quantity > 1)
                        {
                            --cartItem.Quantity;
                        }
                        else
                        {
                            cart.RemoveAll(p => p.ProductId == Id);
                        }

                        if (cart.Count == 0)
                        {
                            HttpContext.Session.Remove("Cart");
                        }
                        else
                        {
                            HttpContext.Session.SetJson("Cart", cart);
                        }
                    }
                }
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Increase(int Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);

            if (product == null)
            {
                return NotFound();
            }

            if (product.Quantity == 0)
            {
                TempData["error"] = "Sản phẩm đã hết hàng!";
                return RedirectToAction("Index");
            }

            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    var userCart = await GetOrCreateUserCartAsync();
                    var cartItem = userCart.CartItems.FirstOrDefault(c => c.ProductId == Id);

                    if (cartItem != null)
                    {
                        if (cartItem.Quantity >= 1 && product.Quantity > cartItem.Quantity)
                        {
                            cartItem.Quantity++;
                        }
                        else
                        {
                            cartItem.Quantity = product.Quantity;
                        }

                        userCart.UpdatedDate = DateTime.Now;
                        await _dataContext.SaveChangesAsync();

                        SyncCartToSession(userCart.CartItems.ToList());
                    }
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Error increasing item quantity: " + ex.Message;
                }
            }
            else
            {
                List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");

                if (cart != null)
                {
                    CartItemModel cartItem = cart.FirstOrDefault(c => c.ProductId == Id);
                    if (cartItem != null)
                    {
                        if (cartItem.Quantity >= 1 && product.Quantity > cartItem.Quantity)
                        {
                            ++cartItem.Quantity;
                        }
                        else
                        {
                            cartItem.Quantity = product.Quantity;
                        }

                        HttpContext.Session.SetJson("Cart", cart);
                    }
                }
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Remove(int Id)
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    var userCart = await GetOrCreateUserCartAsync();
                    var cartItem = userCart.CartItems.FirstOrDefault(c => c.ProductId == Id);

                    if (cartItem != null)
                    {
                        userCart.CartItems.Remove(cartItem);
                        _dataContext.Remove(cartItem);
                        userCart.UpdatedDate = DateTime.Now;
                        await _dataContext.SaveChangesAsync();

                        SyncCartToSession(userCart.CartItems.ToList());
                    }
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Error removing item from cart: " + ex.Message;
                }
            }
            else
            {
                List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");

                if (cart != null)
                {
                    cart.RemoveAll(p => p.ProductId == Id);

                    if (cart.Count == 0)
                    {
                        HttpContext.Session.Remove("Cart");
                    }
                    else
                    {
                        HttpContext.Session.SetJson("Cart", cart);
                    }
                }
            }

            TempData["success"] = "Remove Item of Cart successfully";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Clear()
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    var userCart = await GetOrCreateUserCartAsync();

                    foreach (var item in userCart.CartItems.ToList())
                    {
                        _dataContext.Remove(item);
                    }

                    userCart.CartItems.Clear();
                    userCart.UpdatedDate = DateTime.Now;
                    await _dataContext.SaveChangesAsync();

                    HttpContext.Session.Remove("Cart");
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Error clearing cart: " + ex.Message;
                }
            }
            else
            {
                HttpContext.Session.Remove("Cart");
            }

            TempData["success"] = "Clear All Item of Cart successfully";
            return RedirectToAction("Index");
        }
        private CookieOptions GetCookieOptions(int expireMinutes = 60)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTimeOffset.UtcNow.AddMinutes(expireMinutes),
                Secure = Request.IsHttps, // Tự động phát hiện HTTPS
                SameSite = SameSiteMode.Lax,
                IsEssential = true, // Đánh dấu là essential cookie
                Domain = null // Để null cho phép hoạt động trên domain hiện tại
            };
        }

        // Cập nhật method GetCoupon
        [HttpPost]
        public async Task<IActionResult> GetCoupon(string coupon_value)
        {
            // Coupon functionality disabled - Coupons table not exists
            return Json(new { success = false, message = "Tính năng coupon tạm thời không khả dụng" });
            
            /* Original code commented out:
            var validCoupon = await _dataContext.Coupons
                .FirstOrDefaultAsync(x => x.Name == coupon_value && x.Quantity >= 1 && x.Status == 1);

            if (validCoupon == null)
            {
                return Json(new { success = false, message = "Mã giảm giá không tồn tại hoặc đã hết lượt sử dụng" });
            }

            if (validCoupon.DateExpired < DateTime.Now || validCoupon.DateStart > DateTime.Now)
            {
                return Json(new { success = false, message = "Mã giảm giá đã hết hạn hoặc chưa đến thời gian sử dụng" });
            }

            List<CartItemModel> cartItems;
            if (User.Identity.IsAuthenticated)
            {
                var userCart = await GetOrCreateUserCartAsync();
                cartItems = userCart.CartItems.ToList();
            }
            else
            {
                cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            }

            decimal grandTotal = cartItems.Sum(x => x.Quantity * x.Price);
            decimal shippingPrice = 0;
            var shippingPriceCookie = Request.Cookies["ShippingPrice"];
            if (shippingPriceCookie != null)
            {
                shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceCookie);
            }

            decimal discountAmount = 0;
            string discountInfo = "";
            if (validCoupon.DiscountType == DiscountType.Percentage)
            {
                discountAmount = Math.Round((grandTotal * validCoupon.DiscountValue) / 100, 0);
                discountInfo = $"{validCoupon.Name} | Giảm {validCoupon.DiscountValue}% | {discountAmount.ToString("#,##0")} VNĐ";
            }
            else
            {
                discountAmount = validCoupon.DiscountValue;
                discountInfo = $"{validCoupon.Name} | Giảm {validCoupon.DiscountValue.ToString("#,##0")} VNĐ";
            }

            decimal finalTotal = grandTotal - discountAmount + shippingPrice;
            if (finalTotal < 0) finalTotal = 0;

            try
            {
                var cookieOptions = GetCookieOptions(60); // 60 minutes

                Response.Cookies.Append("CouponCode", validCoupon.Name, cookieOptions);
                Response.Cookies.Append("CouponTitle", discountInfo, cookieOptions);
                Response.Cookies.Append("DiscountAmount", discountAmount.ToString(), cookieOptions);
                Response.Cookies.Append("CouponId", validCoupon.Id.ToString(), cookieOptions);

                return Json(new
                {
                    success = true,
                    discountInfo,
                    discountAmount = discountAmount.ToString("#,##0 đ"),
                    grandTotal,
                    shippingPrice,
                    finalTotal,
                    hasShippingAddress = Request.Cookies["ShippingCity"] != null
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi áp dụng mã giảm giá" });
            }
            */
        }

        // Cập nhật method GetShipping
        [HttpPost]
        public async Task<IActionResult> GetShipping(string tinh, string quan, string phuong, string address)
        {
            var existingShipping = await _dataContext.Shippings.FirstOrDefaultAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);
            decimal shippingPrice = existingShipping?.Price ?? 50000;

            List<CartItemModel> cartItems;
            if (User.Identity.IsAuthenticated)
            {
                var userCart = await GetOrCreateUserCartAsync();
                cartItems = userCart.CartItems.ToList();
            }
            else
            {
                var sessionCartItems = HttpContext.Session.GetJson<List<CartItemSessionDTO>>("Cart");
                cartItems = sessionCartItems?.Select(dto => dto.ToCartItemModel()).ToList() ?? new List<CartItemModel>();
            }

            decimal grandTotal = cartItems.Sum(x => x.Quantity * x.Price);
            if (grandTotal >= 1000000)
            {
                shippingPrice = 0;
            }

            decimal discountAmount = 0;
            var discountCookie = Request.Cookies["DiscountAmount"];
            if (discountCookie != null && decimal.TryParse(discountCookie, out decimal parsedDiscount))
            {
                discountAmount = parsedDiscount;
            }

            decimal finalTotal = grandTotal - discountAmount + shippingPrice;
            if (finalTotal < 0) finalTotal = 0;

            try
            {
                var cookieOptions = GetCookieOptions(30); // 30 minutes

                // Debug: Log values being set to cookies
                Console.WriteLine($"DEBUG - Setting cookies - ShippingCity: {tinh}");
                Console.WriteLine($"DEBUG - Setting cookies - ShippingDistrict: {quan}");
                Console.WriteLine($"DEBUG - Setting cookies - ShippingWard: {phuong}");
                Console.WriteLine($"DEBUG - Setting cookies - ShippingAddress: {address}");
                Console.WriteLine($"DEBUG - Setting cookies - ShippingPrice: {shippingPrice}");

                Response.Cookies.Append("ShippingPrice", JsonConvert.SerializeObject(shippingPrice), cookieOptions);
                Response.Cookies.Append("ShippingCity", tinh, cookieOptions);
                Response.Cookies.Append("ShippingDistrict", quan, cookieOptions);
                Response.Cookies.Append("ShippingWard", phuong, cookieOptions);
                Response.Cookies.Append("ShippingAddress", address, cookieOptions);

                // Debug: Verify cookies were set
                Console.WriteLine($"DEBUG - Cookie verification - ShippingCity: {Request.Cookies["ShippingCity"]}");
                Console.WriteLine($"DEBUG - Cookie verification - ShippingDistrict: {Request.Cookies["ShippingDistrict"]}");
                Console.WriteLine($"DEBUG - Cookie verification - ShippingWard: {Request.Cookies["ShippingWard"]}");
                Console.WriteLine($"DEBUG - Cookie verification - ShippingAddress: {Request.Cookies["ShippingAddress"]}");

                return Json(new
                {
                    success = true,
                    shippingPrice,
                    grandTotal,
                    finalTotal,
                    hasShippingAddress = true
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật thông tin vận chuyển" });
            }
        }
        [HttpPost]
        public IActionResult RemoveShippingCookie()
        {
            decimal grandTotal = 0;
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userCart = GetOrCreateUserCartAsync().Result;
                    grandTotal = userCart.CartItems.Sum(x => x.Quantity * x.Price);
                }
                else
                {
                    var cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
                    if (cartItems != null)
                    {
                        grandTotal = cartItems.Sum(x => x.Quantity * x.Price);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating cart total: {ex.Message}");
            }

            decimal discountAmount = 0;
            var discountCookie = Request.Cookies["DiscountAmount"];
            if (discountCookie != null && decimal.TryParse(discountCookie, out decimal parsedDiscount))
            {
                discountAmount = parsedDiscount;
            }

            decimal finalTotal = grandTotal - discountAmount;
            if (finalTotal < 0) finalTotal = 0;

            Response.Cookies.Delete("ShippingPrice");
            Response.Cookies.Delete("ShippingCity");
            Response.Cookies.Delete("ShippingDistrict");
            Response.Cookies.Delete("ShippingWard");
            Response.Cookies.Delete("ShippingAddress");

            return Json(new
            {
                success = true,
                grandTotal,
                shippingPrice = 0,
                finalTotal,
                hasShippingAddress = false
            });
        }

        [HttpPost]
        public IActionResult RemoveCouponCookie()
        {
            decimal grandTotal = 0;
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userCart = GetOrCreateUserCartAsync().Result;
                    grandTotal = userCart.CartItems.Sum(x => x.Quantity * x.Price);
                }
                else
                {
                    var cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
                    if (cartItems != null)
                    {
                        grandTotal = cartItems.Sum(x => x.Quantity * x.Price);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating cart total: {ex.Message}");
            }

            decimal shippingPrice = 0;
            var shippingCookie = Request.Cookies["ShippingPrice"];
            if (shippingCookie != null)
            {
                shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingCookie);
            }

            decimal finalTotal = grandTotal + shippingPrice;

            Response.Cookies.Delete("CouponTitle");
            Response.Cookies.Delete("CouponCode");
            Response.Cookies.Delete("DiscountAmount");
            Response.Cookies.Delete("CouponId");

            return Json(new
            {
                success = true,
                grandTotal,
                shippingPrice,
                finalTotal,
                hasShippingAddress = Request.Cookies["ShippingCity"] != null
            });
        }
        [HttpPost]
        public async Task<IActionResult> AddWithOptions(int productId, int quantity, string colorId, string sizeId)
        {
            ProductModel product = await _dataContext.Products.FindAsync(productId);

            if (product == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm!" });
            }

            if (product.Quantity < quantity)
            {
                return Json(new { success = false, message = "Số lượng sản phẩm trong kho không đủ!" });
            }

            string cartItemKey = $"{productId}_{colorId}_{sizeId}";

            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    var userCart = await GetOrCreateUserCartAsync();

                    var cartItem = userCart.CartItems.FirstOrDefault(x =>
                        x.ProductId == productId &&
                        x.ColorId == colorId &&
                        x.SizeId == sizeId);

                    if (cartItem == null)
                    {
                        cartItem = new CartItemModel(product)
                        {
                            CartId = userCart.Id,
                            Quantity = quantity,
                            ColorId = colorId,
                            SizeId = sizeId
                        };

                        if (!string.IsNullOrEmpty(colorId))
                        {
                            // var color = await _dataContext.Colors.FindAsync(int.Parse(colorId));
                            // if (color != null)
                            // {
                            //     cartItem.ColorName = color.Name;
                            // }
                            cartItem.ColorName = "Color " + colorId; // Temporary fallback
                        }

                        if (!string.IsNullOrEmpty(sizeId))
                        {
                            // var size = await _dataContext.Sizes.FindAsync(int.Parse(sizeId));
                            // if (size != null)
                            // {
                            //     cartItem.SizeName = size.Name;
                            // }
                            cartItem.SizeName = "Size " + sizeId; // Temporary fallback
                        }

                        userCart.CartItems.Add(cartItem);
                    }
                    else
                    {
                        cartItem.Quantity += quantity;
                    }

                    userCart.UpdatedDate = DateTime.Now;
                    await _dataContext.SaveChangesAsync();

                    SyncCartToSession(userCart.CartItems.ToList());

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Lỗi khi thêm sản phẩm: " + ex.Message });
                }
            }
            else
            {
                List<CartItemSessionDTO> cart = HttpContext.Session.GetJson<List<CartItemSessionDTO>>("Cart") ?? new List<CartItemSessionDTO>();

                var cartItem = cart.FirstOrDefault(x =>
                    x.ProductId == productId &&
                    x.ColorId == colorId &&
                    x.SizeId == sizeId);

                if (cartItem == null)
                {
                    CartItemSessionDTO newItem = new CartItemSessionDTO
                    {
                        ProductId = productId,
                        ProductName = product.Name,
                        Price = product.Price,
                        Quantity = quantity,
                        Image = product.Image,
                        ColorId = colorId,
                        SizeId = sizeId
                    };

                    if (!string.IsNullOrEmpty(colorId))
                    {
                        // var color = await _dataContext.Colors.FindAsync(int.Parse(colorId));
                        // if (color != null)
                        // {
                        //     newItem.ColorName = color.Name;
                        // }
                        newItem.ColorName = "Color " + colorId; // Temporary fallback
                    }

                    if (!string.IsNullOrEmpty(sizeId))
                    {
                        // var size = await _dataContext.Sizes.FindAsync(int.Parse(sizeId));
                        // if (size != null)
                        // {
                        //     newItem.SizeName = size.Name;
                        // }
                        newItem.SizeName = "Size " + sizeId; // Temporary fallback
                    }

                    cart.Add(newItem);
                }
                else
                {
                    cartItem.Quantity += quantity;
                }

                HttpContext.Session.SetJson("Cart", cart);
                return Json(new { success = true });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            int count = 0;
            
            if (User.Identity.IsAuthenticated)
            {
                var userCart = await GetOrCreateUserCartAsync();
                count = userCart.CartItems.Sum(x => x.Quantity);
            }
            else
            {
                var sessionCartItems = HttpContext.Session.GetJson<List<CartItemSessionDTO>>("Cart");
                if (sessionCartItems != null)
                {
                    count = sessionCartItems.Sum(x => x.Quantity);
                }
            }
            
            return Json(new { count = count });
        }

        [HttpGet]
        public async Task<IActionResult> DebugCart()
        {
            var isAuthenticated = User.Identity.IsAuthenticated;
            var userId = isAuthenticated ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;
            var sessionCartCount = HttpContext.Session.GetJson<List<CartItemSessionDTO>>("Cart")?.Count ?? 0;
            var databaseCartCount = 0;
            var databaseCartItems = new List<object>();

            if (isAuthenticated)
            {
                var userCart = await _dataContext.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (userCart != null)
                {
                    databaseCartCount = userCart.CartItems.Count;
                    databaseCartItems = userCart.CartItems.Select(item => new
                    {
                        item.ProductId,
                        item.ProductName,
                        item.Quantity,
                        item.Price,
                        item.ColorName,
                        item.SizeName,
                        item.Image
                    }).Cast<object>().ToList();
                }
            }

            var debugInfo = new
            {
                IsAuthenticated = isAuthenticated,
                UserId = userId,
                SessionCartCount = sessionCartCount,
                DatabaseCartCount = databaseCartCount,
                DatabaseCartItems = databaseCartItems
            };

            return Json(debugInfo);
        }

        [HttpGet]
        public IActionResult DebugShippingCookies()
        {
            var debugInfo = new
            {
                ShippingCity = Request.Cookies["ShippingCity"],
                ShippingDistrict = Request.Cookies["ShippingDistrict"],
                ShippingWard = Request.Cookies["ShippingWard"],
                ShippingAddress = Request.Cookies["ShippingAddress"],
                ShippingPrice = Request.Cookies["ShippingPrice"],
                AllCookies = Request.Cookies.Keys.ToList()
            };

            return Json(debugInfo);
        }

        [HttpPost]
        public async Task<IActionResult> ForceSyncCart()
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    await SyncSessionToDatabaseAsync();
                    
                    var userCart = await GetOrCreateUserCartAsync();
                    var cartItems = userCart.CartItems.ToList();
                    
                    SyncCartToSession(cartItems);
                    
                    return Json(new { 
                        success = true, 
                        message = "Đã sync giỏ hàng thành công",
                        cartCount = cartItems.Count
                    });
                }
                
                return Json(new { 
                    success = false, 
                    message = "User chưa đăng nhập"
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = $"Lỗi: {ex.Message}"
                });
            }
        }

        [HttpGet]
        public IActionResult GetProvinces()
        {
            var provinces = new List<object>
            {
                new { code = "01", name = "Hà Nội" },
                new { code = "79", name = "TP. Hồ Chí Minh" },
                new { code = "31", name = "Hải Phòng" },
                new { code = "48", name = "Đà Nẵng" },
                new { code = "92", name = "Cần Thơ" },
                new { code = "02", name = "Hà Giang" },
                new { code = "04", name = "Cao Bằng" },
                new { code = "06", name = "Bắc Kạn" },
                new { code = "08", name = "Tuyên Quang" },
                new { code = "10", name = "Lào Cai" },
                new { code = "11", name = "Điện Biên" },
                new { code = "12", name = "Lai Châu" },
                new { code = "14", name = "Sơn La" },
                new { code = "15", name = "Yên Bái" },
                new { code = "17", name = "Hòa Bình" },
                new { code = "19", name = "Thái Nguyên" },
                new { code = "20", name = "Lạng Sơn" },
                new { code = "22", name = "Quảng Ninh" },
                new { code = "24", name = "Bắc Giang" },
                new { code = "25", name = "Phú Thọ" },
                new { code = "26", name = "Vĩnh Phúc" },
                new { code = "27", name = "Bắc Ninh" },
                new { code = "30", name = "Hải Dương" },
                new { code = "33", name = "Hưng Yên" },
                new { code = "34", name = "Hà Nam" },
                new { code = "35", name = "Nam Định" },
                new { code = "36", name = "Thái Bình" },
                new { code = "37", name = "Ninh Bình" },
                new { code = "38", name = "Thanh Hóa" },
                new { code = "40", name = "Nghệ An" },
                new { code = "42", name = "Hà Tĩnh" },
                new { code = "44", name = "Quảng Bình" },
                new { code = "45", name = "Quảng Trị" },
                new { code = "46", name = "Thừa Thiên Huế" },
                new { code = "49", name = "Quảng Nam" },
                new { code = "51", name = "Quảng Ngãi" },
                new { code = "52", name = "Bình Định" },
                new { code = "54", name = "Phú Yên" },
                new { code = "56", name = "Khánh Hòa" },
                new { code = "58", name = "Ninh Thuận" },
                new { code = "60", name = "Bình Thuận" },
                new { code = "62", name = "Kon Tum" },
                new { code = "64", name = "Gia Lai" },
                new { code = "66", name = "Đắk Lắk" },
                new { code = "67", name = "Đắk Nông" },
                new { code = "68", name = "Lâm Đồng" },
                new { code = "70", name = "Bình Phước" },
                new { code = "72", name = "Tây Ninh" },
                new { code = "74", name = "Bình Dương" },
                new { code = "75", name = "Đồng Nai" },
                new { code = "77", name = "Bà Rịa - Vũng Tàu" },
                new { code = "80", name = "Long An" },
                new { code = "82", name = "Tiền Giang" },
                new { code = "83", name = "Bến Tre" },
                new { code = "84", name = "Trà Vinh" },
                new { code = "86", name = "Vĩnh Long" },
                new { code = "87", name = "Đồng Tháp" },
                new { code = "89", name = "An Giang" },
                new { code = "91", name = "Kiên Giang" },
                new { code = "93", name = "Hậu Giang" },
                new { code = "94", name = "Sóc Trăng" },
                new { code = "95", name = "Bạc Liêu" },
                new { code = "96", name = "Cà Mau" }
            };

            return Json(provinces);
        }

        [HttpGet]
        public IActionResult GetDistricts(string provinceCode)
        {
            var districts = new Dictionary<string, List<object>>
            {
                ["01"] = new List<object> // Hà Nội
                {
                    new { code = "001", name = "Ba Đình" },
                    new { code = "002", name = "Hoàn Kiếm" },
                    new { code = "003", name = "Tây Hồ" },
                    new { code = "004", name = "Long Biên" },
                    new { code = "005", name = "Cầu Giấy" },
                    new { code = "006", name = "Đống Đa" },
                    new { code = "007", name = "Hai Bà Trưng" },
                    new { code = "008", name = "Hoàng Mai" },
                    new { code = "009", name = "Thanh Xuân" },
                    new { code = "016", name = "Sóc Sơn" },
                    new { code = "017", name = "Đông Anh" },
                    new { code = "018", name = "Gia Lâm" },
                    new { code = "019", name = "Nam Từ Liêm" },
                    new { code = "020", name = "Thanh Trì" },
                    new { code = "021", name = "Bắc Từ Liêm" },
                    new { code = "250", name = "Mê Linh" },
                    new { code = "268", name = "Hà Đông" },
                    new { code = "269", name = "Sơn Tây" },
                    new { code = "271", name = "Ba Vì" },
                    new { code = "272", name = "Phúc Thọ" },
                    new { code = "273", name = "Đan Phượng" },
                    new { code = "274", name = "Hoài Đức" },
                    new { code = "275", name = "Quốc Oai" },
                    new { code = "276", name = "Thạch Thất" },
                    new { code = "277", name = "Chương Mỹ" },
                    new { code = "278", name = "Thanh Oai" },
                    new { code = "279", name = "Thường Tín" },
                    new { code = "280", name = "Phú Xuyên" },
                    new { code = "281", name = "Ứng Hòa" },
                    new { code = "282", name = "Mỹ Đức" }
                },
                ["79"] = new List<object> // TP. Hồ Chí Minh
                {
                    new { code = "760", name = "Quận 1" },
                    new { code = "761", name = "Quận 12" },
                    new { code = "762", name = "Quận Thủ Đức" },
                    new { code = "763", name = "Quận 9" },
                    new { code = "764", name = "Quận Gò Vấp" },
                    new { code = "765", name = "Quận Bình Thạnh" },
                    new { code = "766", name = "Quận Tân Bình" },
                    new { code = "767", name = "Quận Tân Phú" },
                    new { code = "768", name = "Quận Phú Nhuận" },
                    new { code = "769", name = "Quận 2" },
                    new { code = "770", name = "Quận 3" },
                    new { code = "771", name = "Quận 10" },
                    new { code = "772", name = "Quận 11" },
                    new { code = "773", name = "Quận 4" },
                    new { code = "774", name = "Quận 5" },
                    new { code = "775", name = "Quận 6" },
                    new { code = "776", name = "Quận 8" },
                    new { code = "777", name = "Quận Bình Tân" },
                    new { code = "778", name = "Quận 7" },
                    new { code = "783", name = "Huyện Củ Chi" },
                    new { code = "784", name = "Huyện Hóc Môn" },
                    new { code = "785", name = "Huyện Bình Chánh" },
                    new { code = "786", name = "Huyện Nhà Bè" },
                    new { code = "787", name = "Huyện Cần Giờ" }
                },
                ["48"] = new List<object> // Đà Nẵng
                {
                    new { code = "490", name = "Quận Liên Chiểu" },
                    new { code = "491", name = "Quận Thanh Khê" },
                    new { code = "492", name = "Quận Hải Châu" },
                    new { code = "493", name = "Quận Sơn Trà" },
                    new { code = "494", name = "Quận Ngũ Hành Sơn" },
                    new { code = "495", name = "Quận Cẩm Lệ" },
                    new { code = "497", name = "Huyện Hòa Vang" },
                    new { code = "498", name = "Huyện Hoàng Sa" }
                },
                ["31"] = new List<object> // Hải Phòng
                {
                    new { code = "303", name = "Quận Hồng Bàng" },
                    new { code = "304", name = "Quận Ngô Quyền" },
                    new { code = "305", name = "Quận Lê Chân" },
                    new { code = "306", name = "Quận Hải An" },
                    new { code = "307", name = "Quận Kiến An" },
                    new { code = "308", name = "Quận Đồ Sơn" },
                    new { code = "309", name = "Quận Dương Kinh" },
                    new { code = "311", name = "Huyện Thuỷ Nguyên" },
                    new { code = "312", name = "Huyện An Dương" },
                    new { code = "313", name = "Huyện An Lão" },
                    new { code = "314", name = "Huyện Kiến Thuỵ" },
                    new { code = "315", name = "Huyện Tiên Lãng" },
                    new { code = "316", name = "Huyện Vĩnh Bảo" },
                    new { code = "317", name = "Huyện Cát Hải" },
                    new { code = "318", name = "Huyện Bạch Long Vĩ" }
                },
                ["92"] = new List<object> // Cần Thơ
                {
                    new { code = "916", name = "Quận Ninh Kiều" },
                    new { code = "917", name = "Quận Ô Môn" },
                    new { code = "918", name = "Quận Bình Thuỷ" },
                    new { code = "919", name = "Quận Cái Răng" },
                    new { code = "923", name = "Quận Thốt Nốt" },
                    new { code = "924", name = "Huyện Vĩnh Thạnh" },
                    new { code = "925", name = "Huyện Cờ Đỏ" },
                    new { code = "926", name = "Huyện Phong Điền" },
                    new { code = "927", name = "Huyện Thới Lai" }
                },
                ["74"] = new List<object> // Bình Dương
                {
                    new { code = "718", name = "Thành phố Thủ Dầu Một" },
                    new { code = "719", name = "Huyện Bến Cát" },
                    new { code = "720", name = "Huyện Tân Uyên" },
                    new { code = "721", name = "Huyện Thuận An" },
                    new { code = "722", name = "Huyện Dĩ An" },
                    new { code = "723", name = "Huyện Phú Giáo" },
                    new { code = "724", name = "Huyện Dầu Tiếng" },
                    new { code = "725", name = "Thị xã Bàu Bàng" },
                    new { code = "726", name = "Thị xã Bến Cát" },
                    new { code = "727", name = "Thành phố Dĩ An" },
                    new { code = "728", name = "Thành phố Thuận An" },
                    new { code = "729", name = "Thành phố Tân Uyên" }
                },
                ["75"] = new List<object> // Đồng Nai
                {
                    new { code = "731", name = "Thành phố Biên Hòa" },
                    new { code = "732", name = "Huyện Tân Phú" },
                    new { code = "733", name = "Huyện Vĩnh Cửu" },
                    new { code = "734", name = "Huyện Định Quán" },
                    new { code = "735", name = "Huyện Trảng Bom" },
                    new { code = "736", name = "Huyện Thống Nhất" },
                    new { code = "737", name = "Huyện Cẩm Mỹ" },
                    new { code = "738", name = "Huyện Long Thành" },
                    new { code = "739", name = "Huyện Xuân Lộc" },
                    new { code = "740", name = "Huyện Nhơn Trạch" },
                    new { code = "741", name = "Thành phố Long Khánh" }
                },
                ["77"] = new List<object> // Bà Rịa - Vũng Tàu
                {
                    new { code = "747", name = "Thành phố Vũng Tàu" },
                    new { code = "748", name = "Thành phố Bà Rịa" },
                    new { code = "750", name = "Huyện Châu Đức" },
                    new { code = "751", name = "Huyện Xuyên Mộc" },
                    new { code = "752", name = "Huyện Long Điền" },
                    new { code = "753", name = "Huyện Đất Đỏ" },
                    new { code = "754", name = "Thị xã Phú Mỹ" },
                    new { code = "755", name = "Huyện Côn Đảo" }
                },
                ["80"] = new List<object> // Long An
                {
                    new { code = "794", name = "Thành phố Tân An" },
                    new { code = "795", name = "Thị xã Kiến Tường" },
                    new { code = "796", name = "Huyện Tân Hưng" },
                    new { code = "797", name = "Huyện Vĩnh Hưng" },
                    new { code = "798", name = "Huyện Mộc Hóa" },
                    new { code = "799", name = "Huyện Tân Thạnh" },
                    new { code = "800", name = "Huyện Thạnh Hóa" },
                    new { code = "801", name = "Huyện Đức Huệ" },
                    new { code = "802", name = "Huyện Đức Hòa" },
                    new { code = "803", name = "Huyện Bến Lức" },
                    new { code = "804", name = "Huyện Thủ Thừa" },
                    new { code = "805", name = "Huyện Tân Trụ" },
                    new { code = "806", name = "Huyện Cần Đước" },
                    new { code = "807", name = "Huyện Cần Giuộc" },
                    new { code = "808", name = "Huyện Châu Thành" }
                },
                ["82"] = new List<object> // Tiền Giang
                {
                    new { code = "815", name = "Thành phố Mỹ Tho" },
                    new { code = "816", name = "Thị xã Gò Công" },
                    new { code = "817", name = "Thị xã Cai Lậy" },
                    new { code = "818", name = "Huyện Tân Phước" },
                    new { code = "819", name = "Huyện Cái Bè" },
                    new { code = "820", name = "Huyện Cai Lậy" },
                    new { code = "821", name = "Huyện Châu Thành" },
                    new { code = "822", name = "Huyện Chợ Gạo" },
                    new { code = "823", name = "Huyện Gò Công Tây" },
                    new { code = "824", name = "Huyện Gò Công Đông" },
                    new { code = "825", name = "Huyện Tân Phú Đông" }
                },
                ["83"] = new List<object> // Bến Tre
                {
                    new { code = "829", name = "Thành phố Bến Tre" },
                    new { code = "831", name = "Huyện Châu Thành" },
                    new { code = "832", name = "Huyện Chợ Lách" },
                    new { code = "833", name = "Huyện Mỏ Cày Nam" },
                    new { code = "834", name = "Huyện Giồng Trôm" },
                    new { code = "835", name = "Huyện Bình Đại" },
                    new { code = "836", name = "Huyện Ba Tri" },
                    new { code = "837", name = "Huyện Thạnh Phú" },
                    new { code = "838", name = "Huyện Mỏ Cày Bắc" }
                },
                ["84"] = new List<object> // Trà Vinh
                {
                    new { code = "842", name = "Thành phố Trà Vinh" },
                    new { code = "844", name = "Huyện Càng Long" },
                    new { code = "845", name = "Huyện Cầu Kè" },
                    new { code = "846", name = "Huyện Tiểu Cần" },
                    new { code = "847", name = "Huyện Châu Thành" },
                    new { code = "848", name = "Huyện Cầu Ngang" },
                    new { code = "849", name = "Huyện Trà Cú" },
                    new { code = "850", name = "Huyện Duyên Hải" },
                    new { code = "851", name = "Thị xã Duyên Hải" }
                },
                ["86"] = new List<object> // Vĩnh Long
                {
                    new { code = "855", name = "Thành phố Vĩnh Long" },
                    new { code = "857", name = "Huyện Long Hồ" },
                    new { code = "858", name = "Huyện Mang Thít" },
                    new { code = "859", name = "Huyện Vũng Liêm" },
                    new { code = "860", name = "Huyện Tam Bình" },
                    new { code = "861", name = "Thị xã Bình Minh" },
                    new { code = "862", name = "Huyện Trà Ôn" },
                    new { code = "863", name = "Huyện Bình Tân" }
                },
                ["87"] = new List<object> // Đồng Tháp
                {
                    new { code = "866", name = "Thành phố Cao Lãnh" },
                    new { code = "867", name = "Thành phố Sa Đéc" },
                    new { code = "868", name = "Thành phố Hồng Ngự" },
                    new { code = "869", name = "Huyện Tân Hồng" },
                    new { code = "870", name = "Huyện Hồng Ngự" },
                    new { code = "871", name = "Huyện Tam Nông" },
                    new { code = "872", name = "Huyện Tháp Mười" },
                    new { code = "873", name = "Huyện Cao Lãnh" },
                    new { code = "874", name = "Huyện Thanh Bình" },
                    new { code = "875", name = "Huyện Lấp Vò" },
                    new { code = "876", name = "Huyện Lai Vung" },
                    new { code = "877", name = "Huyện Châu Thành" }
                },
                ["89"] = new List<object> // An Giang
                {
                    new { code = "883", name = "Thành phố Long Xuyên" },
                    new { code = "884", name = "Thành phố Châu Đốc" },
                    new { code = "886", name = "Huyện An Phú" },
                    new { code = "887", name = "Thị xã Tân Châu" },
                    new { code = "888", name = "Huyện Phú Tân" },
                    new { code = "889", name = "Huyện Châu Phú" },
                    new { code = "890", name = "Huyện Tịnh Biên" },
                    new { code = "891", name = "Huyện Tri Tôn" },
                    new { code = "892", name = "Huyện Châu Thành" },
                    new { code = "893", name = "Huyện Chợ Mới" },
                    new { code = "894", name = "Huyện Thoại Sơn" }
                },
                ["91"] = new List<object> // Kiên Giang
                {
                    new { code = "899", name = "Thành phố Rạch Giá" },
                    new { code = "900", name = "Thành phố Hà Tiên" },
                    new { code = "902", name = "Huyện Kiên Lương" },
                    new { code = "903", name = "Huyện Hòn Đất" },
                    new { code = "904", name = "Huyện Tân Hiệp" },
                    new { code = "905", name = "Huyện Châu Thành" },
                    new { code = "906", name = "Huyện Giồng Riềng" },
                    new { code = "907", name = "Huyện Gò Quao" },
                    new { code = "908", name = "Huyện An Biên" },
                    new { code = "909", name = "Huyện An Minh" },
                    new { code = "910", name = "Huyện Vĩnh Thuận" },
                    new { code = "911", name = "Thành phố Phú Quốc" },
                    new { code = "912", name = "Huyện Kiên Hải" },
                    new { code = "913", name = "Huyện U Minh Thượng" },
                    new { code = "914", name = "Huyện Giang Thành" }
                },
                ["93"] = new List<object> // Hậu Giang
                {
                    new { code = "930", name = "Thành phố Vị Thanh" },
                    new { code = "931", name = "Thành phố Ngã Bảy" },
                    new { code = "932", name = "Huyện Châu Thành A" },
                    new { code = "933", name = "Huyện Châu Thành" },
                    new { code = "934", name = "Huyện Phụng Hiệp" },
                    new { code = "935", name = "Huyện Vị Thủy" },
                    new { code = "936", name = "Huyện Long Mỹ" },
                    new { code = "937", name = "Thị xã Long Mỹ" }
                },
                ["94"] = new List<object> // Sóc Trăng
                {
                    new { code = "941", name = "Thành phố Sóc Trăng" },
                    new { code = "943", name = "Huyện Châu Thành" },
                    new { code = "944", name = "Huyện Kế Sách" },
                    new { code = "945", name = "Huyện Mỹ Tú" },
                    new { code = "946", name = "Huyện Cù Lao Dung" },
                    new { code = "947", name = "Huyện Long Phú" },
                    new { code = "948", name = "Huyện Mỹ Xuyên" },
                    new { code = "949", name = "Thị xã Ngã Năm" },
                    new { code = "950", name = "Huyện Thạnh Trị" },
                    new { code = "951", name = "Thị xã Vĩnh Châu" },
                    new { code = "952", name = "Huyện Trần Đề" }
                },
                ["95"] = new List<object> // Bạc Liêu
                {
                    new { code = "954", name = "Thành phố Bạc Liêu" },
                    new { code = "956", name = "Huyện Hồng Dân" },
                    new { code = "957", name = "Huyện Phước Long" },
                    new { code = "958", name = "Huyện Vĩnh Lợi" },
                    new { code = "959", name = "Thị xã Giá Rai" },
                    new { code = "960", name = "Huyện Đông Hải" },
                    new { code = "961", name = "Huyện Hoà Bình" }
                },
                ["96"] = new List<object> // Cà Mau
                {
                    new { code = "964", name = "Thành phố Cà Mau" },
                    new { code = "966", name = "Huyện U Minh" },
                    new { code = "967", name = "Huyện Thới Bình" },
                    new { code = "968", name = "Huyện Trần Văn Thời" },
                    new { code = "969", name = "Huyện Cái Nước" },
                    new { code = "970", name = "Huyện Đầm Dơi" },
                    new { code = "971", name = "Huyện Năm Căn" },
                    new { code = "972", name = "Huyện Phú Tân" },
                    new { code = "973", name = "Huyện Ngọc Hiển" }
                }
            };

            if (districts.ContainsKey(provinceCode))
            {
                return Json(districts[provinceCode]);
            }

            return Json(new List<object>());
        }

        [HttpGet]
        public IActionResult GetWards(string provinceCode, string districtCode)
        {
            var wards = new Dictionary<string, Dictionary<string, List<object>>>
            {
                ["01"] = new Dictionary<string, List<object>> // Hà Nội
                {
                    ["001"] = new List<object> // Ba Đình
                    {
                        new { code = "00001", name = "Phường Phúc Xá" },
                        new { code = "00004", name = "Phường Trúc Bạch" },
                        new { code = "00006", name = "Phường Vĩnh Phúc" },
                        new { code = "00007", name = "Phường Cống Vị" },
                        new { code = "00008", name = "Phường Liễu Giai" },
                        new { code = "00010", name = "Phường Nguyễn Trung Trực" },
                        new { code = "00013", name = "Phường Quán Thánh" },
                        new { code = "00016", name = "Phường Ngọc Hà" },
                        new { code = "00019", name = "Phường Điện Biên" },
                        new { code = "00022", name = "Phường Đội Cấn" },
                        new { code = "00025", name = "Phường Ngọc Khánh" },
                        new { code = "00028", name = "Phường Kim Mã" },
                        new { code = "00031", name = "Phường Giảng Võ" },
                        new { code = "00034", name = "Phường Thành Công" }
                    },
                    ["002"] = new List<object> // Hoàn Kiếm
                    {
                        new { code = "00037", name = "Phường Phúc Tân" },
                        new { code = "00040", name = "Phường Đồng Xuân" },
                        new { code = "00043", name = "Phường Hàng Mã" },
                        new { code = "00046", name = "Phường Hàng Buồm" },
                        new { code = "00049", name = "Phường Hàng Đào" },
                        new { code = "00052", name = "Phường Hàng Bồ" },
                        new { code = "00055", name = "Phường Cửa Đông" },
                        new { code = "00058", name = "Phường Lý Thái Tổ" },
                        new { code = "00061", name = "Phường Hàng Bạc" },
                        new { code = "00064", name = "Phường Hàng Gai" },
                        new { code = "00067", name = "Phường Chương Dương" },
                        new { code = "00070", name = "Phường Hàng Trống" },
                        new { code = "00073", name = "Phường Cửa Nam" },
                        new { code = "00076", name = "Phường Hàng Bông" },
                        new { code = "00079", name = "Phường Tràng Tiền" },
                        new { code = "00082", name = "Phường Trần Hưng Đạo" },
                        new { code = "00085", name = "Phường Phan Chu Trinh" },
                        new { code = "00088", name = "Phường Hàng Bài" }
                    },
                    ["005"] = new List<object> // Cầu Giấy
                    {
                        new { code = "00133", name = "Phường Dịch Vọng" },
                        new { code = "00136", name = "Phường Dịch Vọng Hậu" },
                        new { code = "00139", name = "Phường Quan Hoa" },
                        new { code = "00142", name = "Phường Yên Hòa" },
                        new { code = "00145", name = "Phường Trung Hòa" },
                        new { code = "00148", name = "Phường Cát Linh" },
                        new { code = "00151", name = "Phường Văn Miếu" },
                        new { code = "00154", name = "Phường Quốc Tử Giám" },
                        new { code = "00157", name = "Phường Láng Thượng" },
                        new { code = "00160", name = "Phường Ô Chợ Dừa" },
                        new { code = "00163", name = "Phường Văn Chương" },
                        new { code = "00166", name = "Phường Hàng Bột" },
                        new { code = "00169", name = "Phường Láng Hạ" },
                        new { code = "00172", name = "Phường Khâm Thiên" },
                        new { code = "00175", name = "Phường Thổ Quan" },
                        new { code = "00178", name = "Phường Nam Đồng" },
                        new { code = "00181", name = "Phường Trung Phụng" },
                        new { code = "00184", name = "Phường Quang Trung" },
                        new { code = "00187", name = "Phường Trung Liệt" },
                        new { code = "00190", name = "Phường Phương Liên" },
                        new { code = "00193", name = "Phường Thịnh Quang" },
                        new { code = "00196", name = "Phường Trung Tự" },
                        new { code = "00199", name = "Phường Kim Liên" },
                        new { code = "00202", name = "Phường Phương Mai" },
                        new { code = "00205", name = "Phường Ngã Tư Sở" },
                        new { code = "00208", name = "Phường Khương Thượng" }
                    }
                },
                ["79"] = new Dictionary<string, List<object>> // TP. Hồ Chí Minh
                {
                    ["760"] = new List<object> // Quận 1
                    {
                        new { code = "26734", name = "Phường Tân Định" },
                        new { code = "26737", name = "Phường Đa Kao" },
                        new { code = "26740", name = "Phường Bến Nghé" },
                        new { code = "26743", name = "Phường Bến Thành" },
                        new { code = "26746", name = "Phường Nguyễn Thái Bình" },
                        new { code = "26749", name = "Phường Phạm Ngũ Lão" },
                        new { code = "26752", name = "Phường Cầu Ông Lãnh" },
                        new { code = "26755", name = "Phường Cô Giang" },
                        new { code = "26758", name = "Phường Nguyễn Cư Trinh" },
                        new { code = "26761", name = "Phường Cầu Kho" }
                    },
                    ["761"] = new List<object> // Quận 12
                    {
                        new { code = "26764", name = "Phường Thạnh Xuân" },
                        new { code = "26767", name = "Phường Thạnh Lộc" },
                        new { code = "26770", name = "Phường Hiệp Thành" },
                        new { code = "26773", name = "Phường Thới An" },
                        new { code = "26776", name = "Phường Tân Chánh Hiệp" },
                        new { code = "26779", name = "Phường An Phú Đông" },
                        new { code = "26782", name = "Phường Tân Thới Hiệp" },
                        new { code = "26785", name = "Phường Trung Mỹ Tây" },
                        new { code = "26787", name = "Phường Tân Hưng Thuận" },
                        new { code = "26788", name = "Phường Đông Hưng Thuận" },
                        new { code = "26791", name = "Phường Tân Thới Nhất" }
                    },
                    ["765"] = new List<object> // Quận Bình Thạnh
                    {
                        new { code = "26860", name = "Phường 1" },
                        new { code = "26861", name = "Phường 2" },
                        new { code = "26862", name = "Phường 3" },
                        new { code = "26863", name = "Phường 5" },
                        new { code = "26864", name = "Phường 6" },
                        new { code = "26865", name = "Phường 7" },
                        new { code = "26866", name = "Phường 11" },
                        new { code = "26867", name = "Phường 12" },
                        new { code = "26868", name = "Phường 13" },
                        new { code = "26869", name = "Phường 14" },
                        new { code = "26870", name = "Phường 15" },
                        new { code = "26871", name = "Phường 17" },
                        new { code = "26872", name = "Phường 19" },
                        new { code = "26873", name = "Phường 21" },
                        new { code = "26874", name = "Phường 22" },
                        new { code = "26875", name = "Phường 24" },
                        new { code = "26876", name = "Phường 25" },
                        new { code = "26877", name = "Phường 26" },
                        new { code = "26878", name = "Phường 27" },
                        new { code = "26879", name = "Phường 28" }
                    }
                },
                ["48"] = new Dictionary<string, List<object>> // Đà Nẵng
                {
                    ["492"] = new List<object> // Quận Hải Châu
                    {
                        new { code = "20134", name = "Phường Thạch Thang" },
                        new { code = "20137", name = "Phường Hải Châu I" },
                        new { code = "20140", name = "Phường Hải Châu II" },
                        new { code = "20143", name = "Phường Phước Ninh" },
                        new { code = "20146", name = "Phường Hòa Thuận Tây" },
                        new { code = "20149", name = "Phường Hòa Thuận Đông" },
                        new { code = "20152", name = "Phường Nam Dương" },
                        new { code = "20155", name = "Phường Bình Hiên" },
                        new { code = "20158", name = "Phường Bình Thuận" },
                        new { code = "20161", name = "Phường Hòa Cường Bắc" },
                        new { code = "20164", name = "Phường Hòa Cường Nam" }
                    }
                },
                ["31"] = new Dictionary<string, List<object>> // Hải Phòng
                {
                    ["303"] = new List<object> // Quận Hồng Bàng
                    {
                        new { code = "11179", name = "Phường Hàng Kênh" },
                        new { code = "11182", name = "Phường Hàng Đào" },
                        new { code = "11185", name = "Phường Hàng Bạc" },
                        new { code = "11188", name = "Phường Hàng Gai" },
                        new { code = "11191", name = "Phường Hàng Buồm" },
                        new { code = "11194", name = "Phường Hàng Thùng" },
                        new { code = "11197", name = "Phường Hàng Bồ" },
                        new { code = "11200", name = "Phường Cầu Đất" },
                        new { code = "11203", name = "Phường Lạc Viên" },
                        new { code = "11206", name = "Phường Gia Viên" },
                        new { code = "11209", name = "Phường Đông Khê" },
                        new { code = "11212", name = "Phường Cầu Tre" },
                        new { code = "11215", name = "Phường Lạch Tray" },
                        new { code = "11218", name = "Phường Đổng Quốc Bình" }
                    }
                },
                ["92"] = new Dictionary<string, List<object>> // Cần Thơ
                {
                    ["916"] = new List<object> // Quận Ninh Kiều
                    {
                        new { code = "31168", name = "Phường Tân An" },
                        new { code = "31169", name = "Phường Tân Bình" },
                        new { code = "31171", name = "Phường Hưng Phú" },
                        new { code = "31172", name = "Phường Hưng Thạnh" },
                        new { code = "31174", name = "Phường Ba Láng" },
                        new { code = "31175", name = "Phường Thường Thạnh" },
                        new { code = "31177", name = "Phường Phú Thứ" },
                        new { code = "31178", name = "Phường Tân Phú" },
                        new { code = "31180", name = "Phường Thốt Nốt" },
                        new { code = "31183", name = "Phường Thới Thuận" },
                        new { code = "31186", name = "Phường Thuận An" },
                        new { code = "31189", name = "Phường Tân Lộc" },
                        new { code = "31192", name = "Phường Trung Nhứt" },
                        new { code = "31195", name = "Phường Thạnh Hoà" },
                        new { code = "31198", name = "Phường Trung Kiên" },
                        new { code = "31201", name = "Phường Tân Hưng" },
                        new { code = "31204", name = "Phường Thuận Hưng" }
                    }
                },
                ["74"] = new Dictionary<string, List<object>> // Bình Dương
                {
                    ["718"] = new List<object> // Thành phố Thủ Dầu Một
                    {
                        new { code = "25834", name = "Phường Chánh Mỹ" },
                        new { code = "25835", name = "Phường Chánh Nghĩa" },
                        new { code = "25837", name = "Phường Định Hoà" },
                        new { code = "25838", name = "Phường Hiệp An" },
                        new { code = "25840", name = "Phường Hoà Phú" },
                        new { code = "25841", name = "Phường Phú Cường" },
                        new { code = "25843", name = "Phường Phú Hòa" },
                        new { code = "25844", name = "Phường Phú Lợi" },
                        new { code = "25846", name = "Phường Phú Mỹ" },
                        new { code = "25847", name = "Phường Phú Tân" },
                        new { code = "25849", name = "Phường Phú Thọ" },
                        new { code = "25850", name = "Phường Tân An" },
                        new { code = "25852", name = "Phường Tương Bình Hiệp" }
                    }
                },
                ["75"] = new Dictionary<string, List<object>> // Đồng Nai
                {
                    ["731"] = new List<object> // Thành phố Biên Hòa
                    {
                        new { code = "25999", name = "Phường An Bình" },
                        new { code = "26002", name = "Phường Bình Đa" },
                        new { code = "26005", name = "Phường Bửu Hòa" },
                        new { code = "26008", name = "Phường Bửu Long" },
                        new { code = "26011", name = "Phường Hiệp Hòa" },
                        new { code = "26014", name = "Phường Hố Nai" },
                        new { code = "26017", name = "Phường Hóa An" },
                        new { code = "26020", name = "Phường Long Bình" },
                        new { code = "26023", name = "Phường Long Bình Tân" },
                        new { code = "26026", name = "Phường Phước Tân" },
                        new { code = "26029", name = "Phường Quang Vinh" },
                        new { code = "26032", name = "Phường Quyết Thắng" },
                        new { code = "26035", name = "Phường Tam Hiệp" },
                        new { code = "26038", name = "Phường Tam Hòa" },
                        new { code = "26041", name = "Phường Tam Phước" },
                        new { code = "26044", name = "Phường Tân Biên" },
                        new { code = "26047", name = "Phường Tân Hạnh" },
                        new { code = "26050", name = "Phường Tân Hiệp" },
                        new { code = "26053", name = "Phường Tân Hòa" },
                        new { code = "26056", name = "Phường Tân Mai" },
                        new { code = "26059", name = "Phường Tân Phong" },
                        new { code = "26062", name = "Phường Tân Tiến" },
                        new { code = "26065", name = "Phường Tân Vạn" },
                        new { code = "26068", name = "Phường Thanh Bình" },
                        new { code = "26071", name = "Phường Thống Nhất" },
                        new { code = "26074", name = "Phường Trảng Dài" },
                        new { code = "26077", name = "Phường Trung Dũng" }
                    }
                }
            };

            if (wards.ContainsKey(provinceCode) && wards[provinceCode].ContainsKey(districtCode))
            {
                return Json(wards[provinceCode][districtCode]);
            }

            return Json(new List<object>());
        }
    }
}