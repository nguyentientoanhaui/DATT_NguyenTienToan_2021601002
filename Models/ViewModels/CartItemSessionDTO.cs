using Shopping_Demo.Models;

namespace Shopping_Demo.Models.ViewModels
{
    // Thêm class này vào Models/ViewModels
    public class CartItemSessionDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Image { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string ColorId { get; set; }
        public string ColorName { get; set; }
        public string SizeId { get; set; }
        public string SizeName { get; set; }

        public CartItemSessionDTO() { }

        public CartItemSessionDTO(CartItemModel item)
        {
            ProductId = item.ProductId;
            ProductName = item.ProductName;
            Image = item.Image;
            Quantity = item.Quantity;
            Price = item.Price;
            ColorId = item.ColorId;
            ColorName = item.ColorName;
            SizeId = item.SizeId;
            SizeName = item.SizeName;
        }

        public CartItemSessionDTO(ProductModel product)
        {
            ProductId = product.Id;
            ProductName = product.Name;
            Image = product.Image;
            Quantity = 1;
            Price = product.Price;
            ColorId = "";
            ColorName = "";
            SizeId = "";
            SizeName = "";
        }

        public CartItemModel ToCartItemModel()
        {
            return new CartItemModel
            {
                ProductId = ProductId,
                ProductName = ProductName,
                Image = Image,
                Quantity = Quantity,
                Price = Price,
                ColorId = ColorId,
                ColorName = ColorName,
                SizeId = SizeId,
                SizeName = SizeName
            };
        }
    }

}
