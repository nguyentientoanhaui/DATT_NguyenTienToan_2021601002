namespace Shopping_Demo.Models
{
    public class CartModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } 
        public AppUserModel User { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public virtual ICollection<CartItemModel> CartItems { get; set; }
    }
}
