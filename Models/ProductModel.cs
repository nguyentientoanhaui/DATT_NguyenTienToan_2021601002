using Shopping_Demo.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Demo.Models
{
    public class ProductModel
    {
		[Key]
		public int Id { get; set; }

		[Required(ErrorMessage = "Yêu cầu nhập tên sản phầm")]
		[StringLength(500, ErrorMessage = "Tên sản phẩm không được vượt quá 500 ký tự")]
		public string Name { get; set; }

		[StringLength(200, ErrorMessage = "Slug không được vượt quá 200 ký tự")]
		public string Slug { get; set; }

		[Required(ErrorMessage = "Yêu cầu nhập mô tả")]
		[StringLength(2000, ErrorMessage = "Mô tả không được vượt quá 2000 ký tự")]
		public string Description { get; set; }
        
        [Required(ErrorMessage = "Yêu cầu nhập giá")]
		[Range(0.01, double.MaxValue)]
		[Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
		[Required(ErrorMessage = "Yêu cầu nhập giá vốn")]
		[Range(0.01, double.MaxValue)]
		[Column(TypeName = "decimal(18,2)")]
		public decimal CapitalPrice { get; set; }

		[Required, Range(1, int.MaxValue, ErrorMessage = "Chọn một thương hiệu")]
		public int BrandId { get; set; }

		[Required, Range(1, int.MaxValue, ErrorMessage = "Chọn một danh mục")]
		public int CategoryId { get; set; }
		
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
        public int Quantity { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng đã bán phải lớn hơn hoặc bằng 0")]
        public int Sold { get; set; }

        public string Image { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Watch Specifications - New Fields
        public string Model { get; set; }
        public string ModelNumber { get; set; }
        public int? Year { get; set; }
        public string Gender { get; set; }
        public string Condition { get; set; }
        public string CaseMaterial { get; set; }
        public string CaseSize { get; set; }
        public string Crystal { get; set; }
        public string BezelMaterial { get; set; }
        public string SerialNumber { get; set; }
        public string DialColor { get; set; }
        public string HourMarkers { get; set; }
        public string Calibre { get; set; }
        public string MovementType { get; set; }
        public string Complication { get; set; }
        public string BraceletMaterial { get; set; }
        public string BraceletType { get; set; }
        public string ClaspType { get; set; }
        public bool? BoxAndPapers { get; set; }
        public string Certificate { get; set; }
        public string WarrantyInfo { get; set; }
        public string ItemNumber { get; set; }
        public decimal? CreditCardPrice { get; set; }
        
        // Timestamps
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? ScrapedAt { get; set; }
        public string SourceUrl { get; set; }
        
        // Navigation Properties
        public CategoryModel Category { get; set; }
		public BrandModel Brand { get; set; }
        public ICollection<ProductReviewModel> ProductReviews { get; set; }
        public ICollection<CartItemModel> CartItems { get; set; }
        public ICollection<WishlistModel> WishLists { get; set; }
        public ICollection<CompareModel> Compares { get; set; }
        public ICollection<SellRequestModel> SellRequests { get; set; }
        public ICollection<StatisticalModel> Statisticals { get; set; }

        [NotMapped]
		[FileExtension]
		public IFormFile? ImageUpload { get; set; }

        public List<ProductImageModel> ProductImages { get; set; }
        public List<ProductColorModel> ProductColors { get; set; }
        public List<ProductSizeModel> ProductSizes { get; set; }

        [NotMapped]
        public List<IFormFile> AdditionalImages { get; set; }

        [NotMapped]
        public List<int> SelectedColors { get; set; }

        [NotMapped]
        public List<int> SelectedSizes { get; set; }

        [NotMapped]
        public List<int> ImageColors { get; set; }

        [NotMapped]
        public int DefaultImageIndex { get; set; }
        public List<ProductQuantityModel> ProductQuantities { get; set; }
        
        public ProductModel()
        {
            ProductReviews = new List<ProductReviewModel>();
            CartItems = new List<CartItemModel>();
            WishLists = new List<WishlistModel>();
            Compares = new List<CompareModel>();
            SellRequests = new List<SellRequestModel>();
            Statisticals = new List<StatisticalModel>();
            ProductQuantities = new List<ProductQuantityModel>();
            IsActive = true;
        }
    }
}
