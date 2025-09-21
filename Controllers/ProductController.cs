using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Models;
using Shopping_Demo.Models.ViewModels;
using Shopping_Demo.Repository;
using Shopping_Demo.Services;
using System.Security.Claims;
using X.PagedList;

namespace Shopping_Demo.Controllers
{
    public class ProductController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly DataContext _dataContext;
        private readonly IBadWordsService _badWordsService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IRecommendationService _recommendationService;
        
        public ProductController(DataContext context, UserManager<AppUserModel> userManager, IBadWordsService badWordsService, IWebHostEnvironment webHostEnvironment, IRecommendationService recommendationService)
        {
            _dataContext = context;
            _userManager = userManager;
            _badWordsService = badWordsService;
            _webHostEnvironment = webHostEnvironment;
            _recommendationService = recommendationService;
        }
        public async Task<IActionResult> Index(int? page, int? pageSize, string sortBy = "featured", 
            string[] models = null, string[] brands = null, decimal? minPrice = null, decimal? maxPrice = null,
            string[] colors = null, string[] sizes = null, string[] conditions = null, string[] genders = null,
            string[] categories = null, int? minQuantity = null, int? maxQuantity = null,
            string[] materials = null, string[] crystals = null, string[] bezels = null, string[] dials = null,
            string[] waterResistance = null, string[] movements = null, string[] complications = null,
            string[] braceletMaterials = null, string[] braceletTypes = null, string[] clasps = null)
        {
            try
            {
                int currentPage = page ?? 1;
                int itemsPerPage = pageSize ?? 12;
            
            // Get all active products
            IQueryable<ProductModel> query = _dataContext.Products
                .Where(p => p.IsActive)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages);
                // .Include(p => p.ProductColors).ThenInclude(pc => pc.Color) // Disabled - table removed
                // .Include(p => p.ProductSizes).ThenInclude(ps => ps.Size); // Disabled - table removed
            
            // Apply filters
            if (models != null && models.Any())
            {
                query = query.Where(p => models.Contains(p.Model));
            }
            
            if (brands != null && brands.Any())
            {
                query = query.Where(p => brands.Contains(p.Brand.Name));
            }
            
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }
            
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }
            
            // Apply additional filters
            if (colors != null && colors.Any())
            {
                query = query.Where(p => colors.Contains(p.DialColor));
            }
            
            if (sizes != null && sizes.Any())
            {
                query = query.Where(p => sizes.Contains(p.CaseSize));
            }
            
            // Filter by condition (using Description field)
            if (conditions != null && conditions.Any())
            {
                query = query.Where(p => conditions.Any(c => p.Description.Contains(c)));
            }
            
            // Filter by gender (using Description field)
            if (genders != null && genders.Any())
            {
                query = query.Where(p => genders.Any(g => p.Description.Contains(g)));
            }
            
            // Filter by category (using Category.Name)
            if (categories != null && categories.Any())
            {
                query = query.Where(p => categories.Contains(p.Category.Name));
            }
            
            // Filter by quantity range
            if (minQuantity.HasValue)
            {
                query = query.Where(p => p.Quantity >= minQuantity.Value);
            }
            
            if (maxQuantity.HasValue)
            {
                query = query.Where(p => p.Quantity <= maxQuantity.Value);
            }
            
            // Watch Specifications Filters
            if (materials != null && materials.Any())
            {
                query = query.Where(p => materials.Any(m => p.Description.Contains(m)));
            }
            
            if (crystals != null && crystals.Any())
            {
                query = query.Where(p => crystals.Any(c => p.Description.Contains(c)));
            }
            
            if (bezels != null && bezels.Any())
            {
                query = query.Where(p => bezels.Any(b => p.Description.Contains(b)));
            }
            
            if (dials != null && dials.Any())
            {
                query = query.Where(p => dials.Any(d => p.Description.Contains(d)));
            }
            
            if (waterResistance != null && waterResistance.Any())
            {
                query = query.Where(p => waterResistance.Any(w => p.Description.Contains(w)));
            }
            
            if (movements != null && movements.Any())
            {
                query = query.Where(p => movements.Any(m => p.Description.Contains(m)));
            }
            
            if (complications != null && complications.Any())
            {
                query = query.Where(p => complications.Any(c => p.Description.Contains(c)));
            }
            
            if (braceletMaterials != null && braceletMaterials.Any())
            {
                query = query.Where(p => braceletMaterials.Any(b => p.Description.Contains(b)));
            }
            
            if (braceletTypes != null && braceletTypes.Any())
            {
                query = query.Where(p => braceletTypes.Any(b => p.Description.Contains(b)));
            }
            
            if (clasps != null && clasps.Any())
            {
                query = query.Where(p => clasps.Any(c => p.Description.Contains(c)));
            }
            
            // Apply sorting
            IQueryable<ProductModel> sortedQuery;
            switch (sortBy)
            {
                case "name":
                    sortedQuery = query.OrderBy(p => p.Name);
                    break;
                case "price-low":
                    sortedQuery = query.OrderBy(p => p.Price);
                    break;
                case "price-high":
                    sortedQuery = query.OrderByDescending(p => p.Price);
                    break;
                case "newest":
                    sortedQuery = query.OrderByDescending(p => p.Id);
                    break;
                case "featured":
                default:
                    sortedQuery = query.OrderByDescending(p => p.Sold).ThenByDescending(p => p.Id);
                    break;
            }
            
            // Apply pagination
            var products = await sortedQuery
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToListAsync();
            
            // Get total count for pagination
            var totalCount = await sortedQuery.CountAsync();
            
            // Create paged list
            var pagedProducts = new StaticPagedList<ProductModel>(products, currentPage, itemsPerPage, totalCount);
            
            // Get filter data for sidebar
            var allProducts = await _dataContext.Products
                .Where(p => p.IsActive)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                // .Include(p => p.ProductColors).ThenInclude(pc => pc.Color) // Disabled - table removed
                .ToListAsync();
            
            // Debug: Check if data is loaded
            System.Diagnostics.Debug.WriteLine($"AllProducts loaded: {allProducts.Count}");
            // System.Diagnostics.Debug.WriteLine($"Products with ProductColors: {allProducts.Count(p => p.ProductColors != null)}"); // Disabled - table removed
            System.Diagnostics.Debug.WriteLine($"Products with CaseSize: {allProducts.Count(p => !string.IsNullOrEmpty(p.CaseSize))}");
            
            // Filter counts
            var modelCounts = allProducts
                .Where(p => !string.IsNullOrEmpty(p.Model)) // Chỉ lấy products có Model
                .GroupBy(p => p.Model)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name) // Sắp xếp theo tên thay vì số lượng
                .ToList(); // Bỏ .Take(10) để hiển thị tất cả models
            
            var brandCounts = allProducts
                .GroupBy(p => p.Brand?.Name ?? "Unknown")
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name) // Sắp xếp theo tên thay vì số lượng
                .ToList(); // Bỏ .Take(10) để hiển thị tất cả brands
            
            var priceRanges = new[]
            {
                new { Range = "Under $2,000", Min = 0m, Max = 2000m },
                new { Range = "$2,000 to $5,000", Min = 2000m, Max = 5000m },
                new { Range = "$5,000 to $10,000", Min = 5000m, Max = 10000m },
                new { Range = "Over $10,000", Min = 10000m, Max = decimal.MaxValue }
            };
            
            var priceCounts = priceRanges.Select(range => new
            {
                Range = range.Range,
                Count = allProducts.Count(p => p.Price >= range.Min && p.Price < range.Max)
            }).ToList();
            
            // Additional filter counts
                
            var colorCounts = allProducts
                .Where(p => !string.IsNullOrEmpty(p.DialColor))
                .GroupBy(p => p.DialColor)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();
            
            // Debug logging for colors
            System.Diagnostics.Debug.WriteLine($"Products with DialColor: {allProducts.Count(p => !string.IsNullOrEmpty(p.DialColor))}");
            System.Diagnostics.Debug.WriteLine($"Color counts: {colorCounts.Count}");
            foreach (var color in colorCounts)
            {
                System.Diagnostics.Debug.WriteLine($"Color: {color.Name}, Count: {color.Count}");
            }
                
            // Condition counts (from Description)
            var conditionCounts = allProducts
                .SelectMany(p => ExtractConditionsFromDescription(p.Description))
                .GroupBy(c => c)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();
                
            // Gender counts (from Description)
            var genderCounts = allProducts
                .SelectMany(p => ExtractGendersFromDescription(p.Description))
                .GroupBy(g => g)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();
                
            // Category counts (already have from Category.Name)
            var categoryCounts = allProducts
                .GroupBy(p => p.Category?.Name ?? "Unknown")
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name) // Sắp xếp theo tên thay vì số lượng
                .ToList(); // Bỏ .Take(10) để hiển thị tất cả categories
                
            // Watch Specifications Counts
            var materialCounts = allProducts
                .SelectMany(p => ExtractMaterialsFromDescription(p.Description))
                .GroupBy(m => m)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();
                
            var crystalCounts = allProducts
                .SelectMany(p => ExtractCrystalsFromDescription(p.Description))
                .GroupBy(c => c)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();
                
            var bezelCounts = allProducts
                .SelectMany(p => ExtractBezelsFromDescription(p.Description))
                .GroupBy(b => b)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();
                
            var dialCounts = allProducts
                .SelectMany(p => ExtractDialsFromDescription(p.Description))
                .GroupBy(d => d)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();
                
            var waterResistanceCounts = allProducts
                .SelectMany(p => ExtractWaterResistanceFromDescription(p.Description))
                .GroupBy(w => w)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();
                
            var movementCounts = allProducts
                .SelectMany(p => ExtractMovementsFromDescription(p.Description))
                .GroupBy(m => m)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();
                
            var complicationCounts = allProducts
                .SelectMany(p => ExtractComplicationsFromDescription(p.Description))
                .GroupBy(c => c)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();
                
            var braceletMaterialCounts = allProducts
                .SelectMany(p => ExtractBraceletMaterialsFromDescription(p.Description))
                .GroupBy(b => b)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();
            
            // Size counts from Products.CaseSize
            var sizeCounts = allProducts
                .Where(p => !string.IsNullOrEmpty(p.CaseSize))
                .GroupBy(p => p.CaseSize)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();
            
            // Debug logging
            System.Diagnostics.Debug.WriteLine($"Total products: {allProducts.Count}");
            System.Diagnostics.Debug.WriteLine($"Products with CaseSize: {allProducts.Count(p => !string.IsNullOrEmpty(p.CaseSize))}");
            System.Diagnostics.Debug.WriteLine($"Size counts: {sizeCounts.Count}");
            foreach (var size in sizeCounts)
            {
                System.Diagnostics.Debug.WriteLine($"Size: {size.Name}, Count: {size.Count}");
            }
            

            

                
            var braceletTypeCounts = allProducts
                .SelectMany(p => ExtractBraceletTypesFromDescription(p.Description))
                .GroupBy(b => b)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();
                
            var claspCounts = allProducts
                .SelectMany(p => ExtractClaspsFromDescription(p.Description))
                .GroupBy(c => c)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();
            
            // Set ViewBag for pagination controls and filters
            ViewBag.PageSize = itemsPerPage;
            ViewBag.SortBy = sortBy;
            
            // Filter data for sidebar
            ViewBag.ModelData = modelCounts;
            ViewBag.BrandData = brandCounts;
            ViewBag.CategoryData = categoryCounts;
            ViewBag.ConditionData = conditionCounts;
            ViewBag.GenderData = genderCounts;
            ViewBag.CaseMaterialData = materialCounts;
            ViewBag.DialColorData = colorCounts;
            ViewBag.HourMarkersData = crystalCounts;
            ViewBag.MovementTypeData = movementCounts;
            ViewBag.BraceletMaterialData = braceletMaterialCounts;
            ViewBag.SizeData = sizeCounts;
            
            // Selected filters
            ViewBag.SelectedModels = models ?? new string[0];
            ViewBag.SelectedBrands = brands ?? new string[0];
            ViewBag.SelectedCategories = categories ?? new string[0];
            ViewBag.SelectedConditions = conditions ?? new string[0];
            ViewBag.SelectedGenders = genders ?? new string[0];
            ViewBag.SelectedMaterials = materials ?? new string[0];
            ViewBag.SelectedColors = colors ?? new string[0];
            ViewBag.SelectedMarkers = crystals ?? new string[0];
            ViewBag.SelectedMovements = movements ?? new string[0];
            ViewBag.SelectedBracelets = braceletMaterials ?? new string[0];
            ViewBag.SelectedSizes = sizes ?? new string[0];
            
            // Price filters
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            
            return View(pagedProducts);
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Error in Product Index: {ex.Message}");
                
                // Return a simple error view or redirect to home
                TempData["error"] = "Có lỗi xảy ra khi tải trang sản phẩm. Vui lòng thử lại.";
                return RedirectToAction("Index", "Home");
            }
        }
        public async Task<IActionResult> Details(int Id)
        {
            System.Diagnostics.Debug.WriteLine($"ProductController.Details called with Id: {Id}");
            
            if (Id == 0) 
            {
                System.Diagnostics.Debug.WriteLine("Id is 0, redirecting to Index");
                return RedirectToAction("Index");
            }

            // Track user behavior - Temporarily disabled due to missing UserBehaviors table
            // var sessionId = HttpContext.Session.Id;
            // var currentUserId = User.Identity.IsAuthenticated ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;
            // var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var productById = await _dataContext.Products
                // .Include(p => p.ProductColors).ThenInclude(pc => pc.Color) // Disabled - table removed
                // .Include(p => p.ProductSizes).ThenInclude(ps => ps.Size) // Disabled - table removed
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Id == Id);

            // ProductImages are now properly linked to Products after running fix script
            // No need to clear ProductImages anymore

            // Debug: Log ProductImages data
            if (productById?.ProductImages != null)
            {
                Console.WriteLine($"Product {Id} has {productById.ProductImages.Count} ProductImages:");
                foreach (var img in productById.ProductImages)
                {
                    Console.WriteLine($"  - ID: {img.Id}, ImageName: {img.ImageName}, ImageUrl: {img.ImageUrl}, IsDefault: {img.IsDefault}");
                }
            }
            else
            {
                Console.WriteLine($"Product {Id} has no ProductImages");
            }

            // Comment out filtering to show all images
            // if (productById?.ProductImages != null && !string.IsNullOrEmpty(productById.Image))
            // {
            //     productById.ProductImages = productById.ProductImages
            //         .Where(pi => string.IsNullOrEmpty(pi.ImageUrl) || pi.ImageUrl != productById.Image)
            //         .ToList();
            // }

            if (productById == null) 
            {
                System.Diagnostics.Debug.WriteLine($"Product with Id {Id} not found in database");
                return NotFound();
            }
            
            System.Diagnostics.Debug.WriteLine($"Found product: {productById.Name} (Id: {productById.Id}, IsActive: {productById.IsActive})");

            var relatedProducts = await _dataContext.Products
                .Where(p => p.CategoryId == productById.CategoryId && p.Id != productById.Id)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Take(4).ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;

            var productReviews = await _dataContext.ProductReviews
                .Where(r => r.ProductId == Id)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            double averageRating = 0;
            if (productReviews.Any())
            {
                averageRating = productReviews.Average(r => r.Rating);
            }

            var viewModel = new ProductDetailsViewModel
            {
                ProductDetail = productById,
                ProductReviews = productReviews,
                AverageRating = averageRating
            };

            if (User.Identity.IsAuthenticated)
            {
                var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(authenticatedUserId);
                if (user != null)
                {
                    viewModel.Name = user.FullName;
                    viewModel.Email = user.Email;
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CommentProduct(ProductReviewModel review)
        {
            if (!ModelState.IsValid)
            {
                List<string> errors = new List<string>();
                foreach (var val in ModelState.Values)
                {
                    foreach (var error in val.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                
                string errorMessage = errors.Any() ? 
                    "Có lỗi trong biểu mẫu đánh giá: " + string.Join(", ", errors) :
                    "Có lỗi trong biểu mẫu đánh giá";
                    
                TempData["error"] = errorMessage;
                return RedirectToAction("Details", new { id = review.ProductId });
            }

            // Check for bad words in the comment
            if (_badWordsService.ContainsBadWords(review.Comment))
            {
                TempData["error"] = "Bình luận chứa từ ngữ không phù hợp. Vui lòng kiểm tra lại.";
                return RedirectToAction("Details", new { id = review.ProductId });
            }

            try
            {
                var existingRating = await _dataContext.ProductReviews
                    .FirstOrDefaultAsync(r => r.ProductId == review.ProductId &&
                                            r.UserName == review.UserName);

                if (existingRating != null)
                {
                    existingRating.Comment = review.Comment;
                    existingRating.Rating = review.Rating;
                    existingRating.CreatedDate = DateTime.Now;

                    _dataContext.ProductReviews.Update(existingRating);
                }
                else
                {
                    var ratingEntity = new ProductReviewModel
                    {
                        ProductId = review.ProductId,
                        UserName = review.UserName,
                        OrderCode = review.OrderCode,
                        Comment = review.Comment,
                        Rating = review.Rating,
                        CreatedDate = DateTime.Now
                    };

                    _dataContext.ProductReviews.Add(ratingEntity);
                }

                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Đánh giá đã được lưu thành công";
                return RedirectToAction("Details", new { id = review.ProductId });
            }
            catch (Exception ex)
            {
                TempData["error"] = "Có lỗi xảy ra khi lưu đánh giá: " + ex.Message;
                return RedirectToAction("Details", new { id = review.ProductId });
            }
        }

        // Action để so sánh sản phẩm từ JavaScript
        public async Task<IActionResult> Compare(string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                TempData["error"] = "Không có sản phẩm nào để so sánh";
                return RedirectToAction("Index");
            }

            try
            {
                var productIds = ids.Split(',').Select(int.Parse).ToList();
                
                if (productIds.Count < 2)
                {
                    TempData["error"] = "Cần ít nhất 2 sản phẩm để so sánh";
                    return RedirectToAction("Index");
                }

                if (productIds.Count > 4)
                {
                    TempData["error"] = "Chỉ có thể so sánh tối đa 4 sản phẩm";
                    return RedirectToAction("Index");
                }

                var products = await _dataContext.Products
                    .Where(p => productIds.Contains(p.Id))
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.ProductImages)
                    .ToListAsync();

                if (products.Count != productIds.Count)
                {
                    TempData["error"] = "Một số sản phẩm không tồn tại";
                    return RedirectToAction("Index");
                }

                // Sắp xếp theo thứ tự được chọn
                var orderedProducts = productIds
                    .Select(id => products.First(p => p.Id == id))
                    .ToList();

                return View(orderedProducts);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error in product comparison"); // Assuming _logger is available
                TempData["error"] = "Có lỗi xảy ra khi so sánh sản phẩm";
                return RedirectToAction("Index");
            }
        }



        public async Task<IActionResult> Search(string searchTerm, int? page, int? pageSize, string sortBy = "featured",
            string[] models = null, string[] brands = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            // Kiểm tra nếu searchTerm null hoặc rỗng
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                TempData["error"] = "Hãy nhập từ khóa";
                return RedirectToAction("Index","Home");
            }

            // Lưu lịch sử tìm kiếm ngay từ đầu
            await SaveSearchHistory(searchTerm);

            // Debug log
            System.Diagnostics.Debug.WriteLine($"Search term: {searchTerm}");

            // Kiểm tra xem searchTerm có phải là tên brand đơn giản không
            var isSimpleBrandSearch = await _dataContext.Brands
                .AnyAsync(b => b.Name.ToLower().Trim() == searchTerm.ToLower().Trim());

            // Kiểm tra xem searchTerm có phải là tên category đơn giản không
            var isSimpleCategorySearch = await _dataContext.Categories
                .AnyAsync(c => c.Name.ToLower().Trim() == searchTerm.ToLower().Trim());

            // Kiểm tra xem searchTerm có phải là tên model đơn giản không
            var isSimpleModelSearch = await _dataContext.Products
                .Where(p => !string.IsNullOrEmpty(p.Model))
                .AnyAsync(p => p.Model.ToLower().Trim() == searchTerm.ToLower().Trim());

            // Chỉ chuyển hướng nếu searchTerm là tên đơn giản
            if (isSimpleBrandSearch || isSimpleCategorySearch || isSimpleModelSearch)
            {
                System.Diagnostics.Debug.WriteLine($"Simple search term detected, will redirect if exact match found");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Complex search term detected, will show search results");
            }

            // Kiểm tra xem searchTerm có phải là tên brand không
            var matchingBrand = await _dataContext.Brands
                .FirstOrDefaultAsync(b => b.Name.ToLower() == searchTerm.ToLower());

            // Nếu không tìm thấy exact match, tìm partial match
            if (matchingBrand == null)
            {
                matchingBrand = await _dataContext.Brands
                    .FirstOrDefaultAsync(b => b.Name.ToLower().Contains(searchTerm.ToLower()));
            }

            System.Diagnostics.Debug.WriteLine($"Matching brand: {matchingBrand?.Name}");

            if (matchingBrand != null && isSimpleBrandSearch)
            {
                // Kiểm tra xem có bao nhiêu sản phẩm của brand này
                var brandProductCount = await _dataContext.Products
                    .CountAsync(p => p.BrandId == matchingBrand.Id && p.IsActive);

                // Nếu tìm thấy brand và có sản phẩm, chuyển hướng đến trang brand
                if (brandProductCount > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Redirecting to brand: {matchingBrand.Name} with {brandProductCount} products");
                    return RedirectToAction("Brand", new { brandId = matchingBrand.Id });
                }
            }

            // Kiểm tra xem searchTerm có phải là tên category không
            var matchingCategory = await _dataContext.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == searchTerm.ToLower());

            // Nếu không tìm thấy exact match, tìm partial match
            if (matchingCategory == null)
            {
                matchingCategory = await _dataContext.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower().Contains(searchTerm.ToLower()));
            }

            // Debug log
            System.Diagnostics.Debug.WriteLine($"Matching category: {matchingCategory?.Name}");

            if (matchingCategory != null && isSimpleCategorySearch)
            {
                // Kiểm tra xem có bao nhiêu sản phẩm của category này
                var categoryProductCount = await _dataContext.Products
                    .CountAsync(p => p.CategoryId == matchingCategory.Id && p.IsActive);

                // Nếu tìm thấy category và có sản phẩm, chuyển hướng đến trang product với filter category
                if (categoryProductCount > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Redirecting to category: {matchingCategory.Name} with {categoryProductCount} products");
                    return RedirectToAction("Index", new { categories = new[] { matchingCategory.Name } });
                }
            }

            // Kiểm tra xem searchTerm có phải là tên model không
            var matchingModel = await _dataContext.Products
                .Where(p => !string.IsNullOrEmpty(p.Model))
                .FirstOrDefaultAsync(p => p.Model.ToLower() == searchTerm.ToLower());

            // Nếu không tìm thấy exact match, tìm partial match
            if (matchingModel == null)
            {
                matchingModel = await _dataContext.Products
                    .Where(p => !string.IsNullOrEmpty(p.Model))
                    .FirstOrDefaultAsync(p => p.Model.ToLower().Contains(searchTerm.ToLower()));
            }

            // Debug log
            System.Diagnostics.Debug.WriteLine($"Matching model: {matchingModel?.Model}");

            if (matchingModel != null && isSimpleModelSearch)
            {
                // Kiểm tra xem có bao nhiêu sản phẩm của model này
                var modelProductCount = await _dataContext.Products
                    .CountAsync(p => p.Model == matchingModel.Model && p.IsActive);

                // Nếu tìm thấy model và có sản phẩm, chuyển hướng đến trang product với filter model
                if (modelProductCount > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Redirecting to model: {matchingModel.Model} with {modelProductCount} products");
                    return RedirectToAction("Index", new { models = new[] { matchingModel.Model } });
                }
            }

            // Tìm kiếm theo tên sản phẩm chính xác
            var searchQuery = _dataContext.Products
                .Where(p => p.IsActive)
                .Where(p => 
                    // Tìm kiếm chính xác trong tên sản phẩm (ưu tiên cao nhất)
                    p.Name.ToLower().Contains(searchTerm.ToLower()) ||
                    // Tìm kiếm trong mô tả sản phẩm
                    (p.Description != null && p.Description.ToLower().Contains(searchTerm.ToLower())) ||
                    // Tìm kiếm trong tên brand
                    p.Brand.Name.ToLower().Contains(searchTerm.ToLower()) ||
                    // Tìm kiếm trong tên category
                    p.Category.Name.ToLower().Contains(searchTerm.ToLower()) ||
                    // Tìm kiếm trong model
                    (p.Model != null && p.Model.ToLower().Contains(searchTerm.ToLower()))
                );

            // Đếm kết quả tìm kiếm
            var searchResultCount = await searchQuery.CountAsync();

            System.Diagnostics.Debug.WriteLine($"Search result count: {searchResultCount}");

            // Nếu kết quả tìm kiếm quá ít (dưới 3 sản phẩm), thử tìm kiếm rộng hơn
            if (searchResultCount < 3)
            {
                System.Diagnostics.Debug.WriteLine($"Few results found, trying broader search");
                
                // Tìm kiếm rộng hơn - tìm kiếm theo từng từ trong searchTerm
                var searchKeywords = searchTerm.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var broaderSearchQuery = _dataContext.Products
                    .Where(p => p.IsActive)
                    .Where(p => 
                        searchKeywords.Any(keyword => 
                            p.Name.ToLower().Contains(keyword) ||
                            (p.Description != null && p.Description.ToLower().Contains(keyword)) ||
                            p.Brand.Name.ToLower().Contains(keyword) ||
                            p.Category.Name.ToLower().Contains(keyword) ||
                            (p.Model != null && p.Model.ToLower().Contains(keyword))
                        )
                    );

                var broaderResultCount = await broaderSearchQuery.CountAsync();
                System.Diagnostics.Debug.WriteLine($"Broader search result count: {broaderResultCount}");

                if (broaderResultCount > searchResultCount)
                {
                    searchQuery = broaderSearchQuery;
                    searchResultCount = broaderResultCount;
                }
            }

            int currentPage = page ?? 1;
            int itemsPerPage = pageSize ?? 12;

            // Sử dụng logic tìm kiếm thông minh đã được cải thiện
            IQueryable<ProductModel> query = searchQuery
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages);
                // .Include(p => p.ProductColors).ThenInclude(pc => pc.Color) // Disabled - table removed
                // .Include(p => p.ProductSizes).ThenInclude(ps => ps.Size); // Disabled - table removed

            // Apply filters
            if (models != null && models.Any())
            {
                query = query.Where(p => models.Contains(p.Model));
            }
            
            if (brands != null && brands.Any())
            {
                query = query.Where(p => brands.Contains(p.Brand.Name));
            }
            
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }
            
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // Apply sorting with search relevance
            IQueryable<ProductModel> sortedQuery;
            switch (sortBy)
            {
                case "name":
                    sortedQuery = query.OrderBy(p => p.Name);
                    break;
                case "price-low":
                    sortedQuery = query.OrderBy(p => p.Price);
                    break;
                case "price-high":
                    sortedQuery = query.OrderByDescending(p => p.Price);
                    break;
                case "newest":
                    sortedQuery = query.OrderByDescending(p => p.Id);
                    break;
                case "featured":
                default:
                    // Sắp xếp theo độ chính xác tìm kiếm trước, sau đó theo số lượng bán
                    sortedQuery = query
                        .OrderByDescending(p => p.Name.ToLower().Contains(searchTerm.ToLower())) // Tên sản phẩm chứa searchTerm được ưu tiên
                        .ThenByDescending(p => p.Sold)
                        .ThenByDescending(p => p.Id);
                    break;
            }

            // Apply pagination
            var products = await sortedQuery
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToListAsync();

            // Get total count for pagination
            var totalCount = await sortedQuery.CountAsync();

            // Create paged list
            var pagedProducts = new StaticPagedList<ProductModel>(products, currentPage, itemsPerPage, totalCount);

            // Get filter data for sidebar using smart search
            var allSearchProducts = await searchQuery
                .Include(p => p.Category)
                .Include(p => p.Brand)
                // .Include(p => p.ProductColors).ThenInclude(pc => pc.Color) // Disabled - table removed
                // .Include(p => p.ProductSizes).ThenInclude(ps => ps.Size) // Disabled - table removed
                .ToListAsync();

            // Filter counts
            var modelCounts = allSearchProducts
                .Where(p => !string.IsNullOrEmpty(p.Model))
                .GroupBy(p => p.Model)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();

            var brandCounts = allSearchProducts
                .GroupBy(p => p.Brand?.Name ?? "Unknown")
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();

            var sizeCounts = allSearchProducts
                .Where(p => !string.IsNullOrEmpty(p.CaseSize))
                .GroupBy(p => p.CaseSize)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();

            var priceRanges = new[]
            {
                new { Range = "$2,000 to $5,000", Min = 2000m, Max = 5000m },
                new { Range = "$7,000 to $10,000", Min = 7000m, Max = 10000m },
                new { Range = "$10,000 to $15,000", Min = 10000m, Max = 15000m }
            };

            var priceCounts = priceRanges.Select(range => new
            {
                Range = range.Range,
                Count = allSearchProducts.Count(p => p.Price >= range.Min && p.Price < range.Max)
            }).ToList();

            // Set ViewBag for pagination controls and filters
            ViewBag.Keyword = searchTerm;
            ViewBag.PageSize = itemsPerPage;
            ViewBag.SortBy = sortBy;
            ViewBag.ModelCounts = modelCounts;
            ViewBag.BrandCounts = brandCounts;
            ViewBag.SizeCounts = sizeCounts;
            ViewBag.PriceCounts = priceCounts;
            ViewBag.SelectedModels = models ?? new string[0];
            ViewBag.SelectedBrands = brands ?? new string[0];
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View(pagedProducts);
        }
        [HttpGet]
        public async Task<IActionResult> GetImageByColor(int productId, int colorId)
        {
            var productImages = await _dataContext.ProductImages
                .Where(pi => pi.ProductId == productId && pi.ColorId == colorId)
                .ToListAsync();

            if (productImages != null && productImages.Any())
            {
                // Lấy ảnh đầu tiên của màu này
                string imageName = productImages.First().ImageName;
                return Json(new { success = true, imageName = imageName });
            }

            // Nếu không tìm thấy ảnh cho màu sắc này, trả về ảnh mặc định của sản phẩm
            var product = await _dataContext.Products.FindAsync(productId);
            if (product != null)
            {
                return Json(new { success = true, imageName = product.Image });
            }
            return Json(new { success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetPersonalizedRecommendations()
        {
            var sessionId = HttpContext.Session.Id;
            var currentUserId = User.Identity.IsAuthenticated ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;
            
            var recommendations = await _recommendationService.GetPersonalizedRecommendations(sessionId, currentUserId, 8);
            
            var result = recommendations.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                imageUrl = Url.Action("GetProductImage", "Home", new { productId = p.Id }),
                price = p.Price,
                category = p.Category?.Name ?? "",
                brand = p.Brand?.Name ?? ""
            }).ToList();
            
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentlyViewed()
        {
            var sessionId = HttpContext.Session.Id;
            var currentUserId = User.Identity.IsAuthenticated ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;
            
            var recentProducts = await _recommendationService.GetRecentlyViewedProducts(sessionId, currentUserId, 8);
            
            var result = recentProducts.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                imageUrl = Url.Action("GetProductImage", "Home", new { productId = p.Id }),
                price = p.Price,
                category = p.Category?.Name ?? "",
                brand = p.Brand?.Name ?? ""
            }).ToList();
            
            return Json(result);
        }

        public async Task<IActionResult> GetSearchSuggestions(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
                {
                    return Json(new { 
                        recentlySearched = new List<object>(),
                        topSuggestions = new List<object>(),
                        productSuggestions = new List<object>()
                    });
                }

            var userSessionId = HttpContext.Session.Id;
            var userCurrentId = User.Identity.IsAuthenticated ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;

            // Lấy lịch sử tìm kiếm gần đây
            var recentlySearched = await _dataContext.SearchHistories
                .Where(sh => (userCurrentId != null && sh.UserId == userCurrentId) ||
                            (userCurrentId == null && sh.SessionId == userSessionId))
                .OrderByDescending(sh => sh.SearchedAt)
                .Take(5)
                .Select(sh => new { term = sh.SearchTerm })
                .ToListAsync();

            // Lấy gợi ý dựa trên model sản phẩm
            var topSuggestions = new List<object>();
            
            // Nếu tìm kiếm theo brand, lấy các model của brand đó
            var brand = await _dataContext.Brands
                .FirstOrDefaultAsync(b => b.Name.ToLower().Contains(searchTerm.ToLower()));
            
            if (brand != null)
            {
                var brandModels = await _dataContext.Products
                    .Where(p => p.BrandId == brand.Id && p.IsActive && !string.IsNullOrEmpty(p.Model))
                    .Select(p => p.Model)
                    .Distinct()
                    .Take(10)
                    .ToListAsync();

                topSuggestions = brandModels.Select(m => new { term = $"{brand.Name} {m}" }).Cast<object>().ToList();
            }
            
            // Nếu không có brand models hoặc không tìm thấy brand, lấy các model phổ biến
            if (!topSuggestions.Any())
            {
                var popularModels = await _dataContext.Products
                    .Where(p => p.IsActive && !string.IsNullOrEmpty(p.Model) && 
                               (p.Name.ToLower().Contains(searchTerm.ToLower()) || 
                                p.Model.ToLower().Contains(searchTerm.ToLower()) ||
                                p.Brand.Name.ToLower().Contains(searchTerm.ToLower())))
                    .GroupBy(p => p.Model)
                    .OrderByDescending(g => g.Count())
                    .Take(10)
                    .Select(g => new { term = g.Key })
                    .ToListAsync();

                topSuggestions = popularModels.Cast<object>().ToList();
            }
            
            // Nếu vẫn không có, lấy tất cả models phổ biến
            if (!topSuggestions.Any())
            {
                var allPopularModels = await _dataContext.Products
                    .Where(p => p.IsActive && !string.IsNullOrEmpty(p.Model))
                    .GroupBy(p => p.Model)
                    .OrderByDescending(g => g.Count())
                    .Take(10)
                    .Select(g => new { term = g.Key })
                    .ToListAsync();

                topSuggestions = allPopularModels.Cast<object>().ToList();
            }
            
            // Nếu vẫn không có models, tạo một số suggestions mặc định
            if (!topSuggestions.Any())
            {
                topSuggestions = new List<object>
                {
                    new { term = "Rolex Submariner" },
                    new { term = "Omega Speedmaster" },
                    new { term = "Cartier Santos" },
                    new { term = "Patek Philippe" },
                    new { term = "Audemars Piguet" }
                };
            }

            // Get product suggestions based on search term
            var productSuggestions = await _dataContext.Products
                .Where(p => p.IsActive && (
                    p.Name.ToLower().Contains(searchTerm.ToLower()) ||
                    p.Brand.Name.ToLower().Contains(searchTerm.ToLower()) ||
                    p.Category.Name.ToLower().Contains(searchTerm.ToLower()) ||
                    (p.Model != null && p.Model.ToLower().Contains(searchTerm.ToLower()))
                ))
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .OrderByDescending(p => p.Name.ToLower().Contains(searchTerm.ToLower())) // Prioritize exact name matches
                .ThenByDescending(p => p.Sold) // Then by popularity
                .Take(8)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    imageUrl = Url.Action("GetProductImage", "Home", new { productId = p.Id }),
                    price = p.Price,
                    category = p.Category.Name,
                    brand = p.Brand.Name
                })
                .ToListAsync();
                
            System.Diagnostics.Debug.WriteLine($"Found {productSuggestions.Count} product suggestions for '{searchTerm}':");
            foreach (var product in productSuggestions)
            {
                System.Diagnostics.Debug.WriteLine($"  - ID: {product.id}, Name: {product.name}, Brand: {product.brand}");
            }

            // Get personalized recommendations if user has viewing history
            var personalizedProducts = await _recommendationService.GetPersonalizedRecommendations(userSessionId, userCurrentId, 4);
            
            var personalizedSuggestions = personalizedProducts
                .Where(p => !productSuggestions.Any(ps => ps.id == p.Id)) // Avoid duplicates
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    imageUrl = Url.Action("GetProductImage", "Home", new { productId = p.Id }),
                    price = p.Price,
                    category = p.Category?.Name ?? "",
                    brand = p.Brand?.Name ?? ""
                })
                .Take(4)
                .ToList();

            // Combine search results with personalized recommendations
            var allSuggestions = productSuggestions.Concat(personalizedSuggestions).Take(8).ToList();
            
            // Nếu vẫn không có sản phẩm nào, lấy một số sản phẩm phổ biến
            if (!allSuggestions.Any())
            {
                var popularProducts = await _dataContext.Products
                    .Where(p => p.IsActive)
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .OrderByDescending(p => p.Sold)
                    .Take(8)
                    .Select(p => new
                    {
                        id = p.Id,
                        name = p.Name,
                        imageUrl = Url.Action("GetProductImage", "Home", new { productId = p.Id }),
                        price = p.Price,
                        category = p.Category.Name,
                        brand = p.Brand.Name
                    })
                    .ToListAsync();
                
                allSuggestions = popularProducts;
            }
            


            return Json(new { 
                recentlySearched = recentlySearched,
                topSuggestions = topSuggestions,
                productSuggestions = allSuggestions
            });
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Error in GetSearchSuggestions: {ex.Message}");
                
                // Return empty results instead of throwing
                return Json(new { 
                    recentlySearched = new List<object>(),
                    topSuggestions = new List<object>(),
                    productSuggestions = new List<object>()
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClearSearchHistory()
        {
            try
            {
                var sessionId = HttpContext.Session.Id;
                var currentUserId = User.Identity.IsAuthenticated ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;

                var historiesToDelete = await _dataContext.SearchHistories
                    .Where(sh => (currentUserId != null && sh.UserId == currentUserId) ||
                                (currentUserId == null && sh.SessionId == sessionId))
                    .ToListAsync();

                _dataContext.SearchHistories.RemoveRange(historiesToDelete);
                await _dataContext.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveSearchHistoryItem(string searchTerm)
        {
            try
            {
                var sessionId = HttpContext.Session.Id;
                var currentUserId = User.Identity.IsAuthenticated ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;

                var historyToDelete = await _dataContext.SearchHistories
                    .FirstOrDefaultAsync(sh => 
                        sh.SearchTerm.ToLower() == searchTerm.ToLower() &&
                        ((currentUserId != null && sh.UserId == currentUserId) ||
                         (currentUserId == null && sh.SessionId == sessionId)));

                if (historyToDelete != null)
                {
                    _dataContext.SearchHistories.Remove(historyToDelete);
                    await _dataContext.SaveChangesAsync();
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> Brand(int brandId, int? page, int? pageSize, string sortBy = "featured",
            [FromQuery] string[] models = null, [FromQuery] string[] materials = null, [FromQuery] string[] genders = null, [FromQuery] string[] conditions = null,
            [FromQuery] string[] colors = null, [FromQuery] string[] markers = null, [FromQuery] string[] movements = null, [FromQuery] string[] bracelets = null,
            [FromQuery] string[] sizes = null, [FromQuery] decimal? minPrice = null, [FromQuery] decimal? maxPrice = null)
        {
            // Debug logging
            System.Diagnostics.Debug.WriteLine($"Brand method called with brandId: {brandId}");
            System.Diagnostics.Debug.WriteLine($"Models: {models?.Length ?? 0} items");
            if (models != null && models.Length > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Models: {string.Join(", ", models)}");
            }
            System.Diagnostics.Debug.WriteLine($"Materials: {materials?.Length ?? 0} items");
            System.Diagnostics.Debug.WriteLine($"Genders: {genders?.Length ?? 0} items");
            System.Diagnostics.Debug.WriteLine($"Conditions: {conditions?.Length ?? 0} items");
            System.Diagnostics.Debug.WriteLine($"Colors: {colors?.Length ?? 0} items");
            System.Diagnostics.Debug.WriteLine($"Markers: {markers?.Length ?? 0} items");
            System.Diagnostics.Debug.WriteLine($"Movements: {movements?.Length ?? 0} items");
            System.Diagnostics.Debug.WriteLine($"Bracelets: {bracelets?.Length ?? 0} items");
            System.Diagnostics.Debug.WriteLine($"Sizes: {sizes?.Length ?? 0} items");
            if (sizes != null && sizes.Length > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Sizes: {string.Join(", ", sizes)}");
            }
            System.Diagnostics.Debug.WriteLine($"MinPrice: {minPrice}, MaxPrice: {maxPrice}");
            int currentPage = page ?? 1;
            int itemsPerPage = pageSize ?? 12;
            
            var brand = await _dataContext.Brands.FindAsync(brandId);
            if (brand == null)
            {
                return NotFound();
            }
            
            IQueryable<ProductModel> query = _dataContext.Products
                .Where(p => p.IsActive && p.BrandId == brandId)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages);
            
            // Apply filters
            if (models != null && models.Length > 0)
            {
                query = query.Where(p => models.Contains(p.Model));
                System.Diagnostics.Debug.WriteLine($"Filtering by models: {string.Join(", ", models)}");
            }
            
            if (materials != null && materials.Length > 0)
            {
                query = query.Where(p => materials.Contains(p.CaseMaterial));
                System.Diagnostics.Debug.WriteLine($"Filtering by materials: {string.Join(", ", materials)}");
            }
            
            if (genders != null && genders.Length > 0)
            {
                query = query.Where(p => genders.Contains(p.Gender));
                System.Diagnostics.Debug.WriteLine($"Filtering by genders: {string.Join(", ", genders)}");
            }
            
            if (conditions != null && conditions.Length > 0)
            {
                query = query.Where(p => conditions.Contains(p.Condition));
                System.Diagnostics.Debug.WriteLine($"Filtering by conditions: {string.Join(", ", conditions)}");
            }
            
            if (colors != null && colors.Length > 0)
            {
                query = query.Where(p => colors.Contains(p.DialColor));
                System.Diagnostics.Debug.WriteLine($"Filtering by colors: {string.Join(", ", colors)}");
            }
            
            if (markers != null && markers.Length > 0)
            {
                query = query.Where(p => markers.Contains(p.HourMarkers));
                System.Diagnostics.Debug.WriteLine($"Filtering by markers: {string.Join(", ", markers)}");
            }
            
            if (movements != null && movements.Length > 0)
            {
                query = query.Where(p => movements.Contains(p.MovementType));
                System.Diagnostics.Debug.WriteLine($"Filtering by movements: {string.Join(", ", movements)}");
            }
            
            if (bracelets != null && bracelets.Length > 0)
            {
                query = query.Where(p => bracelets.Contains(p.BraceletMaterial));
                System.Diagnostics.Debug.WriteLine($"Filtering by bracelets: {string.Join(", ", bracelets)}");
            }
            
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
                System.Diagnostics.Debug.WriteLine($"Filtering by minPrice: {minPrice.Value}");
            }
            
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
                System.Diagnostics.Debug.WriteLine($"Filtering by maxPrice: {maxPrice.Value}");
            }
            
            IQueryable<ProductModel> sortedQuery;
            switch (sortBy)
            {
                case "name":
                    sortedQuery = query.OrderBy(p => p.Name);
                    break;
                case "price-low":
                    sortedQuery = query.OrderBy(p => p.Price);
                    break;
                case "price-high":
                    sortedQuery = query.OrderByDescending(p => p.Price);
                    break;
                case "newest":
                    sortedQuery = query.OrderByDescending(p => p.Id);
                    break;
                case "featured":
                default:
                    sortedQuery = query.OrderByDescending(p => p.Sold).ThenByDescending(p => p.Id);
                    break;
            }
            
            var products = await sortedQuery
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToListAsync();
            
            var totalCount = await sortedQuery.CountAsync();
            
            // Debug logging for results
            System.Diagnostics.Debug.WriteLine($"Total products after filtering: {totalCount}");
            System.Diagnostics.Debug.WriteLine($"Products on current page: {products.Count}");
            
            var pagedProducts = new StaticPagedList<ProductModel>(products, currentPage, itemsPerPage, totalCount);
            
            // Get filter data from database - use filtered products for accurate counts
            var allBrandProducts = await _dataContext.Products
                .Where(p => p.IsActive && p.BrandId == brandId)
                .ToListAsync();
            
            // Create a copy of the filtered query for calculating filter counts
            var filteredProducts = allBrandProducts.AsQueryable();
            
            // Apply the same filters to get accurate counts
            if (models != null && models.Length > 0)
            {
                filteredProducts = filteredProducts.Where(p => models.Contains(p.Model));
            }
            if (materials != null && materials.Length > 0)
            {
                filteredProducts = filteredProducts.Where(p => materials.Contains(p.CaseMaterial));
            }
            if (genders != null && genders.Length > 0)
            {
                filteredProducts = filteredProducts.Where(p => genders.Contains(p.Gender));
            }
            if (conditions != null && conditions.Length > 0)
            {
                filteredProducts = filteredProducts.Where(p => conditions.Contains(p.Condition));
            }
            if (colors != null && colors.Length > 0)
            {
                filteredProducts = filteredProducts.Where(p => colors.Contains(p.DialColor));
            }
            if (markers != null && markers.Length > 0)
            {
                filteredProducts = filteredProducts.Where(p => markers.Contains(p.HourMarkers));
            }
            if (movements != null && movements.Length > 0)
            {
                filteredProducts = filteredProducts.Where(p => movements.Contains(p.MovementType));
            }
            if (bracelets != null && bracelets.Length > 0)
            {
                filteredProducts = filteredProducts.Where(p => bracelets.Contains(p.BraceletMaterial));
            }
            if (sizes != null && sizes.Length > 0)
            {
                filteredProducts = filteredProducts.Where(p => sizes.Contains(p.CaseSize));
            }
            if (minPrice.HasValue)
            {
                filteredProducts = filteredProducts.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                filteredProducts = filteredProducts.Where(p => p.Price <= maxPrice.Value);
            }
            
            var filteredProductsList = filteredProducts.ToList();
            
            // Model filter data - use filtered products for accurate counts
            var modelData = filteredProductsList
                .Where(p => !string.IsNullOrEmpty(p.Model))
                .GroupBy(p => p.Model)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();
            
            // Category filter data - use filtered products for accurate counts
            var categoryData = filteredProductsList
                .Where(p => p.Category != null)
                .GroupBy(p => p.Category.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();
            
            // Gender filter data - use filtered products for accurate counts
            var genderData = filteredProductsList
                .Where(p => !string.IsNullOrEmpty(p.Gender))
                .GroupBy(p => p.Gender)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();
            
            // Condition filter data - use filtered products for accurate counts
            var conditionData = filteredProductsList
                .Where(p => !string.IsNullOrEmpty(p.Condition))
                .GroupBy(p => p.Condition)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();
            
            // Case Material filter data - use filtered products for accurate counts
            var caseMaterialData = filteredProductsList
                .Where(p => !string.IsNullOrEmpty(p.CaseMaterial))
                .GroupBy(p => p.CaseMaterial)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();
            
            // Dial Color filter data - use filtered products for accurate counts
            var dialColorData = filteredProductsList
                .Where(p => !string.IsNullOrEmpty(p.DialColor))
                .GroupBy(p => p.DialColor)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();
            
            // Hour Markers filter data - use filtered products for accurate counts
            var hourMarkersData = filteredProductsList
                .Where(p => !string.IsNullOrEmpty(p.HourMarkers))
                .GroupBy(p => p.HourMarkers)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();
            
            // Movement Type filter data - use filtered products for accurate counts
            var movementTypeData = filteredProductsList
                .Where(p => !string.IsNullOrEmpty(p.MovementType))
                .GroupBy(p => p.MovementType)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();
            
            // Bracelet Material filter data - use filtered products for accurate counts
            var braceletMaterialData = filteredProductsList
                .Where(p => !string.IsNullOrEmpty(p.BraceletMaterial))
                .GroupBy(p => p.BraceletMaterial)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();
            
            // Size filter data - use filtered products for accurate counts
            var sizeData = filteredProductsList
                .Where(p => !string.IsNullOrEmpty(p.CaseSize))
                .GroupBy(p => p.CaseSize)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToList();
            
            // Price ranges
            var minPriceRange = allBrandProducts.Min(p => p.Price);
            var maxPriceRange = allBrandProducts.Max(p => p.Price);
            
            ViewBag.Brand = brand;
            ViewBag.PageSize = itemsPerPage;
            ViewBag.SortBy = sortBy;
            ViewBag.TotalCount = await _dataContext.Products.CountAsync(p => p.IsActive && p.BrandId == brandId);
            
            // Filter data
            ViewBag.ModelData = modelData;
            ViewBag.CategoryData = categoryData;
            ViewBag.GenderData = genderData;
            ViewBag.ConditionData = conditionData;
            ViewBag.CaseMaterialData = caseMaterialData;
            ViewBag.DialColorData = dialColorData;
            ViewBag.SelectedColors = colors ?? new string[0];
            ViewBag.HourMarkersData = hourMarkersData;
            ViewBag.MovementTypeData = movementTypeData;
            ViewBag.BraceletMaterialData = braceletMaterialData;
            ViewBag.SizeData = sizeData;
            ViewBag.SelectedSizes = sizes ?? new string[0];
            ViewBag.MinPrice = minPriceRange;
            ViewBag.MaxPrice = maxPriceRange;
            
            return View("Brand", pagedProducts);
        }
        
        // Helper methods to extract information from Description
        private List<string> ExtractConditionsFromDescription(string description)
        {
            var conditions = new List<string>();
            if (string.IsNullOrEmpty(description)) return conditions;
            
            var conditionKeywords = new[] { "Excellent", "Good", "Fair", "Poor", "Mint", "Like New", "Used", "Pre-owned" };
            foreach (var keyword in conditionKeywords)
            {
                if (description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    conditions.Add(keyword);
                }
            }
            return conditions;
        }
        
        private List<string> ExtractGendersFromDescription(string description)
        {
            var genders = new List<string>();
            if (string.IsNullOrEmpty(description)) return genders;
            
            var genderKeywords = new[] { "Men", "Women", "Unisex", "Ladies", "Gentlemen" };
            foreach (var keyword in genderKeywords)
            {
                if (description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    genders.Add(keyword);
                }
            }
            return genders;
        }
        
        // Watch Specifications Helper Methods
        private List<string> ExtractMaterialsFromDescription(string description)
        {
            var materials = new List<string>();
            if (string.IsNullOrEmpty(description)) return materials;
            
            var materialKeywords = new[] { "Stainless Steel", "Yellow Gold", "White Gold", "Rose Gold", "Platinum", "Titanium", "Ceramic", "Premium Metal", "Steel and Gold" };
            foreach (var keyword in materialKeywords)
            {
                if (description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    materials.Add(keyword);
                }
            }
            return materials;
        }
        
        private List<string> ExtractCrystalsFromDescription(string description)
        {
            var crystals = new List<string>();
            if (string.IsNullOrEmpty(description)) return crystals;
            
            var crystalKeywords = new[] { "Sapphire", "Mineral", "Acrylic", "Scratch-resistant Sapphire", "Anti-reflective" };
            foreach (var keyword in crystalKeywords)
            {
                if (description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    crystals.Add(keyword);
                }
            }
            return crystals;
        }
        
        private List<string> ExtractBezelsFromDescription(string description)
        {
            var bezels = new List<string>();
            if (string.IsNullOrEmpty(description)) return bezels;
            
            var bezelKeywords = new[] { "Fixed Bezel", "Rotating Bezel", "Unidirectional", "Bidirectional", "Tachymeter", "GMT Bezel" };
            foreach (var keyword in bezelKeywords)
            {
                if (description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    bezels.Add(keyword);
                }
            }
            return bezels;
        }
        
        private List<string> ExtractDialsFromDescription(string description)
        {
            var dials = new List<string>();
            if (string.IsNullOrEmpty(description)) return dials;
            
            var dialKeywords = new[] { "Blue Dial", "Black Dial", "White Dial", "Silver Dial", "Green Dial", "Classic Dial", "Chronograph Dial" };
            foreach (var keyword in dialKeywords)
            {
                if (description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    dials.Add(keyword);
                }
            }
            return dials;
        }
        
        private List<string> ExtractWaterResistanceFromDescription(string description)
        {
            var waterResistance = new List<string>();
            if (string.IsNullOrEmpty(description)) return waterResistance;
            
            var waterResistanceKeywords = new[] { "30M", "50M", "100M", "200M", "300M", "500M", "1000M", "30 meters", "50 meters", "100 meters", "200 meters", "300 meters" };
            foreach (var keyword in waterResistanceKeywords)
            {
                if (description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    waterResistance.Add(keyword);
                }
            }
            return waterResistance;
        }
        
        private List<string> ExtractMovementsFromDescription(string description)
        {
            var movements = new List<string>();
            if (string.IsNullOrEmpty(description)) return movements;
            
            var movementKeywords = new[] { "Automatic", "Manual", "Quartz", "B01", "B20", "3135", "3235", "4130", "116500" };
            foreach (var keyword in movementKeywords)
            {
                if (description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    movements.Add(keyword);
                }
            }
            return movements;
        }
        
        private List<string> ExtractComplicationsFromDescription(string description)
        {
            var complications = new List<string>();
            if (string.IsNullOrEmpty(description)) return complications;
            
            var complicationKeywords = new[] { "Time Only", "Chronograph", "Date", "Day-Date", "GMT", "Moon Phase", "Perpetual Calendar" };
            foreach (var keyword in complicationKeywords)
            {
                if (description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    complications.Add(keyword);
                }
            }
            return complications;
        }
        
        private List<string> ExtractBraceletMaterialsFromDescription(string description)
        {
            var braceletMaterials = new List<string>();
            if (string.IsNullOrEmpty(description)) return braceletMaterials;
            
            var braceletMaterialKeywords = new[] { "Stainless Steel", "Yellow Gold", "White Gold", "Rose Gold", "Leather", "Rubber", "NATO", "Mesh" };
            foreach (var keyword in braceletMaterialKeywords)
            {
                if (description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    braceletMaterials.Add(keyword);
                }
            }
            return braceletMaterials;
        }
        
        private List<string> ExtractBraceletTypesFromDescription(string description)
        {
            var braceletTypes = new List<string>();
            if (string.IsNullOrEmpty(description)) return braceletTypes;
            
            var braceletTypeKeywords = new[] { "Metal Bracelet", "Leather Strap", "Rubber Strap", "NATO Strap", "Mesh Bracelet", "Oyster", "Jubilee", "President" };
            foreach (var keyword in braceletTypeKeywords)
            {
                if (description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    braceletTypes.Add(keyword);
                }
            }
            return braceletTypes;
        }
        
        private List<string> ExtractClaspsFromDescription(string description)
        {
            var clasps = new List<string>();
            if (string.IsNullOrEmpty(description)) return clasps;
            
            var claspKeywords = new[] { "Folding Clasp", "Deployant Clasp", "Tang Buckle", "Butterfly Clasp", "Oysterclasp", "Crownclasp" };
            foreach (var keyword in claspKeywords)
            {
                if (description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    clasps.Add(keyword);
                }
            }
            return clasps;
        }

        [HttpGet]
        public async Task<IActionResult> CheckDatabaseImages()
        {
            var products = await _dataContext.Products.ToListAsync();
            var imageList = products.Select(p => new { p.Id, p.Name, p.Image }).ToList();
            
            ViewBag.Products = imageList;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CheckDuplicateProducts()
        {
            // Find duplicate products based on Name and Brand
            var duplicates = await _dataContext.Products
                .GroupBy(p => new { p.Name, BrandName = p.Brand.Name })
                .Where(g => g.Count() > 1)
                .Select(g => new
                {
                    Name = g.Key.Name,
                    BrandName = g.Key.BrandName,
                    Count = g.Count(),
                    Products = g.Select(p => new { p.Id, p.Name, BrandName = p.Brand.Name, p.Price, p.Image }).ToList()
                })
                .ToListAsync();

            ViewBag.Duplicates = duplicates;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveDuplicateProducts()
        {
            try
            {
                // Find duplicate products based on Name and Brand
                var duplicateGroups = await _dataContext.Products
                    .GroupBy(p => new { p.Name, BrandName = p.Brand.Name })
                    .Where(g => g.Count() > 1)
                    .ToListAsync();

                int removedCount = 0;

                foreach (var group in duplicateGroups)
                {
                    // Keep the first product and remove the rest
                    var productsToRemove = group.Skip(1).ToList();
                    
                    foreach (var product in productsToRemove)
                    {
                        _dataContext.Products.Remove(product);
                        removedCount++;
                    }
                }

                if (removedCount > 0)
                {
                    await _dataContext.SaveChangesAsync();
                    TempData["success"] = $"Removed {removedCount} duplicate products";
                }
                else
                {
                    TempData["info"] = "No duplicate products found";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error removing duplicate products: {ex.Message}";
            }

            return RedirectToAction("CheckDuplicateProducts");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProductImages()
        {
            try
            {
                // Get all products that don't have images
                var productsWithoutImages = await _dataContext.Products
                    .Where(p => string.IsNullOrEmpty(p.Image) || p.Image == "noimage.jpg")
                    .ToListAsync();

                // Get all available images from the products table
                var availableImages = await _dataContext.Products
                    .Where(p => !string.IsNullOrEmpty(p.Image) && p.Image != "noimage.jpg")
                    .Select(p => p.Image)
                    .Distinct()
                    .ToListAsync();

                if (availableImages.Any() && productsWithoutImages.Any())
                {
                    var random = new Random();
                    int updatedCount = 0;
                    
                    // Update products one by one to avoid OUTPUT clause issues
                    foreach (var product in productsWithoutImages)
                    {
                        // Assign a random available image
                        var randomImage = availableImages[random.Next(availableImages.Count)];
                        product.Image = randomImage;
                        
                        // Save changes for each product individually
                        _dataContext.Products.Update(product);
                        await _dataContext.SaveChangesAsync();
                        updatedCount++;
                    }

                    TempData["success"] = $"Updated {updatedCount} products with images";
                }
                else
                {
                    TempData["info"] = "No products to update or no images available";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error updating product images: {ex.Message}";
            }

            return RedirectToAction("CheckDatabaseImages");
        }

        [HttpPost]
        public async Task<IActionResult> FixProductImages()
        {
            try
            {
                // Get all products that need image updates
                var productsToUpdate = await _dataContext.Products
                    .Where(p => string.IsNullOrEmpty(p.Image) || p.Image == "noimage.jpg")
                    .ToListAsync();
                
                // Get all available images from the media folder
                var mediaPath = Path.Combine(_webHostEnvironment.WebRootPath, "media", "products");
                var availableImages = new List<string>();
                
                if (Directory.Exists(mediaPath))
                {
                    availableImages = Directory.GetFiles(mediaPath)
                        .Select(Path.GetFileName)
                        .Where(f => !string.IsNullOrEmpty(f))
                        .ToList();
                }

                if (availableImages.Any() && productsToUpdate.Any())
                {
                    var random = new Random();
                    int updatedCount = 0;

                    // Update products one by one to avoid OUTPUT clause issues
                    foreach (var product in productsToUpdate)
                    {
                        var randomImage = availableImages[random.Next(availableImages.Count)];
                        product.Image = randomImage;
                        
                        // Save changes for each product individually
                        _dataContext.Products.Update(product);
                        await _dataContext.SaveChangesAsync();
                        updatedCount++;
                    }

                    TempData["success"] = $"Updated {updatedCount} products with images from media folder";
                }
                else if (!availableImages.Any())
                {
                    TempData["error"] = "No images found in media/products folder";
                }
                else
                {
                    TempData["info"] = "All products already have images";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error updating product images: {ex.Message}";
            }

            return RedirectToAction("Search", new { searchTerm = "Rolex" });
        }

        private async Task SaveSearchHistory(string searchTerm)
        {
            try
            {
                var sessionId = HttpContext.Session.Id;
                var currentUserId = User.Identity.IsAuthenticated ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;

                System.Diagnostics.Debug.WriteLine($"Saving search history: '{searchTerm}', SessionId: {sessionId}, UserId: {currentUserId}");

                // Kiểm tra xem đã có lịch sử tìm kiếm này chưa
                var existingHistory = await _dataContext.SearchHistories
                    .FirstOrDefaultAsync(sh => 
                        sh.SearchTerm.ToLower() == searchTerm.ToLower() &&
                        ((currentUserId != null && sh.UserId == currentUserId) ||
                         (currentUserId == null && sh.SessionId == sessionId)));

                if (existingHistory != null)
                {
                    // Cập nhật thời gian tìm kiếm và tăng số lần tìm kiếm
                    existingHistory.SearchedAt = DateTime.Now;
                    existingHistory.SearchCount++;
                    _dataContext.SearchHistories.Update(existingHistory);
                    System.Diagnostics.Debug.WriteLine($"Updated existing search history for: '{searchTerm}'");
                }
                else
                {
                    // Tạo lịch sử tìm kiếm mới
                    var newHistory = new SearchHistoryModel
                    {
                        SearchTerm = searchTerm,
                        SessionId = currentUserId == null ? sessionId : null,
                        UserId = currentUserId,
                        SearchedAt = DateTime.Now,
                        SearchCount = 1
                    };
                    _dataContext.SearchHistories.Add(newHistory);
                    System.Diagnostics.Debug.WriteLine($"Created new search history for: '{searchTerm}'");
                }

                await _dataContext.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"Successfully saved search history for: '{searchTerm}'");
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không làm gián đoạn chức năng tìm kiếm
                System.Diagnostics.Debug.WriteLine($"Error saving search history: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
