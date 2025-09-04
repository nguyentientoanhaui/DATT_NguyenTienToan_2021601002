using System.ComponentModel.DataAnnotations;

namespace Shopping_Demo.Models
{
    public class SizeModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập tên kích cỡ")]
        public string Name { get; set; }
    }
}
