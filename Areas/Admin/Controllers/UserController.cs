using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Areas.Admin.Models.ViewModels;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Shopping_Demo.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = "Admin,Manager")] // Tạm thời comment để test
    public class UserController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private SignInManager<AppUserModel> _signInManager;
        private readonly DataContext _dataContext;
        public UserController(
            UserManager<AppUserModel> userManager, 
            RoleManager<IdentityRole> roleManager, 
            DataContext context, 
            SignInManager<AppUserModel> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dataContext = context;
            _signInManager = signInManager;
        }
        [HttpGet]
        public async Task<IActionResult> AdminLogout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home", new { area = "" });
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _dataContext.Users.ToListAsync();

            // Lấy danh sách các role của user (UserId, RoleName)
            var userRoles = await (from ur in _dataContext.UserRoles
                                   join r in _dataContext.Roles on ur.RoleId equals r.Id
                                   select new { ur.UserId, RoleName = r.Name }).ToListAsync();

            // Tạo danh sách ViewModel kết hợp user + role
            var usersWithRoles = users.Select(u => new UserWithRoleViewModel
            {
                User = u,
                RoleName = userRoles.FirstOrDefault(ur => ur.UserId == u.Id)?.RoleName ?? "No Role"
            }).ToList();

            var loggedInUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.loggedInUser = loggedInUser;

            return View(usersWithRoles);

        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            return View(new AppUserModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppUserModel user)
        {
            if (ModelState.IsValid)
            {
                var createUser = await _userManager.CreateAsync(user, user.PasswordHash);
                if (createUser.Succeeded)
                {
                    var createUserResult = await _userManager.FindByEmailAsync(user.Email);
                    var userId = createUserResult.Id;
                    var role = _roleManager.FindByIdAsync(user.RoleId);

                    var addToRoleResult = await _userManager.AddToRoleAsync(createUserResult, role.Result.Name);
                    if (!addToRoleResult.Succeeded)
                    {
                        foreach (var error in addToRoleResult.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View(user);
                    }

                    return RedirectToAction("Index", "User");
                }
                else
                {
                    foreach (var error in createUser.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(user);
                }
            }
            else
            {
                TempData["error"] = "Modelu có 1 vài phần bị lỗi";
                List<string> errors = new List<string>();
                foreach (var val in ModelState.Values)
                {
                    foreach (var error in val.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Delete(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                return NotFound();
            }

            // Begin a transaction to ensure all operations succeed or fail together
            using var transaction = await _dataContext.Database.BeginTransactionAsync();
            try
            {
                // 1. Delete user reviews
                var userReviews = await _dataContext.ProductReviews.Where(r => r.UserName == user.Email).ToListAsync();
                if (userReviews.Any())
                {
                    _dataContext.ProductReviews.RemoveRange(userReviews);
                    await _dataContext.SaveChangesAsync();
                }

                // 2. Delete user wishlist items
                var wishlistItems = await _dataContext.WishLists.Where(w => w.UserId == Id).ToListAsync();
                if (wishlistItems.Any())
                {
                    _dataContext.WishLists.RemoveRange(wishlistItems);
                    await _dataContext.SaveChangesAsync();
                }

                // 3. Delete user cart items and cart
                var cart = await _dataContext.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == Id);

                if (cart != null)
                {
                    // Remove cart items first
                    if (cart.CartItems != null && cart.CartItems.Any())
                    {
                        _dataContext.RemoveRange(cart.CartItems);
                        await _dataContext.SaveChangesAsync();
                    }

                    // Then remove the cart itself
                    _dataContext.Carts.Remove(cart);
                    await _dataContext.SaveChangesAsync();
                }

                // 4. Finally delete the user
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    // If user deletion fails, roll back all changes
                    await transaction.RollbackAsync();
                    return View("Error");
                }

                // If everything succeeded, commit the transaction
                await transaction.CommitAsync();
                return RedirectToAction("Index", "User");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return View("Error");
            }
        }
        [HttpGet]
        public async Task<IActionResult> ToggleLockout(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                return NotFound();
            }

            try
            {
                // Kiểm tra trạng thái khóa hiện tại
                var isLocked = user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow;
                IdentityResult result;

                if (isLocked)
                {
                    // Nếu tài khoản đang khóa, mở khóa
                    result = await _userManager.SetLockoutEndDateAsync(user, null);
                    TempData["success"] = "Đã mở khóa tài khoản thành công";
                }
                else
                {
                    // Nếu tài khoản chưa khóa, khóa tài khoản (khóa đến năm 9999)
                    result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                    TempData["success"] = "Đã khóa tài khoản thành công";
                }

                if (!result.Succeeded)
                {
                    TempData["error"] = "Không thể thay đổi trạng thái khóa của tài khoản";
                    return RedirectToAction("Index");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = "Đã xảy ra lỗi khi thay đổi trạng thái khóa";
                return RedirectToAction("Index");
            }
        }

        // Temporary method to fix user roles - remove authorization temporarily
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> FixUserRoles()
        {
            try
            {
                // Create roles if they don't exist
                var roles = new[] { "Admin", "Manager", "Staff", "Sale", "Shipper", "Customer" };
                foreach (var roleName in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                // Get all users
                var users = await _userManager.Users.ToListAsync();
                var results = new List<string>();

                foreach (var user in users)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    results.Add($"User: {user.Email} - Current Roles: {string.Join(", ", userRoles)}");

                    // If user has no roles, assign Customer role
                    if (!userRoles.Any())
                    {
                        await _userManager.AddToRoleAsync(user, "Customer");
                        results.Add($"  -> Assigned Customer role to {user.Email}");
                    }
                }

                // Create admin user if doesn't exist
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

                    var createResult = await _userManager.CreateAsync(adminUser, "Admin123!");
                    if (createResult.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(adminUser, "Admin");
                        results.Add($"Created admin user: admin@arum.com with password: Admin123!");
                    }
                    else
                    {
                        results.Add($"Failed to create admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    var adminRoles = await _userManager.GetRolesAsync(adminUser);
                    if (!adminRoles.Contains("Admin"))
                    {
                        await _userManager.AddToRoleAsync(adminUser, "Admin");
                        results.Add($"Added Admin role to existing user: {adminUser.Email}");
                    }
                    else
                    {
                        results.Add($"Admin user already has Admin role: {adminUser.Email}");
                    }
                }

                ViewBag.Results = results;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAllUsersExceptAdmin()
        {
            try
            {
                // Lấy tất cả users trừ admin@aurum.com
                var usersToDelete = await _userManager.Users
                    .Where(u => u.Email != "admin@aurum.com")
                    .ToListAsync();

                int deletedCount = 0;
                using var transaction = await _dataContext.Database.BeginTransactionAsync();

                try
                {
                    foreach (var user in usersToDelete)
                    {
                        // 1. Xóa user reviews
                        var userReviews = await _dataContext.ProductReviews.Where(r => r.UserName == user.Email).ToListAsync();
                        if (userReviews.Any())
                        {
                            _dataContext.ProductReviews.RemoveRange(userReviews);
                        }

                        // 2. Xóa user wishlist items
                        var wishlistItems = await _dataContext.WishLists.Where(w => w.UserId == user.Id).ToListAsync();
                        if (wishlistItems.Any())
                        {
                            _dataContext.WishLists.RemoveRange(wishlistItems);
                        }

                        // 3. Xóa user cart items và cart
                        var cart = await _dataContext.Carts
                            .Include(c => c.CartItems)
                            .FirstOrDefaultAsync(c => c.UserId == user.Id);

                        if (cart != null)
                        {
                            if (cart.CartItems != null && cart.CartItems.Any())
                            {
                                _dataContext.RemoveRange(cart.CartItems);
                            }
                            _dataContext.Carts.Remove(cart);
                        }

                        // 4. Xóa user
                        var result = await _userManager.DeleteAsync(user);
                        if (result.Succeeded)
                        {
                            deletedCount++;
                        }
                    }

                    await _dataContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["success"] = $"Đã xóa thành công {deletedCount} tài khoản (giữ lại admin@aurum.com)!";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["error"] = "Có lỗi xảy ra khi xóa dữ liệu: " + ex.Message;
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AppUserModel user, string Id)
        {
            var existingUser = await _userManager.FindByIdAsync(Id);
            if (existingUser == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                existingUser.UserName = user.UserName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.RoleId = user.RoleId;
                existingUser.Address = user.Address;
                existingUser.Gender = user.Gender;
                existingUser.FullName = user.FullName;

                var updateUserResult = await _userManager.UpdateAsync(existingUser);
                if (updateUserResult.Succeeded)
                {
                    TempData["success"] = "Cập nhật thông tin thành công";
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    foreach (var error in updateUserResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(existingUser);
                }
            }

            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            TempData["error"] = "Model validation failed.";
            var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
            string errorMessage = string.Join("\n", errors);

            return View(existingUser);
        }
    }
}
