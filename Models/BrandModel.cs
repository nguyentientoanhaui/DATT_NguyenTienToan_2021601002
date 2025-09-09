using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Shopping_Demo.Models
{
    public class BrandModel
    {
		[Key]
		public int Id { get; set; }
		[Required(ErrorMessage = "Yêu cầu nhập tên thương hiệu")]
		[StringLength(200, ErrorMessage = "Tên thương hiệu không được vượt quá 200 ký tự")]
		public string Name { get; set; }
		
		[Required(ErrorMessage = "Yêu cầu nhập mô tả")]
		[StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
		public string Description { get; set; }
		
		[StringLength(200, ErrorMessage = "Slug không được vượt quá 200 ký tự")]
		public string Slug { get; set; }
		
		[Range(0, 1, ErrorMessage = "Status phải là 0 hoặc 1")]
		public int Status { get; set; }
		
		// Navigation Properties
		public ICollection<ProductModel> Products { get; set; }
		
		public BrandModel()
		{
			Products = new List<ProductModel>();
		}
	}
}
