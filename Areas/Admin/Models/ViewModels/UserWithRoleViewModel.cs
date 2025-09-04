using Shopping_Demo.Models;

namespace Shopping_Demo.Areas.Admin.Models.ViewModels
{
    public class UserWithRoleViewModel
    {
        public AppUserModel User { get; set; }
        public string RoleName { get; set; }
    }
}
