using System.ComponentModel.DataAnnotations;

namespace Shopping_Demo.Models.ViewModels
{
    public class LoginViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập Email hoặc Username")]
        [Display(Name = "Email hoặc Username")]
        public string Email { get; set; }

        [DataType(DataType.Password), Required(ErrorMessage = "Yêu cầu nhập mật khẩu")]
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
    }
}



