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
        // public DbSet<ColorModel> Colors { get; set; } // Removed - table deleted from database
        // public DbSet<SizeModel> Sizes { get; set; } // Removed - table deleted from database
        public DbSet<ProductImageModel> ProductImages { get; set; }
        // public DbSet<ProductColorModel> ProductColors { get; set; } // Removed - table deleted from database
        // public DbSet<ProductSizeModel> ProductSizes { get; set; } // Removed - table deleted from database
        // public DbSet<UserBehaviorModel> UserBehaviors { get; set; } // Removed - table deleted from database
        // public DbSet<SearchHistoryModel> SearchHistories { get; set; } // Removed - table deleted from database
        public DbSet<PaymentModel> Payments { get; set; }
        public DbSet<SellRequestModel> SellRequests { get; set; }
        public DbSet<LargePaymentModel> LargePayments { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== FOREIGN KEY CONSTRAINTS =====
            
            // Order và OrderDetails
            modelBuilder.Entity<OrderDetails>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderCode)
                .HasPrincipalKey(o => o.OrderCode)
                .OnDelete(DeleteBehavior.Cascade);

            // Products và các bảng liên quan
            modelBuilder.Entity<ProductModel>()
                .HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductModel>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // SellRequests
            modelBuilder.Entity<SellRequestModel>()
                .HasOne(sr => sr.Product)
                .WithMany(p => p.SellRequests)
                .HasForeignKey(sr => sr.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Explicitly configure the foreign key to use ProductId instead of ProductModelId
            modelBuilder.Entity<SellRequestModel>()
                .Property(sr => sr.ProductId)
                .HasColumnName("ProductId");

            // CartItems
            modelBuilder.Entity<CartItemModel>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // WishLists -> Users
            modelBuilder.Entity<WishlistModel>()
                .HasOne(w => w.User)
                .WithMany(u => u.WishLists)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Compares - DISABLED temporarily
            // modelBuilder.Entity<CompareModel>()
            //     .HasOne(c => c.User)
            //     .WithMany(u => u.Compares)
            //     .HasForeignKey(c => c.UserId)
            //     .OnDelete(DeleteBehavior.Cascade);

            // ProductImages
            modelBuilder.Entity<ProductImageModel>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ProductReviews
            modelBuilder.Entity<ProductReviewModel>()
                .HasOne(pr => pr.Product)
                .WithMany(p => p.ProductReviews)
                .HasForeignKey(pr => pr.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductReviewModel>()
                .HasOne(pr => pr.Order)
                .WithMany(o => o.ProductReviews)
                .HasForeignKey(pr => pr.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== THÊM CÁC LIÊN KẾT CÒN THIẾU =====

            // CartItems -> Products
            modelBuilder.Entity<CartItemModel>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            // Orders -> Users
            modelBuilder.Entity<OrderModel>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Orders -> Coupons
            modelBuilder.Entity<OrderModel>()
                .HasOne(o => o.Coupon)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CouponId)
                .OnDelete(DeleteBehavior.SetNull);

            // Carts -> Users
            modelBuilder.Entity<CartModel>()
                .HasOne(c => c.User)
                .WithMany(u => u.Carts)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // WishLists -> Products
            modelBuilder.Entity<WishlistModel>()
                .HasOne(w => w.Product)
                .WithMany(p => p.WishLists)
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Compares -> Products
            modelBuilder.Entity<CompareModel>()
                .HasOne(c => c.Product)
                .WithMany(p => p.Compares)
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // SellRequests -> Users
            modelBuilder.Entity<SellRequestModel>()
                .HasOne(sr => sr.User)
                .WithMany(u => u.SellRequests)
                .HasForeignKey(sr => sr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payments -> Orders (qua OrderCode)
            modelBuilder.Entity<PaymentModel>()
                .HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderId)
                .HasPrincipalKey(o => o.OrderCode)
                .OnDelete(DeleteBehavior.Cascade);

            // MomoInfos -> Orders (qua OrderCode)
            modelBuilder.Entity<MomoInfoModel>()
                .HasOne(m => m.Order)
                .WithMany(o => o.MomoInfos)
                .HasForeignKey(m => m.OrderCode)
                .HasPrincipalKey(o => o.OrderCode)
                .OnDelete(DeleteBehavior.Cascade);

            // Statisticals -> Products
            modelBuilder.Entity<StatisticalModel>()
                .HasOne(s => s.Product)
                .WithMany(p => p.Statisticals)
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Contact -> Users (nếu có UserId) - DISABLED
            // modelBuilder.Entity<ContactModel>()
            //     .HasOne(c => c.User)
            //     .WithMany(u => u.Contacts)
            //     .HasForeignKey(c => c.UserId)
            //     .OnDelete(DeleteBehavior.SetNull);

            // ===== UNIQUE CONSTRAINTS =====
            
            // OrderCode unique
            modelBuilder.Entity<OrderModel>()
                .HasIndex(o => o.OrderCode)
                .IsUnique();

            // Slug unique cho Products, Categories, Brands
            modelBuilder.Entity<ProductModel>()
                .HasIndex(p => p.Slug)
                .IsUnique()
                .HasFilter("[Slug] IS NOT NULL");

            modelBuilder.Entity<CategoryModel>()
                .HasIndex(c => c.Slug)
                .IsUnique()
                .HasFilter("[Slug] IS NOT NULL");

            modelBuilder.Entity<BrandModel>()
                .HasIndex(b => b.Slug)
                .IsUnique()
                .HasFilter("[Slug] IS NOT NULL");

            // ===== INDEXES FOR PERFORMANCE =====
            
            // Products
            modelBuilder.Entity<ProductModel>()
                .HasIndex(p => p.IsActive);
            modelBuilder.Entity<ProductModel>()
                .HasIndex(p => p.CreatedDate);
            modelBuilder.Entity<ProductModel>()
                .HasIndex(p => new { p.BrandId, p.CategoryId });

            // Orders
            modelBuilder.Entity<OrderModel>()
                .HasIndex(o => o.UserId);
            modelBuilder.Entity<OrderModel>()
                .HasIndex(o => o.CreatedDate);
            modelBuilder.Entity<OrderModel>()
                .HasIndex(o => o.Status);

            // SellRequests
            modelBuilder.Entity<SellRequestModel>()
                .HasIndex(sr => sr.Status);
            modelBuilder.Entity<SellRequestModel>()
                .HasIndex(sr => sr.CreatedAt);
            modelBuilder.Entity<SellRequestModel>()
                .HasIndex(sr => sr.UserId);

            // ProductReviews
            modelBuilder.Entity<ProductReviewModel>()
                .HasIndex(pr => pr.ProductId);
            modelBuilder.Entity<ProductReviewModel>()
                .HasIndex(pr => pr.CreatedDate);

            // ===== DATA VALIDATION =====
            
            // String length constraints
            modelBuilder.Entity<ProductModel>()
                .Property(p => p.Name)
                .HasMaxLength(500);

            modelBuilder.Entity<ProductModel>()
                .Property(p => p.Slug)
                .HasMaxLength(200);

            modelBuilder.Entity<ProductModel>()
                .Property(p => p.Description)
                .HasMaxLength(2000);

            modelBuilder.Entity<CategoryModel>()
                .Property(c => c.Name)
                .HasMaxLength(200);

            modelBuilder.Entity<BrandModel>()
                .Property(b => b.Name)
                .HasMaxLength(200);

            // Decimal precision
            modelBuilder.Entity<ProductModel>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProductModel>()
                .Property(p => p.CapitalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderModel>()
                .Property(o => o.ShippingCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderModel>()
                .Property(o => o.DiscountAmount)
                .HasPrecision(18, 2);
        }

    }
}
