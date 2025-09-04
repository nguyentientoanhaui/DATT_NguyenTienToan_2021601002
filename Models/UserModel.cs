using System.ComponentModel.DataAnnotations;

namespace Shopping_Demo.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập Email"), EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password), Required(ErrorMessage = "Yêu cầu nhập mật khẩu")]
        public string Password { get; set; }
    }
}
