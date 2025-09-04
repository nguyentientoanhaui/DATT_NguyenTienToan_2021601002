namespace Shopping_Demo.Models.ViewModels
{
    public class WishlistSessionDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }

        // Constructor để chuyển đổi từ WishlistModel sang DTO
        public WishlistSessionDTO() { }

        public WishlistSessionDTO(ProductModel product)
        {
            ProductId = product.Id;
            ProductName = product.Name;
            Price = product.Price;
            Image = product.Image;
        }


        // Phương thức chuyển đổi từ DTO sang WishlistModel
        public WishlistModel ToWishlistModel()
        {
            return new WishlistModel
            {
                ProductId = this.ProductId
            };
        }
    }
}
