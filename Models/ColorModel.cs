using System.ComponentModel.DataAnnotations;

namespace Shopping_Demo.Models
{
    public class ColorModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập tên màu sắc")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập mã màu sắc")]
        public string Code { get; set; }
    }
}
