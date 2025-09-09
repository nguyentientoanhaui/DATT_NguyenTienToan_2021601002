using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Shopping_Demo.Models
{
	public class AppUserModel : IdentityUser
	{
		public string Occupation {  get; set; }
		public string RoleId {  get; set; }
		public string Token {  get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }

        [Range(1, 120, ErrorMessage = "Tuổi phải từ 1 đến 120")]
        public int? Age { get; set; }

        // Navigation Properties
        public virtual ICollection<OrderModel> Orders { get; set; }
        public virtual ICollection<CartModel> Carts { get; set; }
        public virtual ICollection<WishlistModel> WishLists { get; set; }
        public virtual ICollection<CompareModel> Compares { get; set; }
        public virtual ICollection<SellRequestModel> SellRequests { get; set; }
        // public virtual ICollection<ContactModel> Contacts { get; set; } // DISABLED

        public AppUserModel()
        {
            Orders = new List<OrderModel>();
            Carts = new List<CartModel>();
            WishLists = new List<WishlistModel>();
            Compares = new List<CompareModel>();
            SellRequests = new List<SellRequestModel>();
            // Contacts = new List<ContactModel>(); // DISABLED
        }
    }
}
