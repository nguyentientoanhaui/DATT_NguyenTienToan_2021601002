using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shopping_Demo.Models;

namespace Shopping_Demo.Repository
{
	public class DataContext : IdentityDbContext<AppUserModel>
	{
		public DataContext(DbContextOptions<DataContext> options) : base(options) { }
		public DbSet<BrandModel> Brands { get; set; }
		public DbSet<CategoryModel> Categories { get; set; }
		public DbSet<ProductModel> Products { get; set; }
		public DbSet<OrderModel> Orders { get; set; }
		public DbSet<OrderDetails> OrderDetails { get; set; }
		        public DbSet<ProductReviewModel> ProductReviews { get; set; }
		public DbSet<SliderModel> Sliders { get; set; }
		public DbSet<ContactModel> Contact { get; set; }
        public DbSet<WishlistModel> WishLists { get; set; }
        public DbSet<CompareModel> Compares { get; set; }
        public DbSet<ProductQuantityModel> ProductQuantities { get; set; }
        public DbSet<ShippingModel> Shippings { get; set; }
        public DbSet<CouponModel> Coupons { get; set; }
        public DbSet<StatisticalModel> Statisticals { get; set; }
        public DbSet<CartModel> Carts { get; set; }
        public DbSet<CartItemModel> CartItems { get; set; }
        public DbSet<MomoInfoModel> MomoInfos { get; set; }
        public DbSet<ColorModel> Colors { get; set; }
        public DbSet<SizeModel> Sizes { get; set; }
        public DbSet<ProductImageModel> ProductImages { get; set; }
        public DbSet<ProductColorModel> ProductColors { get; set; }
        public DbSet<ProductSizeModel> ProductSizes { get; set; }
        public DbSet<UserBehaviorModel> UserBehaviors { get; set; }
        public DbSet<SearchHistoryModel> SearchHistories { get; set; }
        public DbSet<PaymentModel> Payments { get; set; }
        public DbSet<SellRequestModel> SellRequests { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Thiết lập mối quan hệ giữa Order và OrderDetails
            modelBuilder.Entity<OrderDetails>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderCode)
                .HasPrincipalKey(o => o.OrderCode);

            // Thiết lập mối quan hệ giữa Order và Coupon
            //modelBuilder.Entity<OrderModel>()
            //    .HasOne(o => o.Coupon)
            //    .WithMany(c => c.Orders)
            //    .HasForeignKey(o => o.CouponCode)
            //    .HasPrincipalKey(c => c.Name); // Giả định CouponCode tương ứng với Name trong CouponModel

            // Thiết lập mối quan hệ giữa Order và Shipping
            //modelBuilder.Entity<OrderModel>()
            //    .HasOne(o => o.Shipping)
            //    .WithMany(s => s.Orders)
            //    .HasForeignKey(o => o.ShippingId);
        }

    }
}
