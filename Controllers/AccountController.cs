using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Areas.Admin.Repository;
using Shopping_Demo.Models;
using Shopping_Demo.Models.ViewModels;
using Shopping_Demo.Repository;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;

namespace Shopping_Demo.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUserModel> _userManager;
        private SignInManager<AppUserModel> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly DataContext _dataContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AccountController(SignInManager<AppUserModel> signInManager, UserManager<AppUserModel> userManager, IEmailSender emailSender, DataContext dataContext, RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
            _dataContext = dataContext;
            _roleManager = roleManager;
        }

        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        public IActionResult TestOAuth()
        {
            return View();
        }

        public async Task<IActionResult> UpdateAccount()
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateInfoAccount(AppUserModel user)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userById = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (userById == null)
            {
                return NotFound();
            }
            userById.PhoneNumber = user.PhoneNumber;
            userById.FullName = user.FullName;
            userById.Address = user.Address;
            userById.Gender = user.Gender;
            userById.Age = user.Age; // THÊM DÒNG NÀY ĐỂ CẬP NHẬT TUỔI

            _dataContext.Update(userById);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Update Account Information Successfully";

            return RedirectToAction("UpdateAccount", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel login)
        {
            if (ModelState.IsValid)
            {
                // Try to find user by email first
                var user = await _userManager.FindByEmailAsync(login.Email);
                
                // If not found by email, try to find by username
                if (user == null)
                {
                    user = await _userManager.FindByNameAsync(login.Email);
                }
                
                if (user != null)
                {
                    Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(user.UserName, login.Password, false, false);
                    if (result.Succeeded)
                    {
                        await MergeCartAsync(user.Id);
                        await MergeWishlistAsync(user.Id);
                        return Redirect(login.ReturnUrl ?? "/");
                    }
                }
                ModelState.AddModelError("", "Email/Username hoặc mật khẩu không đúng");
            }
            return View(login);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new AppUserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AppUserModel user, string password, string confirmPassword)
        {
            if (ModelState.IsValid)
            {
                // Validate password
                if (string.IsNullOrEmpty(password))
                {
                    ModelState.AddModelError("", "Password is required");
                    return View(user);
                }

                if (password != confirmPassword)
                {
                    ModelState.AddModelError("", "Password confirmation does not match");
                    return View(user);
                }

                if (string.IsNullOrEmpty(user.UserName))
                {
                    user.UserName = "user_" + user.PhoneNumber;
                }
                user.Token = Guid.NewGuid().ToString();
                IdentityResult result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Customer");

                    TempData["success"] = "Đăng ký thành công";
                    return Redirect("/Account/Login");
                }

                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(user);
        }

        public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync();
            HttpContext.Session.Remove("Cart");
            return Redirect(returnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> HistoryPublic(string userEmail = "")
        {
            try
            {
                if (string.IsNullOrEmpty(userEmail))
                {
                    return Content("UserEmail is required");
                }

                var orders = await _dataContext.Orders
                    .Where(od => od.UserName == userEmail)
                    .OrderByDescending(od => od.CreatedDate)
                    .Take(10)
                    .Select(o => new { o.OrderCode, o.UserName, o.CreatedDate, o.Status })
                    .ToListAsync();

                return Json(new { 
                    UserEmail = userEmail, 
                    OrdersCount = orders.Count,
                    Orders = orders 
                });
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }
        }

        public async Task<IActionResult> History(UserModel user)
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var orders = await _dataContext.Orders
                .Include(o => o.User)
                .Where(od => od.UserName == userEmail)
                .OrderByDescending(od => od.CreatedDate)
                .ToListAsync();

            // Get all order details for these orders
            var orderCodes = orders.Select(o => o.OrderCode).ToList();
            var orderDetails = await _dataContext.OrderDetails
                .Include(od => od.Product)
                .Where(od => orderCodes.Contains(od.OrderCode))
                .ToListAsync();

            // Group order details by order code
            var orderDetailsDict = orderDetails.GroupBy(od => od.OrderCode)
                .ToDictionary(g => g.Key, g => g.ToList());

            ViewBag.UserEmail = userEmail;
            ViewBag.OrderDetails = orderDetailsDict;
            return View(orders);
        }

        public async Task<IActionResult> CancelOrder(string ordercode)
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            try
            {
                var order = await _dataContext.Orders.Where(o => o.OrderCode == ordercode).FirstAsync();
                order.Status = 3;
                _dataContext.Update(order);
                await _dataContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                return BadRequest("An error occurred while canceling the order.");
            }
            return RedirectToAction("History", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> SendMailForgetPass(AppUserModel user)
        {
            var checkMail = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (checkMail == null)
            {
                TempData["error"] = "Email not found";
                return RedirectToAction("ForgetPass", "Account");
            }
            else
            {
                string token = Guid.NewGuid().ToString();
                checkMail.Token = token;
                _dataContext.Update(checkMail);
                await _dataContext.SaveChangesAsync();
                var receiver = checkMail.Email;
                var subject = "Change password for user " + checkMail.Email;
                var message = "Click on link to change password " +
                    "<a href='" + $"{Request.Scheme}://{Request.Host}/Account/NewPass?email=" + checkMail.Email + "&token=" + token + "'>Bấm vào đây</a>";

                await _emailSender.SendEmailAsync(receiver, subject, message);
            }

            TempData["success"] = "An email has been sent to your registered email address with password reset instructions.";
            return RedirectToAction("ForgetPass", "Account");
        }

        public IActionResult ForgetPass()
        {
            return View();
        }

        public async Task<IActionResult> NewPass(AppUserModel user, string token)
        {
            var checkuser = await _userManager.Users
                .Where(u => u.Email == user.Email)
                .Where(u => u.Token == user.Token).FirstOrDefaultAsync();

            if (checkuser != null)
            {
                ViewBag.Email = checkuser.Email;
                ViewBag.Token = token;
            }
            else
            {
                TempData["error"] = "Email not found or token is not right";
                return RedirectToAction("ForgetPass", "Account");
            }
            return View();
        }
        public async Task<IActionResult> UpdateNewPassword(AppUserModel user, string token)
        {
            var checkuser = await _userManager.Users
                .Where(u => u.Email == user.Email)
                .Where(u => u.Token == user.Token).FirstOrDefaultAsync();

            if (checkuser != null)
            {
                string newtoken = Guid.NewGuid().ToString();
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var passwordHash = passwordHasher.HashPassword(checkuser, user.PasswordHash);

                checkuser.PasswordHash = passwordHash;
                checkuser.Token = newtoken;

                await _userManager.UpdateAsync(checkuser);
                TempData["success"] = "Password updated successfully.";
                return RedirectToAction("Login", "Account");
            }
            else
            {
                TempData["error"] = "Email not found or token is not right";
                return RedirectToAction("ForgetPass", "Account");
            }
        }
        private async Task MergeCartAsync(string userId)
        {
            var sessionCartItems = HttpContext.Session.GetJson<List<CartItemSessionDTO>>("Cart");

            if (sessionCartItems != null && sessionCartItems.Any())
            {
                var sessionCart = sessionCartItems.Select(dto => dto.ToCartItemModel()).ToList();
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

                foreach (var sessionItem in sessionCart)
                {
                    var existingItem = userCart.CartItems.FirstOrDefault(ci => 
                        ci.ProductId == sessionItem.ProductId && 
                        ci.ColorName == sessionItem.ColorName && 
                        ci.SizeName == sessionItem.SizeName);

                    if (existingItem != null)
                    {
                        existingItem.Quantity += sessionItem.Quantity;
                        _dataContext.Update(existingItem);
                    }
                    else
                    {
                        var newCartItem = new CartItemModel
                        {
                            CartId = userCart.Id,
                            ProductId = sessionItem.ProductId,
                            ProductName = sessionItem.ProductName,
                            Price = sessionItem.Price,
                            Image = sessionItem.Image,
                            Quantity = sessionItem.Quantity,
                            ColorId = sessionItem.ColorId,
                            ColorName = sessionItem.ColorName,
                            SizeId = sessionItem.SizeId,
                            SizeName = sessionItem.SizeName
                        };
                        _dataContext.CartItems.Add(newCartItem);
                    }
                }

                userCart.UpdatedDate = DateTime.Now;
                await _dataContext.SaveChangesAsync();
                SyncCartToSession(userCart.CartItems.ToList());
            }
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
        private async Task MergeWishlistAsync(string userId)
        {
            var sessionWishlist = HttpContext.Session.GetJson<List<WishlistSessionDTO>>("Wishlist");

            if (sessionWishlist != null && sessionWishlist.Any())
            {
                foreach (var item in sessionWishlist)
                {
                    var existingItem = await _dataContext.WishLists
                        .FirstOrDefaultAsync(w => w.ProductId == item.ProductId && w.UserId == userId);

                    if (existingItem == null)
                    {
                        var newWishlistItem = new WishlistModel
                        {
                            ProductId = item.ProductId,
                            UserId = userId
                        };
                        _dataContext.WishLists.Add(newWishlistItem);
                    }
                }

                await _dataContext.SaveChangesAsync();
                await SyncWishlistFromDBAsync(userId);
            }
        }
        private async Task SyncWishlistFromDBAsync(string userId)
        {
            var wishlistItems = await _dataContext.WishLists
                .Include(w => w.Product)
                .Where(w => w.UserId == userId)
                .ToListAsync();

            if (wishlistItems != null && wishlistItems.Any())
            {
                var sessionWishlistItems = wishlistItems.Select(item => new WishlistSessionDTO
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name,
                    Price = item.Product?.Price ?? 0,
                    Image = item.Product?.Image
                }).ToList();

                HttpContext.Session.SetJson("Wishlist", sessionWishlistItems);
            }
            else
            {
                HttpContext.Session.Remove("Wishlist");
            }
        }
        public async Task<IActionResult> ViewOrderClient(string orderCode)
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == orderCode && o.UserName == userEmail);

            if (order == null)
            {
                return NotFound();
            }

            var detailsOrder = await _dataContext.OrderDetails
                .Include(p => p.Product)
                .Where(o => o.OrderCode == orderCode)
                .ToListAsync();

            ViewBag.ShippingCost = order.ShippingCost;
            ViewBag.DiscountAmount = order.DiscountAmount;
            ViewBag.Status = order.Status;
            ViewBag.CouponCode = order.CouponCode;
            ViewBag.ShippingCity = order.ShippingCity;
            ViewBag.ShippingDistrict = order.ShippingDistrict;
            ViewBag.ShippingWard = order.ShippingWard;
            ViewBag.ShippingAddress = order.ShippingAddress;

            return View(detailsOrder);
        }

        // Step 1: Display the form to verify current password
        [HttpGet]
        public IActionResult ChangePassword()
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            return View("VerifyPassword", new CurrentPasswordViewModel());
        }

        // Step 1: Verify the current password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCurrentPassword(CurrentPasswordViewModel model)
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View("VerifyPassword", model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var passwordCheck = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (!passwordCheck)
            {
                ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng.");
                return View("VerifyPassword", model);
            }

            // Password is correct, proceed to step 2
            var newPasswordModel = new NewPasswordViewModel { CurrentPassword = model.CurrentPassword };
            return View("NewPassword", newPasswordModel);
        }

        // Step 2: Update the password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(NewPasswordViewModel model)
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                // Hiển thị tất cả các lỗi xác thực
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine($"Validation error: {error.ErrorMessage}");
                    }
                }
                return View("NewPassword", model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Thử kiểm tra lại mật khẩu hiện tại một lần nữa
            var passwordCheck = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);

            // Update the password
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["success"] = "Mật khẩu đã được thay đổi thành công";
                await _signInManager.SignOutAsync();
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("UpdateAccount", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("NewPassword", model);
        }
        public async Task<IActionResult> RequestReturn(string ordercode)
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            try
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var order = await _dataContext.Orders
                    .Where(o => o.OrderCode == ordercode && o.UserName == userEmail)
                    .FirstOrDefaultAsync();

                if (order == null)
                {
                    TempData["error"] = "Không tìm thấy đơn hàng";
                    return RedirectToAction("History", "Account");
                }

                if (order.Status != 0)
                {
                    TempData["error"] = "Chỉ có thể yêu cầu hoàn hàng cho đơn hàng đã giao thành công";
                    return RedirectToAction("History", "Account");
                }

                if (order.DeliveredDate.HasValue && (DateTime.Now - order.DeliveredDate.Value).TotalDays > 7)
                {
                    TempData["error"] = "Chỉ được phép hoàn hàng trong vòng 7 ngày kể từ ngày giao hàng thành công";
                    return RedirectToAction("History", "Account");
                }

                order.Status = 5; // Định nghĩa 5 là trạng thái "Yêu cầu hoàn hàng"
                _dataContext.Update(order);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Đã gửi yêu cầu hoàn hàng thành công";
            }
            catch (Exception)
            {
                return BadRequest("Đã xảy ra lỗi khi yêu cầu hoàn hàng.");
            }

            return RedirectToAction("History", "Account");
        }

        // External Login Methods
        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            try
            {
                Console.WriteLine($"ExternalLogin called with provider: {provider}");
                var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
                Console.WriteLine($"Redirect URL: {redirectUrl}");
                
                var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
                
                // Thêm cấu hình quan trọng để hiển thị popup
                properties.Items["prompt"] = "select_account";
                properties.Items["access_type"] = "offline";
                
                Console.WriteLine($"Challenge properties configured for provider: {provider}");
                Console.WriteLine($"Properties Items: {string.Join(", ", properties.Items.Select(kv => $"{kv.Key}={kv.Value}"))}");
                
                return Challenge(properties, provider);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ExternalLogin: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["error"] = $"Error initiating {provider} login: {ex.Message}";
                return RedirectToAction("Login");
            }
        }

        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                TempData["error"] = $"Error from external provider: {remoteError}";
                return RedirectToAction("Login");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["error"] = "Error loading external login information.";
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (user != null)
                {
                    await MergeCartAsync(user.Id);
                    await MergeWishlistAsync(user.Id);
                }
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                TempData["error"] = "Account is locked out.";
                return RedirectToAction("Login");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = info.LoginProvider;

                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var name = info.Principal.FindFirstValue(ClaimTypes.Name);
                var pictureUrl = info.Principal.FindFirstValue("picture"); // For profile picture

                var externalLoginViewModel = new ExternalLoginConfirmationViewModel
                {
                    Email = email,
                    Name = name,
                    PictureUrl = pictureUrl,
                    Provider = info.LoginProvider,
                    ProviderKey = info.ProviderKey
                };

                return View("ExternalLoginConfirmation", externalLoginViewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    TempData["error"] = "Error loading external login information during confirmation.";
                    return RedirectToAction("Login");
                }

                var user = new AppUserModel
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.Name,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    Gender = model.Gender,
                    Token = Guid.NewGuid().ToString(),
                    EmailConfirmed = true // External login providers typically confirm email
                };

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "Customer");
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        
                        await MergeCartAsync(user.Id);
                        await MergeWishlistAsync(user.Id);
                        
                        TempData["success"] = "Account created successfully using external login.";
                        return RedirectToLocal(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // Temporary method to create admin user - remove in production
        public async Task<IActionResult> CreateAdminUser()
        {
            try
            {
                // Check if admin role exists
                var adminRole = await _roleManager.FindByNameAsync("Admin");
                if (adminRole == null)
                {
                    adminRole = new IdentityRole("Admin");
                    await _roleManager.CreateAsync(adminRole);
                }

                // Check if admin user exists
                var adminUser = await _userManager.FindByEmailAsync("admin@arum.com");
                if (adminUser == null)
                {
                    adminUser = new AppUserModel
                    {
                        UserName = "admin",
                        Email = "admin@arum.com",
                        EmailConfirmed = true,
                        PhoneNumber = "0123456789",
                        FullName = "Administrator",
                        Address = "Admin Address",
                        Gender = "Male",
                        Token = Guid.NewGuid().ToString()
                    };

                    var result = await _userManager.CreateAsync(adminUser, "Admin123!");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(adminUser, "Admin");
                        TempData["success"] = "Admin user created successfully! Email: admin@arum.com, Password: Admin123!";
                    }
                    else
                    {
                        TempData["error"] = "Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description));
                    }
                }
                else
                {
                    TempData["success"] = "Admin user already exists! Email: admin@arum.com, Password: Admin123!";
                }

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["error"] = "Error creating admin user: " + ex.Message;
                return RedirectToAction("Login");
            }
        }

        // Xác nhận nhận hàng
        [HttpPost]
        public async Task<IActionResult> ConfirmOrderReceived(OrderConfirmationViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                TempData["error"] = "Dữ liệu không hợp lệ";
                return RedirectToAction("ViewOrderClient", new { orderCode = model.OrderCode });
            }

            try
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var order = await _dataContext.Orders
                    .FirstOrDefaultAsync(o => o.OrderCode == model.OrderCode && o.UserName == userEmail);

                if (order == null)
                {
                    TempData["error"] = "Không tìm thấy đơn hàng";
                    return RedirectToAction("History");
                }

                if (order.Status != 0) // Chỉ cho phép xác nhận khi đã giao hàng
                {
                    TempData["error"] = "Chỉ có thể xác nhận đơn hàng đã được giao";
                    return RedirectToAction("ViewOrderClient", new { orderCode = model.OrderCode });
                }

                // Cập nhật trạng thái xác nhận (có thể thêm field IsConfirmed vào OrderModel)
                // Hiện tại chỉ gửi email thông báo
                var subject = "Xác nhận nhận hàng - Mã đơn hàng: " + model.OrderCode;
                var message = $@"
                    <h2>Xác nhận nhận hàng</h2>
                    <p>Khách hàng đã xác nhận nhận hàng thành công.</p>
                    <p><strong>Mã đơn hàng:</strong> {model.OrderCode}</p>
                    <p><strong>Ghi chú:</strong> {model.CustomerNote ?? "Không có"}</p>
                    <p><strong>Thời gian xác nhận:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                ";

                await _emailSender.SendEmailAsync("admin@arum.com", subject, message);
                TempData["success"] = "Đã xác nhận nhận hàng thành công!";
            }
            catch (Exception ex)
            {
                TempData["error"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction("ViewOrderClient", new { orderCode = model.OrderCode });
        }

        // Hiển thị form đánh giá sản phẩm
        [HttpGet]
        public async Task<IActionResult> ReviewProduct(string orderCode, int productId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var order = await _dataContext.Orders
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode && o.UserName == userEmail);

            if (order == null || order.Status != 0)
            {
                TempData["error"] = "Chỉ có thể đánh giá sản phẩm từ đơn hàng đã giao thành công";
                return RedirectToAction("History");
            }

            var product = await _dataContext.Products.FindAsync(productId);
            if (product == null)
            {
                TempData["error"] = "Không tìm thấy sản phẩm";
                return RedirectToAction("History");
            }

            var model = new ProductReviewViewModel
            {
                OrderCode = orderCode,
                ProductId = productId,
                ProductName = product.Name,
                ProductImage = product.Image
            };

            return View(model);
        }

        // Xử lý đánh giá sản phẩm
        [HttpPost]
        public async Task<IActionResult> SubmitReview(ProductReviewViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View("ReviewProduct", model);
            }

            try
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                
                // Kiểm tra xem đã đánh giá chưa
                var existingReview = await _dataContext.ProductReviews
                    .FirstOrDefaultAsync(r => r.OrderCode == model.OrderCode && 
                                            r.ProductId == model.ProductId && 
                                            r.UserName == userEmail);

                if (existingReview != null)
                {
                    TempData["error"] = "Bạn đã đánh giá sản phẩm này rồi";
                    return RedirectToAction("ViewOrderClient", new { orderCode = model.OrderCode });
                }

                var review = new ProductReviewModel
                {
                    OrderCode = model.OrderCode,
                    ProductId = model.ProductId,
                    UserName = userEmail,
                    Rating = model.Rating,
                    Comment = model.Comment,
                    CreatedDate = DateTime.Now
                };

                _dataContext.ProductReviews.Add(review);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Đánh giá sản phẩm thành công!";
                return RedirectToAction("ViewOrderClient", new { orderCode = model.OrderCode });
            }
            catch (Exception ex)
            {
                TempData["error"] = "Có lỗi xảy ra: " + ex.Message;
                return View("ReviewProduct", model);
            }
        }

        // Xác nhận đã nhận hàng
        [HttpGet]
        public async Task<IActionResult> ConfirmReceived(string ordercode)
        {
            if (string.IsNullOrEmpty(ordercode))
            {
                TempData["error"] = "Mã đơn hàng không hợp lệ.";
                return RedirectToAction("History");
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            // Tìm đơn hàng
            var order = await _dataContext.Orders
                .FirstOrDefaultAsync(o => o.OrderCode == ordercode && o.UserName == userEmail);

            if (order == null)
            {
                TempData["error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("History");
            }

            // Kiểm tra trạng thái đơn hàng (chỉ cho phép xác nhận khi đang giao hàng)
            if (order.Status != 4)
            {
                TempData["error"] = "Đơn hàng không ở trạng thái đang giao hàng.";
                return RedirectToAction("History");
            }

            // Cập nhật trạng thái đơn hàng thành "Đã giao hàng"
            order.Status = 0; // Đã giao hàng
            order.DeliveredDate = DateTime.Now;

            _dataContext.Update(order);
            await _dataContext.SaveChangesAsync();

            // Gửi email thông báo cho admin
            await SendOrderReceivedNotificationAsync(order);

            TempData["success"] = "Cảm ơn bạn đã xác nhận nhận hàng! Đơn hàng đã được cập nhật.";
            return RedirectToAction("History");
        }

        private async Task SendOrderReceivedNotificationAsync(OrderModel order)
        {
            try
            {
                // Lấy thông tin chi tiết đơn hàng
                var orderDetails = await _dataContext.OrderDetails
                    .Include(od => od.Product)
                    .Where(od => od.OrderCode == order.OrderCode)
                    .ToListAsync();

                // Tạo nội dung email
                var subject = $"[XÁC NHẬN NHẬN HÀNG] - Đơn hàng #{order.OrderCode}";
                var messageBuilder = new System.Text.StringBuilder();
                
                messageBuilder.AppendLine("<h2 style='color: #28a745;'>✅ Khách hàng đã xác nhận nhận hàng</h2>");
                messageBuilder.AppendLine($"<p><strong>Mã đơn hàng:</strong> {order.OrderCode}</p>");
                messageBuilder.AppendLine($"<p><strong>Khách hàng:</strong> {order.UserName}</p>");
                messageBuilder.AppendLine($"<p><strong>Ngày đặt hàng:</strong> {order.CreatedDate:dd/MM/yyyy HH:mm}</p>");
                messageBuilder.AppendLine($"<p><strong>Ngày xác nhận nhận hàng:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>");
                
                messageBuilder.AppendLine("<h3>Chi tiết đơn hàng:</h3>");
                messageBuilder.AppendLine("<table border='1' cellpadding='8' cellspacing='0' style='border-collapse: collapse; width: 100%; margin-bottom: 20px;'>");
                messageBuilder.AppendLine("<tr style='background-color: #f8f9fa;'>");
                messageBuilder.AppendLine("<th>Sản phẩm</th><th>Màu sắc</th><th>Kích cỡ</th><th>Số lượng</th><th>Đơn giá</th><th>Thành tiền</th>");
                messageBuilder.AppendLine("</tr>");

                decimal totalAmount = 0;
                foreach (var detail in orderDetails)
                {
                    var itemTotal = detail.Price * detail.Quantity;
                    totalAmount += itemTotal;
                    
                    messageBuilder.AppendLine("<tr>");
                    messageBuilder.AppendLine($"<td>{detail.Product?.Name ?? "Sản phẩm"}</td>");
                    messageBuilder.AppendLine($"<td>{detail.ColorName ?? "N/A"}</td>");
                    messageBuilder.AppendLine($"<td>{detail.SizeName ?? "N/A"}</td>");
                    messageBuilder.AppendLine($"<td>{detail.Quantity}</td>");
                    messageBuilder.AppendLine($"<td>{detail.Price:N0}₫</td>");
                    messageBuilder.AppendLine($"<td>{itemTotal:N0}₫</td>");
                    messageBuilder.AppendLine("</tr>");
                }

                messageBuilder.AppendLine("</table>");
                
                messageBuilder.AppendLine("<div style='background-color: #e8f5e8; padding: 15px; border-radius: 8px; margin-top: 20px;'>");
                messageBuilder.AppendLine($"<p><strong>Tổng tiền hàng:</strong> {totalAmount:N0}₫</p>");
                messageBuilder.AppendLine($"<p><strong>Phí vận chuyển:</strong> {order.ShippingCost:N0}₫</p>");
                if (order.DiscountAmount > 0)
                {
                    messageBuilder.AppendLine($"<p><strong>Giảm giá:</strong> -{order.DiscountAmount:N0}₫</p>");
                }
                var finalTotal = totalAmount + order.ShippingCost - order.DiscountAmount;
                messageBuilder.AppendLine($"<p><strong style='color: #28a745; font-size: 1.2em;'>TỔNG THANH TOÁN:</strong> <strong style='color: #28a745; font-size: 1.2em;'>{finalTotal:N0}₫</strong></p>");
                messageBuilder.AppendLine("</div>");

                messageBuilder.AppendLine("<div style='background-color: #fff3cd; padding: 15px; border-radius: 8px; margin-top: 20px;'>");
                messageBuilder.AppendLine("<h4>📋 Thông tin giao hàng:</h4>");
                messageBuilder.AppendLine($"<p><strong>Địa chỉ:</strong> {order.ShippingAddress ?? "Không có thông tin"}</p>");
                messageBuilder.AppendLine($"<p><strong>Phường/Xã:</strong> {order.ShippingWard ?? "Không có thông tin"}</p>");
                messageBuilder.AppendLine($"<p><strong>Quận/Huyện:</strong> {order.ShippingDistrict ?? "Không có thông tin"}</p>");
                messageBuilder.AppendLine($"<p><strong>Tỉnh/Thành:</strong> {order.ShippingCity ?? "Không có thông tin"}</p>");
                messageBuilder.AppendLine($"<p><strong>Phương thức thanh toán:</strong> {order.PaymentMethod ?? "COD"}</p>");
                messageBuilder.AppendLine("</div>");

                messageBuilder.AppendLine("<p style='margin-top: 30px; color: #6c757d; font-style: italic;'>");
                messageBuilder.AppendLine("Email này được gửi tự động từ hệ thống khi khách hàng xác nhận đã nhận được hàng.");
                messageBuilder.AppendLine("</p>");

                // Gửi email cho admin
                await _emailSender.SendEmailAsync("admin@example.com", subject, messageBuilder.ToString());
            }
            catch (Exception ex)
            {
                // Log error nhưng không làm gián đoạn quá trình
                Console.WriteLine($"Lỗi khi gửi email thông báo nhận hàng: {ex.Message}");
            }
        }

        // API để lấy trạng thái đơn hàng cho thông báo real-time
        [HttpGet]
        public async Task<IActionResult> GetOrderStatuses()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            try
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var orders = await _dataContext.Orders
                    .Where(o => o.UserName == userEmail)
                    .Select(o => new { o.OrderCode, o.Status })
                    .ToListAsync();

                return Json(orders);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error retrieving order statuses");
            }
        }
    }
}