using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Models;
using Shopping_Demo.Repository;

namespace Shopping_Demo.Services
{
    public interface IRecommendationService
    {
        Task TrackUserBehavior(string? sessionId, string? userId, int productId, string actionType, string? ipAddress, string? userAgent);
        Task<List<ProductModel>> GetPersonalizedRecommendations(string? sessionId, string? userId, int limit = 8);
        Task<List<ProductModel>> GetRecentlyViewedProducts(string? sessionId, string? userId, int limit = 8);
        Task<List<ProductModel>> GetSimilarProducts(int productId, int limit = 8);
    }

    public class RecommendationService : IRecommendationService
    {
        private readonly DataContext _context;

        public RecommendationService(DataContext context)
        {
            _context = context;
        }

        public async Task TrackUserBehavior(string? sessionId, string? userId, int productId, string actionType, string? ipAddress, string? userAgent)
        {
            var behavior = new UserBehaviorModel
            {
                SessionId = sessionId,
                UserId = userId,
                ProductId = productId,
                ActionType = actionType,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Timestamp = DateTime.Now
            };

            // _context.UserBehaviors.Add(behavior); // Disabled - table removed
            // await _context.SaveChangesAsync();
        }

        public async Task<List<ProductModel>> GetPersonalizedRecommendations(string? sessionId, string? userId, int limit = 8)
        {
            // Lấy danh sách sản phẩm user đã xem - Disabled since UserBehaviors table was removed
            // var viewedProducts = await _context.UserBehaviors
            //     .Where(ub => (ub.SessionId == sessionId || ub.UserId == userId) && ub.ActionType == "View")
            //     .GroupBy(ub => ub.ProductId)
            //     .Select(g => new { ProductId = g.Key, ViewCount = g.Count() })
            //     .OrderByDescending(x => x.ViewCount)
            //     .Take(10)
            //     .ToListAsync();
            var viewedProducts = new List<object>(); // Empty list since table was removed

            if (!viewedProducts.Any())
            {
                // Nếu chưa có hành vi, trả về sản phẩm nổi bật
                return await GetPopularProducts(limit);
            }

            // Lấy categories và brands từ sản phẩm đã xem - Disabled since UserBehaviors table was removed
            // var viewedProductIds = viewedProducts.Select(x => x.ProductId).ToList();
            var viewedProductIds = new List<int>(); // Empty list since table was removed
            var userPreferences = await _context.Products
                .Where(p => viewedProductIds.Contains(p.Id))
                .GroupBy(p => new { p.CategoryId, p.BrandId })
                .Select(g => new { g.Key.CategoryId, g.Key.BrandId, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            // Tìm sản phẩm tương tự
            var recommendedProducts = new List<ProductModel>();
            foreach (var preference in userPreferences)
            {
                var similarProducts = await _context.Products
                    .Where(p => p.IsActive && 
                               (p.CategoryId == preference.CategoryId || p.BrandId == preference.BrandId) &&
                               !viewedProductIds.Contains(p.Id))
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.ProductImages)
                    .Take(limit / userPreferences.Count)
                    .ToListAsync();

                recommendedProducts.AddRange(similarProducts);
            }

            // Nếu chưa đủ, thêm sản phẩm nổi bật
            if (recommendedProducts.Count < limit)
            {
                var popularProducts = await GetPopularProducts(limit - recommendedProducts.Count);
                var existingIds = recommendedProducts.Select(p => p.Id).ToList();
                var newProducts = popularProducts.Where(p => !existingIds.Contains(p.Id)).ToList();
                recommendedProducts.AddRange(newProducts);
            }

            return recommendedProducts.Take(limit).ToList();
        }

        public async Task<List<ProductModel>> GetRecentlyViewedProducts(string? sessionId, string? userId, int limit = 8)
        {
            // var recentViews = await _context.UserBehaviors
            //     .Where(ub => (ub.SessionId == sessionId || ub.UserId == userId) && ub.ActionType == "View")
            //     .OrderByDescending(ub => ub.Timestamp)
            //     .GroupBy(ub => ub.ProductId)
            //     .Select(g => g.First())
            //     .Take(limit)
            //     .Select(ub => ub.ProductId)
            //     .ToListAsync();

            // if (!recentViews.Any())
            //     return new List<ProductModel>();

            // return await _context.Products
            return new List<ProductModel>(); // Return empty list since UserBehaviors table was removed
        }

        public async Task<List<ProductModel>> GetSimilarProducts(int productId, int limit = 8)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return new List<ProductModel>();

            return await _context.Products
                .Where(p => p.IsActive && p.Id != productId &&
                           (p.CategoryId == product.CategoryId || p.BrandId == product.BrandId))
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .Take(limit)
                .ToListAsync();
        }

        private async Task<List<ProductModel>> GetPopularProducts(int limit)
        {
            // Sản phẩm có nhiều lượt xem nhất - Disabled since UserBehaviors table was removed
            // var popularProductIds = await _context.UserBehaviors
            //     .Where(ub => ub.ActionType == "View")
            //     .GroupBy(ub => ub.ProductId)
            //     .Select(g => new { ProductId = g.Key, ViewCount = g.Count() })
            //     .OrderByDescending(x => x.ViewCount)
            //     .Take(limit)
            //     .Select(x => x.ProductId)
            //     .ToListAsync();
            var popularProductIds = new List<int>(); // Empty list since table was removed

            if (!popularProductIds.Any())
            {
                // Fallback: sản phẩm mới nhất
                return await _context.Products
                    .Where(p => p.IsActive)
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.ProductImages)
                    .OrderByDescending(p => p.Id)
                    .Take(limit)
                    .ToListAsync();
            }

            return await _context.Products
                .Where(p => popularProductIds.Contains(p.Id) && p.IsActive)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .OrderBy(p => popularProductIds.IndexOf(p.Id))
                .ToListAsync();
        }
    }
}
