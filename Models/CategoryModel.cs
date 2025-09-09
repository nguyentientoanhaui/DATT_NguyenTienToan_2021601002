using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Shopping_Demo.Models
{
    public class CategoryModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập tên danh mục")]
        [StringLength(200, ErrorMessage = "Tên danh mục không được vượt quá 200 ký tự")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập mô tả")]
        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string Description { get; set; }

        [StringLength(200, ErrorMessage = "Slug không được vượt quá 200 ký tự")]
        public string Slug { get; set; }

        [Range(0, 1, ErrorMessage = "Status phải là 0 hoặc 1")]
        public int Status { get; set; }

        // Thêm các trường mới
        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public CategoryModel ParentCategory { get; set; }

        public ICollection<CategoryModel> ChildCategories { get; set; }

        // Thuộc tính xác định cấp độ danh mục (1: chính, 2: phụ)
        public int Level { get; set; } = 1;
        
        // Navigation Properties
        public ICollection<ProductModel> Products { get; set; }
        
        public CategoryModel()
        {
            ChildCategories = new List<CategoryModel>();
            Products = new List<ProductModel>();
        }
    }
}