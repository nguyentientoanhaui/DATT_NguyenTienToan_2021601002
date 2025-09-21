using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Models;
using Shopping_Demo.Models.ViewModels;
using Shopping_Demo.Repository;
using System.Diagnostics;
using X.PagedList.Extensions;

namespace Shopping_Demo.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUserModel> _userManager;
        public HomeController(ILogger<HomeController> logger, DataContext context, UserManager<AppUserModel> userManager)
        {
            _logger = logger;
            _dataContext = context;
            _userManager = userManager;
        }
        public IActionResult Index(int? page)
        {
            // Lấy sản phẩm nổi bật (featured products) - có thể là sản phẩm mới nhất, bán chạy nhất, hoặc được đánh dấu nổi bật
            var featuredProducts = _dataContext.Products
                .Where(p => p.IsActive)
                .Include("Category")
                .Include("Brand")
                .Include("ProductImages")
                .OrderByDescending(p => p.Sold) // Sắp xếp theo số lượng đã bán
                .ThenByDescending(p => p.Id) // Sau đó theo ID (sản phẩm mới nhất)
                .Take(12) // Lấy 12 sản phẩm nổi bật
                .ToList();

            // Debug: Log số lượng sản phẩm và thông tin
            _logger.LogInformation($"Found {featuredProducts.Count} featured products");
            foreach (var product in featuredProducts.Take(3))
            {
                _logger.LogInformation($"Product: {product.Name}, Image: {product.Image}, Category: {product.Category?.Name}, Brand: {product.Brand?.Name}");
            }

            // var sliders = _dataContext.Sliders.Where(s => s.Status == 1).ToList();
            var sliders = new List<SliderModel>(); // Temporary empty list since Sliders table was removed

            var bestSellingProducts = _dataContext.Products
                .Where(p => p.IsActive)
                .Include("Category")
                .Include("Brand")
                .Include("ProductImages")
                .OrderByDescending(p => p.Sold)
                .Take(6)
                .ToList();

            // Lấy danh sách brands để hiển thị trong carousel
            var brands = _dataContext.Brands
                .Where(b => b.Status == 1)
                .OrderBy(b => b.Name)
                .ToList();

            ViewBag.Sliders = sliders;
            ViewBag.BestSellingProducts = bestSellingProducts;
            ViewBag.Brands = brands;

            return View(featuredProducts);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult ChatbotTest()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statuscode)
        {
            _logger.LogWarning($"Error page accessed with status code: {statuscode}");
            
            if (statuscode == 404)
            {
                _logger.LogInformation("Returning 404 Not Found page");
                return View("NotFound");
            }
            else if (statuscode == 403)
            {
                _logger.LogWarning("Returning 403 Forbidden page");
                ViewBag.ErrorMessage = "Bạn không có quyền truy cập trang này.";
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
            else if (statuscode == 500)
            {
                _logger.LogError("Returning 500 Internal Server Error page");
                ViewBag.ErrorMessage = "Đã xảy ra lỗi máy chủ. Vui lòng thử lại sau.";
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
            else
            {
                _logger.LogWarning($"Returning generic error page for status code: {statuscode}");
                ViewBag.ErrorMessage = $"Đã xảy ra lỗi với mã: {statuscode}";
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
        public async Task<IActionResult> Contact()
        {
            var contact = await _dataContext.Contact.FirstAsync();
            return View(contact);
        }

        [HttpGet]
        public IActionResult SimpleTest()
        {
            return View("SimpleTest");
        }

        [HttpGet]
        public IActionResult TestWishlistPage()
        {
            return View("TestWishlist");
        }

        [HttpGet]
        public IActionResult TestWishlist()
        {
            return Json(new { success = true, message = "Điểm cuối thử nghiệm hoạt động!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetUserWishlist()
        {
            try
            {
                var wishlistedProductIds = new List<int>();

                if (User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        wishlistedProductIds = await _dataContext.WishLists
                            .Where(w => w.UserId == user.Id)
                            .Select(w => w.ProductId)
                            .ToListAsync();
                    }
                }
                else
                {
                    // Lấy từ session cho user chưa đăng nhập
                    var sessionWishlist = HttpContext.Session.GetJson<List<WishlistSessionDTO>>("Wishlist");
                    if (sessionWishlist != null)
                    {
                        wishlistedProductIds = sessionWishlist.Select(w => w.ProductId).ToList();
                    }
                }

                return Json(new { success = true, wishlistedProductIds });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user wishlist");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy danh sách yêu thích" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DebugWishlist()
        {
            try
            {
                var userId = User.Identity.IsAuthenticated ? (await _userManager.GetUserAsync(User))?.Id : null;
                var dbWishlistCount = 0;
                
                if (User.Identity.IsAuthenticated && userId != null)
                {
                    dbWishlistCount = await _dataContext.WishLists.CountAsync(w => w.UserId == userId);
                }
                
                var debugInfo = new
                {
                    IsAuthenticated = User.Identity.IsAuthenticated,
                    UserId = userId,
                    SessionWishlist = HttpContext.Session.GetJson<List<WishlistSessionDTO>>("Wishlist")?.Count ?? 0,
                    DbWishlistCount = dbWishlistCount
                };

                return Json(new { success = true, debugInfo });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddWishList(int Id)
        {
            _logger.LogInformation($"=== AddWishList START === ProductId: {Id}");
            _logger.LogInformation($"User authenticated: {User.Identity.IsAuthenticated}");
            _logger.LogInformation($"User name: {User.Identity.Name}");
            
            try
            {
                // Validate input
                if (Id <= 0)
                {
                    _logger.LogWarning($"Invalid product ID: {Id}");
                    return Json(new { success = false, message = "ID sản phẩm không hợp lệ" });
                }

                var product = await _dataContext.Products.FindAsync(Id);
                _logger.LogInformation($"Product found: {product?.Name ?? "NULL"}");

                if (product == null)
                {
                    _logger.LogWarning($"Product not found with ID: {Id}");
                    return Json(new { success = false, message = "Sản phẩm không tồn tại" });
                }

                if (!product.IsActive)
                {
                    _logger.LogWarning($"Product {Id} is not active");
                    return Json(new { success = false, message = "Sản phẩm không khả dụng" });
                }

                if (User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.GetUserAsync(User);
                    _logger.LogInformation($"User found: {user?.UserName ?? "NULL"}, UserId: {user?.Id ?? "NULL"}");
                    
                    if (user == null)
                    {
                        _logger.LogWarning("User not found despite being authenticated");
                        return Json(new { success = false, message = "Không tìm thấy thông tin người dùng" });
                    }

                    // Kiểm tra xem sản phẩm đã có trong wishlist chưa
                    var existingWishlist = await _dataContext.WishLists
                        .FirstOrDefaultAsync(w => w.ProductId == Id && w.UserId == user.Id);
                    
                    _logger.LogInformation($"Existing wishlist item: {existingWishlist != null}");

                    if (existingWishlist != null)
                    {
                        _logger.LogInformation($"Product {Id} already in wishlist for user {user.Id}");
                        return Json(new { success = true, message = "Sản phẩm đã có trong danh sách yêu thích" });
                    }

                    var wishlistProduct = new WishlistModel
                    {
                        ProductId = Id,
                        UserId = user.Id
                    };

                    _dataContext.WishLists.Add(wishlistProduct);
                    var saveResult = await _dataContext.SaveChangesAsync();
                    _logger.LogInformation($"SaveChanges result: {saveResult} rows affected");
                    
                    _logger.LogInformation($"Successfully added product {Id} to wishlist for user {user.Id}");
                    return Json(new { success = true, message = "Thêm vào danh sách yêu thích thành công" });
                }
                else
                {
                    // Người dùng chưa đăng nhập, lưu vào session
                    List<WishlistSessionDTO> wishlist = HttpContext.Session.GetJson<List<WishlistSessionDTO>>("Wishlist") ?? new List<WishlistSessionDTO>();
                    _logger.LogInformation($"Session wishlist count before: {wishlist.Count}");

                    // Kiểm tra xem sản phẩm đã có trong wishlist chưa
                    if (wishlist.Any(w => w.ProductId == Id))
                    {
                        _logger.LogInformation($"Product {Id} already in session wishlist");
                        return Json(new { success = true, message = "Sản phẩm đã có trong danh sách yêu thích" });
                    }

                    wishlist.Add(new WishlistSessionDTO(product));
                    HttpContext.Session.SetJson("Wishlist", wishlist);
                    _logger.LogInformation($"Session wishlist count after: {wishlist.Count}");
                    
                    _logger.LogInformation($"Successfully added product {Id} to session wishlist");
                    return Json(new { success = true, message = "Thêm vào danh sách yêu thích thành công" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding product {Id} to wishlist: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
            finally
            {
                _logger.LogInformation($"=== AddWishList END === ProductId: {Id}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var product = await _dataContext.Products.FindAsync(productId);

            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });
            }

            if (product.Quantity <= 0)
            {
                return Json(new { success = false, message = "Sản phẩm đã hết hàng" });
            }

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var userCart = await _dataContext.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == user.Id);

                if (userCart == null)
                {
                    userCart = new CartModel
                    {
                        UserId = user.Id,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        CartItems = new List<CartItemModel>()
                    };
                    _dataContext.Carts.Add(userCart);
                }

                var cartItem = userCart.CartItems.FirstOrDefault(x => x.ProductId == productId);
                var currentCartQuantity = cartItem?.Quantity ?? 0;

                // Kiểm tra tổng số lượng trong giỏ hàng không được vượt quá số lượng sản phẩm có sẵn
                if (currentCartQuantity >= product.Quantity)
                {
                    return Json(new { success = false, message = $"Sản phẩm chỉ còn {product.Quantity} chiếc trong kho" });
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

                return Json(new { success = true, message = "Thêm vào giỏ hàng thành công" });
            }
            else
            {
                // Người dùng chưa đăng nhập, lưu vào session
                var sessionCartItems = HttpContext.Session.GetJson<List<CartItemSessionDTO>>("Cart") ?? new List<CartItemSessionDTO>();

                var existingItem = sessionCartItems.FirstOrDefault(x => x.ProductId == productId);
                var currentCartQuantity = existingItem?.Quantity ?? 0;

                // Kiểm tra tổng số lượng trong giỏ hàng không được vượt quá số lượng sản phẩm có sẵn
                if (currentCartQuantity >= product.Quantity)
                {
                    return Json(new { success = false, message = $"Sản phẩm chỉ còn {product.Quantity} chiếc trong kho" });
                }

                if (existingItem == null)
                {
                    sessionCartItems.Add(new CartItemSessionDTO(product));
                }
                else
                {
                    existingItem.Quantity += 1;
                }

                HttpContext.Session.SetJson("Cart", sessionCartItems);

                return Json(new { success = true, message = "Thêm vào giỏ hàng thành công" });
            }
        }


        public async Task<IActionResult> Wishlist()
        {
            try
            {
                List<dynamic> wishlistItems = new List<dynamic>();

                if (User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.GetUserAsync(User);
                    
                    if (user != null)
                    {
                        // Lấy wishlist từ DB cho user đã đăng nhập
                        var dbWishlistItems = await _dataContext.WishLists
                            .Include(w => w.Product)
                            .ThenInclude(p => p.Category)
                            .Include(w => w.Product)
                            .ThenInclude(p => p.Brand)
                            .Where(w => w.UserId == user.Id)
                            .ToListAsync();

                        foreach (var item in dbWishlistItems)
                        {
                            if (item.Product != null)
                            {
                                wishlistItems.Add(new
                                {
                                    Product = item.Product,
                                    WishList = item
                                });
                            }
                        }
                        
                        _logger.LogInformation($"Found {wishlistItems.Count} wishlist items for user {user.Id}");
                    }
                }
                else
                {
                    // Lấy wishlist từ session cho user chưa đăng nhập
                    var sessionWishlist = HttpContext.Session.GetJson<List<WishlistSessionDTO>>("Wishlist") ?? new List<WishlistSessionDTO>();

                    foreach (var item in sessionWishlist)
                    {
                        var product = await _dataContext.Products
                            .Include(p => p.Category)
                            .Include(p => p.Brand)
                            .FirstOrDefaultAsync(p => p.Id == item.ProductId);
                            
                        if (product != null)
                        {
                            wishlistItems.Add(new
                            {
                                Product = product,
                                WishList = new WishlistModel
                                {
                                    Id = 0, // Id tạm thời cho mục đích hiển thị
                                    ProductId = item.ProductId
                                }
                            });
                        }
                    }
                    
                    _logger.LogInformation($"Found {wishlistItems.Count} session wishlist items");
                }

                return View(wishlistItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Wishlist action");
                return View(new List<dynamic>());
            }
        }


        public async Task<IActionResult> DeleteWishlist(int Id)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.GetUserAsync(User);
                    
                    if (user != null)
                    {
                        // Tìm item trong DB dựa vào ProductId và UserId
                        var wishlistItem = await _dataContext.WishLists
                            .FirstOrDefaultAsync(w => w.ProductId == Id && w.UserId == user.Id);

                        if (wishlistItem != null)
                        {
                            _dataContext.WishLists.Remove(wishlistItem);
                            await _dataContext.SaveChangesAsync();
                            _logger.LogInformation($"Removed product {Id} from wishlist for user {user.Id}");
                        }
                        else
                        {
                            _logger.LogWarning($"Wishlist item not found for product {Id} and user {user.Id}");
                        }
                    }
                }
                else
                {
                    // Xóa khỏi session
                    var sessionWishlist = HttpContext.Session.GetJson<List<WishlistSessionDTO>>("Wishlist");

                    if (sessionWishlist != null)
                    {
                        var removedCount = sessionWishlist.RemoveAll(w => w.ProductId == Id);
                        
                        if (removedCount > 0)
                        {
                            if (sessionWishlist.Any())
                            {
                                HttpContext.Session.SetJson("Wishlist", sessionWishlist);
                            }
                            else
                            {
                                HttpContext.Session.Remove("Wishlist");
                            }
                            _logger.LogInformation($"Removed product {Id} from session wishlist");
                        }
                        else
                        {
                            _logger.LogWarning($"Product {Id} not found in session wishlist");
                        }
                    }
                }

                TempData["success"] = "Xóa sản phẩm khỏi danh sách yêu thích thành công";
                return RedirectToAction("Wishlist", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting wishlist item for product {Id}");
                TempData["error"] = "Có lỗi xảy ra khi xóa sản phẩm khỏi danh sách yêu thích";
                return RedirectToAction("Wishlist", "Home");
            }
        }

        // Phương thức đồng bộ Wishlist từ DB vào Session
        private async Task MergeWishlistAsync(string userId)
        {
            // Lấy wishlist từ session
            var sessionWishlist = HttpContext.Session.GetJson<List<WishlistSessionDTO>>("Wishlist");

            if (sessionWishlist != null && sessionWishlist.Any())
            {
                foreach (var item in sessionWishlist)
                {
                    // Kiểm tra xem sản phẩm đã có trong wishlist của user chưa
                    var existingItem = await _dataContext.WishLists
                        .FirstOrDefaultAsync(w => w.ProductId == item.ProductId && w.UserId == userId);

                    if (existingItem == null)
                    {
                        // Nếu chưa có thì thêm mới
                        var newWishlistItem = new WishlistModel
                        {
                            ProductId = item.ProductId,
                            UserId = userId
                        };

                        _dataContext.WishLists.Add(newWishlistItem);
                    }
                }

                await _dataContext.SaveChangesAsync();

                // Xóa session wishlist sau khi đã lưu vào DB
                HttpContext.Session.Remove("Wishlist");
            }
        }
        public async Task<IActionResult> AddCompare(int Id, CompareModel compare)
        {
            // Kiểm tra user đã đăng nhập
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập để sử dụng chức năng so sánh" });
            }

            // Kiểm tra sản phẩm đã tồn tại trong danh sách so sánh chưa
            var existingCompare = await _dataContext.Compares
                .FirstOrDefaultAsync(c => c.ProductId == Id && c.UserId == user.Id);

            if (existingCompare != null)
            {
                return BadRequest(new { success = false, message = "Sản phẩm đã có trong danh sách so sánh" });
            }

            // Kiểm tra giới hạn số lượng sản phẩm so sánh (tối đa 4)
            var userCompareCount = await _dataContext.Compares
                .Where(c => c.UserId == user.Id)
                .CountAsync();

            if (userCompareCount >= 4)
            {
                return BadRequest(new { success = false, message = "Bạn chỉ có thể so sánh tối đa 4 sản phẩm" });
            }

            var compareProduct = new CompareModel
            {
                ProductId = Id,
                UserId = user.Id
            };

            _dataContext.Compares.Add(compareProduct);
            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Đã thêm sản phẩm vào danh sách so sánh thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product to compare");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi thêm sản phẩm vào danh sách so sánh" });
            }
        }

        // Action để lấy hình ảnh sản phẩm từ database
        public async Task<IActionResult> GetProductImage(int productId, int? imageId = null)
        {
            try
            {
                var product = await _dataContext.Products
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.Id == productId);

                if (product == null)
                {
                    return NotFound();
                }

                string imageUrl = null;

                // Nếu có imageId, lấy ảnh cụ thể từ ProductImages
                if (imageId.HasValue && product.ProductImages != null)
                {
                    var specificImage = product.ProductImages.FirstOrDefault(pi => pi.Id == imageId.Value);
                    if (specificImage != null)
                    {
                        // Ưu tiên ImageUrl, nếu không có thì dùng ImageName
                        imageUrl = !string.IsNullOrEmpty(specificImage.ImageUrl) ? specificImage.ImageUrl : specificImage.ImageName;
                    }
                }

                // Nếu không có imageId hoặc không tìm thấy ảnh cụ thể, lấy ảnh mặc định
                if (string.IsNullOrEmpty(imageUrl))
                {
                    // Ưu tiên lấy hình ảnh từ ProductImages có URL
                    var productImageWithUrl = product.ProductImages?.FirstOrDefault(pi => !string.IsNullOrEmpty(pi.ImageUrl));
                    
                    if (productImageWithUrl != null)
                    {
                        imageUrl = productImageWithUrl.ImageUrl;
                    }
                    else
                    {
                        // Nếu không có URL, lấy ảnh mặc định từ ProductImages
                        var productImage = product.ProductImages?.FirstOrDefault(pi => pi.IsDefault) 
                            ?? product.ProductImages?.FirstOrDefault();

                        if (productImage != null)
                        {
                            imageUrl = productImage.ImageName;
                        }
                        else if (!string.IsNullOrEmpty(product.Image))
                        {
                            imageUrl = product.Image;
                        }
                    }
                }

                // Nếu imageUrl là URL, redirect đến URL đó trực tiếp
                if (!string.IsNullOrEmpty(imageUrl) && (imageUrl.StartsWith("http://") || imageUrl.StartsWith("https://")))
                {
                    return Redirect(imageUrl);
                }

                // Nếu imageUrl là file local, tìm trong thư mục media
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media", "products", imageUrl);
                    
                    if (System.IO.File.Exists(imagePath))
                    {
                        var fileBytes = await System.IO.File.ReadAllBytesAsync(imagePath);
                        var extension = Path.GetExtension(imageUrl).ToLowerInvariant();
                        var contentType = extension switch
                        {
                            ".jpg" or ".jpeg" => "image/jpeg",
                            ".png" => "image/png",
                            ".webp" => "image/webp",
                            ".svg" => "image/svg+xml",
                            _ => "image/jpeg"
                        };
                        return File(fileBytes, contentType);
                    }
                }

                // Nếu không có hình ảnh, trả về placeholder
                return File("~/images/placeholder-product.svg", "image/svg+xml");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProductImage: {ex.Message}");
                return File("~/images/placeholder-product.svg", "image/svg+xml");
            }
        }
        public async Task<IActionResult> TestProductImages()
        {
            try
            {
                // Lấy sản phẩm có tên chứa "Ref"
                var productsWithRef = await _dataContext.Products
                    .Where(p => p.Name.Contains("Ref"))
                    .Include(p => p.ProductImages)
                    .Take(5)
                    .ToListAsync();

                var result = new List<object>();
                foreach (var product in productsWithRef)
                {
                    result.Add(new
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Image = product.Image,
                        ProductImagesCount = product.ProductImages?.Count ?? 0,
                        ProductImages = product.ProductImages?.Select(pi => new
                        {
                            Id = pi.Id,
                            ImageName = pi.ImageName,
                            ImageUrl = pi.ImageUrl,
                            IsDefault = pi.IsDefault
                        }).ToList()
                    });
                }

                return Json(new { success = true, products = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        public async Task<IActionResult> Compare()
        {
            // Kiểm tra user đã đăng nhập
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["error"] = "Vui lòng đăng nhập để xem danh sách so sánh";
                return RedirectToAction("Login", "Account");
            }

            var compare_products = await _dataContext.Compares
                .Where(c => c.UserId == user.Id)
                .Include(c => c.Product)
                .ThenInclude(p => p.Brand)
                .Include(c => c.Product)
                .ThenInclude(p => p.Category)
                .Include(c => c.Product)
                .ThenInclude(p => p.ProductImages)
                .Select(c => new { User = user, Product = c.Product, Compares = c })
                .ToListAsync();

            return View(compare_products);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteCompare(int Id)
        {
            try
            {
                // Kiểm tra user đã đăng nhập
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để sử dụng chức năng này" });
                }

                // Tìm và kiểm tra quyền xóa
                var compare = await _dataContext.Compares
                    .FirstOrDefaultAsync(c => c.Id == Id && c.UserId == user.Id);

                if (compare == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm so sánh hoặc bạn không có quyền xóa" });
                }

                _dataContext.Compares.Remove(compare);
                await _dataContext.SaveChangesAsync();
                
                return Json(new { success = true, message = "Sản phẩm đã được xóa khỏi danh sách so sánh thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting compare item {Id}");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa sản phẩm: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClearAllCompare()
        {
            try
            {
                // Kiểm tra user đã đăng nhập
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để sử dụng chức năng này" });
                }

                // Lấy tất cả sản phẩm so sánh của user
                var userCompares = await _dataContext.Compares
                    .Where(c => c.UserId == user.Id)
                    .ToListAsync();

                if (!userCompares.Any())
                {
                    return Json(new { success = false, message = "Danh sách so sánh đã trống" });
                }

                _dataContext.Compares.RemoveRange(userCompares);
                await _dataContext.SaveChangesAsync();
                
                return Json(new { success = true, message = "Đã xóa tất cả sản phẩm khỏi danh sách so sánh thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing all compare items");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa danh sách so sánh: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCompareCount()
        {
            try
            {
                // Kiểm tra user đã đăng nhập
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để sử dụng chức năng so sánh" });
                }

                // Đếm số lượng sản phẩm trong danh sách so sánh
                var compareCount = await _dataContext.Compares
                    .Where(c => c.UserId == user.Id)
                    .CountAsync();

                return Json(new { 
                    success = true, 
                    count = compareCount,
                    isFull = compareCount >= 4,
                    message = compareCount >= 4 ? "Danh sách so sánh đã đầy (tối đa 4 sản phẩm)" : $"Đã có {compareCount}/4 sản phẩm trong danh sách so sánh"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting compare count");
                return Json(new { success = false, message = "Có lỗi xảy ra khi kiểm tra danh sách so sánh" });
            }
        }

        //public async Task<IActionResult> Wishlist()
        //{
        //    var wishlist_product = await (from w in _dataContext.WishLists
        //                                  join p in _dataContext.Products on w.ProductId equals p.Id
        //                                  select new { Product = p, WishLists = w })
        //                       .ToListAsync();

        //    return View(wishlist_product);
        //}
        //public async Task<IActionResult> DeleteWishlist(int Id)
        //{
        //    WishlistModel wishlist = await _dataContext.WishLists.FindAsync(Id);

        //    _dataContext.WishLists.Remove(wishlist);

        //    await _dataContext.SaveChangesAsync();
        //    TempData["success"] = "Yêu thích đã được xóa thành công";
        //    return RedirectToAction("Wishlist", "Home");
        //}

    }
}
