using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;

namespace Shopping_Demo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager,Staff,Sale")]
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string search = "")
        {
            IQueryable<ProductModel> query = _dataContext.Products
                // .Include(p => p.ProductColors).ThenInclude(pc => pc.Color) // Disabled - table removed
                // .Include(p => p.ProductSizes).ThenInclude(ps => ps.Size) // Disabled - table removed
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.Brand);

            // Apply search filter if search term is provided
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(search) ||
                    p.Model.ToLower().Contains(search) ||
                    p.Brand.Name.ToLower().Contains(search) ||
                    p.Category.Name.ToLower().Contains(search) ||
                    p.Condition.ToLower().Contains(search) ||
                    p.Gender.ToLower().Contains(search) ||
                    p.CaseSize.ToLower().Contains(search) ||
                    p.CaseMaterial.ToLower().Contains(search) ||
                    p.DialColor.ToLower().Contains(search) ||
                    p.MovementType.ToLower().Contains(search) ||
                    p.BraceletMaterial.ToLower().Contains(search) ||
                    p.Description.ToLower().Contains(search)
                );
            }

            query = query.OrderByDescending(p => p.Id);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            
            // Ensure page is within valid range
            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));
            
            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < totalPages;
            ViewBag.SearchTerm = search;

            return View(products);
        }
        public async Task<IActionResult> ToggleActive(int Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);

            if (product == null)
            {
                return NotFound();
            }

            product.IsActive = !product.IsActive;

            await _dataContext.SaveChangesAsync();

            string statusMessage = product.IsActive ? "kích hoạt" : "ẩn";
            TempData["success"] = $"Sản phẩm đã được {statusMessage}";

            return RedirectToAction("Index");
        }
        public IActionResult Create()
        {
            // Chỉ lấy danh mục phụ (level 2)
            ViewBag.Categories = new SelectList(_dataContext.Categories.Where(c => c.Level == 2), "Id", "Name");
            ViewBag.MainCategories = new SelectList(_dataContext.Categories.Where(c => c.Level == 1), "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
            
            // Add Colors and Sizes for the view
            ViewBag.Colors = new SelectList(new List<object>(), "Id", "Name"); // Empty list to prevent NullReference

            // Watch-specific dropdowns
            ViewBag.GenderOptions = new SelectList(new[] { "Nam", "Nữ", "Unisex" });
            ViewBag.ConditionOptions = new SelectList(new[] { "Mới", "Đã sử dụng", "Vintage", "Limited Edition" });
            ViewBag.MovementTypeOptions = new SelectList(new[] { "Automatic", "Manual", "Quartz", "Chronograph" });
            ViewBag.CaseMaterialOptions = new SelectList(new[] { "Stainless Steel", "Gold", "Platinum", "Titanium", "Ceramic" });
            ViewBag.BraceletMaterialOptions = new SelectList(new[] { "Stainless Steel", "Leather", "Gold", "Rubber", "NATO" });
            
            return View();
        }

        [HttpGet]
        public JsonResult GetSubCategories(int mainCategoryId)
        {
            var subCategories = _dataContext.Categories
                .Where(c => c.ParentId == mainCategoryId)
                .Select(c => new { Id = c.Id, Name = c.Name })
                .ToList();

            return Json(subCategories);
        }

        [HttpGet]
        public JsonResult GetCategoriesByBrand(int brandId)
        {
            // Tìm các sản phẩm thuộc brand này và lấy categories được sử dụng
            var categories = _dataContext.Products
                .Where(p => p.BrandId == brandId)
                .Select(p => p.Category)
                .Where(c => c != null)
                .Distinct()
                .Select(c => new { Id = c.Id, Name = c.Name })
                .ToList();

            // Nếu không có category nào được sử dụng, trả về tất cả categories
            if (!categories.Any())
            {
                categories = _dataContext.Categories
                    .Select(c => new { Id = c.Id, Name = c.Name })
                    .ToList();
            }

            return Json(categories);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel product, List<IFormFile> AdditionalImages, int DefaultImageIndex = 0)
        {
            // Load dropdown data
            LoadDropdownData(product.BrandId, product);

            if (ModelState.IsValid)
            {
                product.Slug = product.Name.Replace(" ", "-");
                var slug = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Sản phẩm đã tồn tại");
                    return View(product);
                }

                // Kiểm tra xem danh mục đã chọn có phải là danh mục phụ không
                var selectedCategory = await _dataContext.Categories.FindAsync(product.CategoryId);
                if (selectedCategory == null || selectedCategory.Level != 2)
                {
                    ModelState.AddModelError("CategoryId", "Vui lòng chọn một danh mục phụ hợp lệ");
                    return View(product);
                }

                // Xử lý hình ảnh sản phẩm
                if (AdditionalImages != null && AdditionalImages.Count > 0)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");

                    // Đảm bảo thư mục tồn tại
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    // Xác định ảnh chính theo chỉ số đã chọn
                    int mainImageIndex = DefaultImageIndex;
                    if (mainImageIndex >= AdditionalImages.Count)
                    {
                        mainImageIndex = 0; // Nếu chỉ số không hợp lệ, mặc định là ảnh đầu tiên
                    }

                    // Lưu ảnh chính vào trường Image của sản phẩm
                    var mainImage = AdditionalImages[mainImageIndex];
                    string mainImageName = Guid.NewGuid().ToString() + "_" + mainImage.FileName;
                    string mainFilePath = Path.Combine(uploadDir, mainImageName);
                    using (FileStream fs = new FileStream(mainFilePath, FileMode.Create))
                    {
                        await mainImage.CopyToAsync(fs);
                    }
                    product.Image = mainImageName;
                }
                else
                {
                    // Nếu không có ảnh, đặt ảnh mặc định
                    product.Image = "noimage.jpg";
                }

                // Lưu sản phẩm vào database
                _dataContext.Add(product);
                await _dataContext.SaveChangesAsync();

                // Xử lý lưu các ảnh vào bảng ProductImages
                if (AdditionalImages != null && AdditionalImages.Count > 0)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");

                    for (int i = 0; i < AdditionalImages.Count; i++)
                    {
                        var imageFile = AdditionalImages[i];
                        string imageName;

                        // Nếu là ảnh chính thì đã lưu rồi, dùng lại tên
                        if (i == DefaultImageIndex)
                        {
                            imageName = product.Image;
                        }
                        else
                        {
                            // Lưu các ảnh bổ sung
                            imageName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                            string filePath = Path.Combine(uploadDir, imageName);
                            using (FileStream fs = new FileStream(filePath, FileMode.Create))
                            {
                                await imageFile.CopyToAsync(fs);
                            }
                        }

                        // Tạo bản ghi ProductImage
                        ProductImageModel productImage = new ProductImageModel
                        {
                            ProductId = product.Id,
                            ImageName = imageName,
                            IsDefault = (i == DefaultImageIndex),
                            // Lấy màu liên kết với ảnh nếu có
                            ColorId = (product.ImageColors != null && i < product.ImageColors.Count) ? product.ImageColors[i] : null
                        };
                        _dataContext.ProductImages.Add(productImage);
                    }
                }
                else
                {
                    // Tạo bản ghi ProductImage cho ảnh mặc định
                    ProductImageModel defaultImage = new ProductImageModel
                    {
                        ProductId = product.Id,
                        ImageName = "noimage.jpg",
                        IsDefault = true
                    };
                    _dataContext.ProductImages.Add(defaultImage);
                }

                // Xử lý màu sắc - DISABLED - table removed
                // if (product.SelectedColors != null && product.SelectedColors.Count > 0)
                // {
                //     foreach (var colorId in product.SelectedColors)
                //     {
                //         ProductColorModel productColor = new ProductColorModel
                //         {
                //             ProductId = product.Id,
                //             ColorId = colorId
                //         };
                //         // _dataContext.ProductColors.Add(productColor); // Disabled - table removed
                //     }
                // }

                // Xử lý kích cỡ - DISABLED - table removed
                // if (product.SelectedSizes != null && product.SelectedSizes.Count > 0)
                // {
                //     foreach (var sizeId in product.SelectedSizes)
                //     {
                //         ProductSizeModel productSize = new ProductSizeModel
                //         {
                //             ProductId = product.Id,
                //             SizeId = sizeId
                //         };
                //         // _dataContext.ProductSizes.Add(productSize); // Disabled - table removed
                //     }
                // }

                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm sản phẩm thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Model có một vài phần bị lỗi";
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
        [AllowAnonymous]
        public async Task<IActionResult> DebugImages(int id = 959)
        {
            var product = await _dataContext.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return Json(new { error = "Product not found" });
            }

            var result = new
            {
                productId = product.Id,
                productName = product.Name,
                imageCount = product.ProductImages?.Count ?? 0,
                images = product.ProductImages?.Select(img => new
                {
                    id = img.Id,
                    imageName = img.ImageName,
                    isDefault = img.IsDefault,
                    colorId = img.ColorId,
                    fullPath = $"~/media/products/{img.ImageName}"
                }).ToList()
            };

            return Json(result);
        }

        public async Task<IActionResult> Details(int Id)
        {
            ProductModel product = await _dataContext.Products
                // .Include(p => p.ProductColors).ThenInclude(pc => pc.Color) // Disabled - table removed
                // .Include(p => p.ProductSizes).ThenInclude(ps => ps.Size) // Disabled - table removed
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Id == Id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        public async Task<IActionResult> Edit(int Id)
        {
            ProductModel product = await _dataContext.Products
                // .Include(p => p.ProductColors).ThenInclude(pc => pc.Color) // Disabled - table removed
                // .Include(p => p.ProductSizes).ThenInclude(ps => ps.Size) // Disabled - table removed
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Id == Id);

            if (product == null)
            {
                return NotFound();
            }

            // Load dropdown data
            LoadDropdownData(product.BrandId, product);

            // Note: Removed Colors and Sizes selection as per user request

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            ProductModel product,
            List<int> DeleteImages = null,  // Make optional with default value
            int? DefaultImageId = null,
            int? NewDefaultImageIndex = null,
            List<IFormFile> AdditionalImages = null)
        {
            // Reload dropdowns data
            LoadDropdownData(product.BrandId, product);
            TempData["debug"] = $"DeleteImages received: {(DeleteImages != null ? string.Join(", ", DeleteImages) : "none")}";

            if (ModelState.IsValid)
            {
                // Get existing product with related data
                var existingProduct = await _dataContext.Products
                    // .Include(p => p.ProductColors) // Disabled - table removed
                    // .Include(p => p.ProductSizes) // Disabled - table removed
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.Id == product.Id);

                if (existingProduct == null)
                    return NotFound();

                // Update product basic info
                UpdateProductBasicInfo(product, existingProduct);

                var deleteImagesStr = Request.Form["DeleteImages"];
                if (!string.IsNullOrEmpty(deleteImagesStr))
                {
                    var deleteImageIds = deleteImagesStr.ToString().Split(',').Select(int.Parse).ToList();
                    // Process deletions using deleteImageIds
                }
                // Process image deletions
                if (DeleteImages != null && DeleteImages.Count > 0)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");

                    foreach (var imageId in DeleteImages)
                    {
                        var imageToDelete = await _dataContext.ProductImages
                                    .FirstOrDefaultAsync(pi => pi.Id == imageId && pi.ProductId == existingProduct.Id);
                        if (imageToDelete != null)
                        {
                            // Don't delete the file if it's the default "noimage.jpg"
                            if (!string.Equals(imageToDelete.ImageName, "noimage.jpg"))
                            {
                                string filePath = Path.Combine(uploadDir, imageToDelete.ImageName);
                                if (System.IO.File.Exists(filePath))
                                {
                                    System.IO.File.Delete(filePath);
                                }
                            }

                            _dataContext.ProductImages.Remove(imageToDelete);

                            // If this was the default image, update product's main image
                            if (imageToDelete.IsDefault)
                            {
                                existingProduct.Image = "noimage.jpg";
                            }
                        }
                    }
                    await _dataContext.SaveChangesAsync();
                }

                // Process default image changes
                if (DefaultImageId.HasValue)
                {
                    foreach (var image in existingProduct.ProductImages)
                    {
                        bool isDefault = image.Id == DefaultImageId;
                        image.IsDefault = isDefault;

                        if (isDefault)
                        {
                            existingProduct.Image = image.ImageName;
                        }
                    }
                }

                // Process new images
                if (AdditionalImages != null && AdditionalImages.Count > 0)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");

                    // Ensure directory exists
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    for (int i = 0; i < AdditionalImages.Count; i++)
                    {
                        var image = AdditionalImages[i];
                        string imageName = Guid.NewGuid().ToString() + "_" + image.FileName;
                        string filePath = Path.Combine(uploadDir, imageName);

                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(fs);
                        }

                        bool isDefault = (NewDefaultImageIndex.HasValue && NewDefaultImageIndex.Value == i);

                        // If this is the new default image, update product's main image
                        if (isDefault)
                        {
                            existingProduct.Image = imageName;
                            // Also set all existing images to non-default
                            foreach (var existingImage in existingProduct.ProductImages)
                            {
                                existingImage.IsDefault = false;
                            }
                        }

                        // Get color ID from form if available - get from form collection directly
                        int? colorId = null;
                        string colorKey = $"NewImageColors[{i}]";
                        if (Request.Form.ContainsKey(colorKey) && !string.IsNullOrEmpty(Request.Form[colorKey]))
                        {
                            colorId = int.Parse(Request.Form[colorKey]);
                        }

                        ProductImageModel productImage = new ProductImageModel
                        {
                            ProductId = existingProduct.Id,
                            ImageName = imageName,
                            IsDefault = isDefault,
                            ColorId = colorId
                        };

                        _dataContext.ProductImages.Add(productImage);
                    }
                }

                // Note: Removed Colors and Sizes processing as per user request

                // Handle image color associations - get from form collection
                foreach (var key in Request.Form.Keys.Where(k => k.StartsWith("ImageColors[")))
                {
                    string value = Request.Form[key].ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        // Extract the image ID from the key, e.g., "ImageColors[5]" -> 5
                        int imageId;
                        if (int.TryParse(key.Substring(12, key.Length - 13), out imageId))
                        {
                            int? colorId = int.Parse(value);
                            var image = existingProduct.ProductImages.FirstOrDefault(pi => pi.Id == imageId);
                            if (image != null)
                            {
                                image.ColorId = colorId;
                                _dataContext.Update(image);
                            }
                        }
                    }
                }

                // Save all changes
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật sản phẩm thành công";
                return RedirectToAction("Index");
            }

            // If we get here, validation failed
            TempData["error"] = "Cập nhật sản phẩm thất bại";
            return View(product);
        }

        // Helper method to load dropdown data
        private void LoadDropdownData(int? selectedBrandId, ProductModel product)
        {
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", selectedBrandId ?? product?.BrandId);

            // Load categories based on selected brand
            var brandId = selectedBrandId ?? product?.BrandId;
            if (brandId.HasValue && brandId.Value > 0)
            {
                var categories = _dataContext.Products
                    .Where(p => p.BrandId == brandId)
                    .Select(p => p.Category)
                    .Where(c => c != null)
                    .Distinct()
                    .ToList();

                if (!categories.Any())
                {
                    categories = _dataContext.Categories.ToList();
                }

                ViewBag.Categories = new SelectList(categories, "Id", "Name", product?.CategoryId);
            }
            else
            {
                ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product?.CategoryId);
            }

            ViewBag.Colors = new SelectList(new List<object>(), "Id", "Name"); // Empty list to prevent NullReference
            
            // Watch-specific dropdown options
            ViewBag.GenderOptions = new SelectList(new[] { "Nam", "Nữ", "Unisex" }, product?.Gender);
            ViewBag.ConditionOptions = new SelectList(new[] { "Mới", "Đã sử dụng", "Vintage", "Limited Edition" }, product?.Condition);
            ViewBag.MovementTypeOptions = new SelectList(new[] { "Automatic", "Manual", "Quartz", "Chronograph" }, product?.MovementType);
            ViewBag.CaseMaterialOptions = new SelectList(new[] { "Stainless Steel", "Gold", "Platinum", "Titanium", "Ceramic" }, product?.CaseMaterial);
            ViewBag.BraceletMaterialOptions = new SelectList(new[] { "Stainless Steel", "Leather", "Gold", "Rubber", "NATO" }, product?.BraceletMaterial);
        }

        private void UpdateProductBasicInfo(ProductModel product, ProductModel existingProduct)
        {
            existingProduct.Name = product.Name;
            existingProduct.Slug = product.Name.Replace(" ", "-");
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.CapitalPrice = product.CapitalPrice;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.BrandId = product.BrandId;
            
            // Update watch-specific fields
            existingProduct.Model = product.Model;
            existingProduct.ModelNumber = product.ModelNumber;
            existingProduct.Year = product.Year;
            existingProduct.Gender = product.Gender;
            existingProduct.Condition = product.Condition;
            existingProduct.CaseSize = product.CaseSize;
            existingProduct.CaseMaterial = product.CaseMaterial;
            existingProduct.MovementType = product.MovementType;
            existingProduct.Calibre = product.Calibre;
            existingProduct.BraceletMaterial = product.BraceletMaterial;
            existingProduct.DialColor = product.DialColor;
            existingProduct.SerialNumber = product.SerialNumber;
            existingProduct.Quantity = product.Quantity;
            existingProduct.BoxAndPapers = product.BoxAndPapers;
            existingProduct.Certificate = product.Certificate;
            existingProduct.CreditCardPrice = product.CreditCardPrice;
        }

        private async Task ProcessImageDeletions(ProductModel existingProduct, List<int> DeleteImages)
        {
            string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");

            foreach (var imageId in DeleteImages)
            {
                var imageToDelete = existingProduct.ProductImages.FirstOrDefault(pi => pi.Id == imageId);
                if (imageToDelete != null)
                {
                    // Don't delete the file if it's the default "noimage.jpg"
                    if (!string.Equals(imageToDelete.ImageName, "noimage.jpg"))
                    {
                        string filePath = Path.Combine(uploadDir, imageToDelete.ImageName);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }

                    _dataContext.ProductImages.Remove(imageToDelete);

                    // If this was the default image, update product's main image
                    if (imageToDelete.IsDefault)
                    {
                        existingProduct.Image = "noimage.jpg";
                    }
                }
            }

            await _dataContext.SaveChangesAsync();
        }

        private async Task ProcessDefaultImageChanges(ProductModel existingProduct, int? DefaultImageId,
            int? NewDefaultImageIndex, List<IFormFile> AdditionalImages)
        {
            // If setting an existing image as default
            if (DefaultImageId.HasValue)
            {
                foreach (var image in existingProduct.ProductImages)
                {
                    bool isDefault = image.Id == DefaultImageId;
                    image.IsDefault = isDefault;

                    if (isDefault)
                    {
                        existingProduct.Image = image.ImageName;
                    }
                }
            }
            // If setting a new uploaded image as default, we'll handle that in ProcessNewImages
            else if (NewDefaultImageIndex.HasValue && AdditionalImages != null &&
                     NewDefaultImageIndex.Value < AdditionalImages.Count)
            {
                // Set all existing images as non-default
                foreach (var image in existingProduct.ProductImages)
                {
                    image.IsDefault = false;
                }
            }

            await _dataContext.SaveChangesAsync();
        }

        private async Task ProcessNewImages(ProductModel existingProduct, List<IFormFile> AdditionalImages,
            int? NewDefaultImageIndex, Dictionary<int, int?> NewImageColors)
        {
            string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");

            // Ensure directory exists
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            for (int i = 0; i < AdditionalImages.Count; i++)
            {
                var image = AdditionalImages[i];
                string imageName = Guid.NewGuid().ToString() + "_" + image.FileName;
                string filePath = Path.Combine(uploadDir, imageName);

                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fs);
                }

                bool isDefault = (NewDefaultImageIndex.HasValue && NewDefaultImageIndex.Value == i);

                // If this is the new default image, update product's main image
                if (isDefault)
                {
                    existingProduct.Image = imageName;
                }

                // Get color ID for this image if available
                int? colorId = null;
                if (NewImageColors != null && NewImageColors.ContainsKey(i))
                {
                    colorId = NewImageColors[i];
                }

                ProductImageModel productImage = new ProductImageModel
                {
                    ProductId = existingProduct.Id,
                    ImageName = imageName,
                    IsDefault = isDefault,
                    ColorId = colorId
                };

                _dataContext.ProductImages.Add(productImage);
            }

            await _dataContext.SaveChangesAsync();
        }

        private async Task ProcessColorsAndSizes(ProductModel existingProduct, ProductModel product)
        {
            // Update colors - DISABLED - table removed
            // if (product.SelectedColors != null)
            // {
            //     // _dataContext.ProductColors.RemoveRange(existingProduct.ProductColors); // Disabled - table removed

            //     foreach (var colorId in product.SelectedColors)
            //     {
            //         ProductColorModel productColor = new ProductColorModel
            //         {
            //             ProductId = product.Id,
            //             ColorId = colorId
            //         };
            //         // _dataContext.ProductColors.Add(productColor); // Disabled - table removed
            //     }
            // }

            // Update sizes - DISABLED - table removed
            // if (product.SelectedSizes != null)
            // {
            //     // _dataContext.ProductSizes.RemoveRange(existingProduct.ProductSizes); // Disabled - table removed

            //     foreach (var sizeId in product.SelectedSizes)
            //     {
            //         ProductSizeModel productSize = new ProductSizeModel
            //         {
            //             ProductId = product.Id,
            //             SizeId = sizeId
            //         };
            //         // _dataContext.ProductSizes.Add(productSize); // Disabled - table removed
            //     }
            // }

            await _dataContext.SaveChangesAsync();
        }

        private void UpdateImageColorAssociations(ProductModel existingProduct, Dictionary<int, int?> ImageColors)
        {
            foreach (var kvp in ImageColors)
            {
                var image = existingProduct.ProductImages.FirstOrDefault(pi => pi.Id == kvp.Key);
                if (image != null)
                {
                    image.ColorId = kvp.Value;
                    _dataContext.Update(image);
                }
            }
        }

        public async Task<IActionResult> Delete(int Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);
            if (!string.Equals(product.Image, "noimage.jpg"))
            {
                string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                string olfFileImage = Path.Combine(uploadDir, product.Image);

                if (System.IO.File.Exists(olfFileImage))
                {
                    System.IO.File.Delete(olfFileImage);
                }
            }
            _dataContext.Products.Remove(product);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Sản phẩm đã xóa";

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> AddQuantity(int Id)
        {
            var productByQuantity = await _dataContext.ProductQuantities.Where(p => p.ProductId == Id).ToListAsync();
            ViewBag.ProductByQuantity = productByQuantity;
            ViewBag.Id = Id;
            return View();
        }
        [HttpPost]
		[ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMoreQuantity(ProductQuantityModel productQuantityModel)
        {
            // Get the product to update
            var product = _dataContext.Products.Find(productQuantityModel.ProductId);

			if (product == null)
			{
				return NotFound(); // Handle product not found scenario
			}
			product.Quantity += productQuantityModel.Quantity;

			productQuantityModel.Quantity = productQuantityModel.Quantity;
			productQuantityModel.ProductId = productQuantityModel.ProductId;
			productQuantityModel.DateCreated = DateTime.Now;


			_dataContext.Add(productQuantityModel);
			await _dataContext.SaveChangesAsync();
			TempData["success"] = "Thêm số lượng sản phẩm thành công";
			return RedirectToAction("AddQuantity", "Product", new { Id = productQuantityModel.ProductId });
		}

    }
}
