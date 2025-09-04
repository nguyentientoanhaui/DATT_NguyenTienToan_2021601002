using Shopping_Demo.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Demo.Models
{
    public class ContactModel
    {
        [Key]
        [Required(ErrorMessage = "Yêu cầu nhập tiêu đề")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập địa chỉ")]
        public string Map { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập SĐT")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập Email")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập thông tin")]
        public string Description { get; set; }

        public string LogoImage { get; set; }

        [NotMapped]
        [FileExtension]
        public IFormFile? ImageUpload { get; set; }
    }
}
