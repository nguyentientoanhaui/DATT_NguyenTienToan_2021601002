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
        public string Name { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập mô tả")]
        public string Description { get; set; }

        public string Slug { get; set; }

        public int Status { get; set; }

        // Thêm các trường mới
        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public CategoryModel ParentCategory { get; set; }

        public ICollection<CategoryModel> ChildCategories { get; set; }

        // Thuộc tính xác định cấp độ danh mục (1: chính, 2: phụ)
        public int Level { get; set; } = 1;
    }
}