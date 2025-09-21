using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopping_Demo.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChatbotTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Carts_CartId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Carts_AspNetUsers_UserId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Orders_OrderCode",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_UserId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Coupons_CouponId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductColors_Colors_ColorId",
                table: "ProductColors");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductColors_Products_ProductId",
                table: "ProductColors");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_Colors_ColorId",
                table: "ProductImages");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductReviews_Orders_OrderId",
                table: "ProductReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Brands_BrandId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductSizes_Products_ProductId",
                table: "ProductSizes");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductSizes_Sizes_SizeId",
                table: "ProductSizes");

            migrationBuilder.DropForeignKey(
                name: "FK_WishLists_AspNetUsers_UserId",
                table: "WishLists");

            migrationBuilder.DropTable(
                name: "SearchHistories");

            migrationBuilder.DropTable(
                name: "UserBehaviors");

            migrationBuilder.DropIndex(
                name: "IX_Products_BrandId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_ColorId",
                table: "ProductImages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sizes",
                table: "Sizes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductSizes",
                table: "ProductSizes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductColors",
                table: "ProductColors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Colors",
                table: "Colors");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "MomoInfos");

            migrationBuilder.RenameTable(
                name: "Sizes",
                newName: "SizeModel");

            migrationBuilder.RenameTable(
                name: "ProductSizes",
                newName: "ProductSizeModel");

            migrationBuilder.RenameTable(
                name: "ProductColors",
                newName: "ProductColorModel");

            migrationBuilder.RenameTable(
                name: "Colors",
                newName: "ColorModel");

            migrationBuilder.RenameIndex(
                name: "IX_ProductSizes_SizeId",
                table: "ProductSizeModel",
                newName: "IX_ProductSizeModel_SizeId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductSizes_ProductId",
                table: "ProductSizeModel",
                newName: "IX_ProductSizeModel_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductColors_ProductId",
                table: "ProductColorModel",
                newName: "IX_ProductColorModel_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductColors_ColorId",
                table: "ProductColorModel",
                newName: "IX_ProductColorModel_ColorId");

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "Statisticals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "ProductReviews",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "ProductImages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImageName",
                table: "ProductImages",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OrderId",
                table: "Payments",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "OrderCode",
                table: "MomoInfos",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Compares",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Categories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Categories",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SizeName",
                table: "CartItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SizeId",
                table: "CartItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "CartItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Image",
                table: "CartItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ColorName",
                table: "CartItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ColorId",
                table: "CartItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Brands",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Brands",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Brands",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SizeModel",
                table: "SizeModel",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductSizeModel",
                table: "ProductSizeModel",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductColorModel",
                table: "ProductColorModel",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ColorModel",
                table: "ColorModel",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ChatSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastActivityAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatSessions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LargePayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CustomerEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShippingAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LargePayments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SellRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    WatchInfo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Condition = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Year = table.Column<int>(type: "int", nullable: true),
                    ExpectedPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdminResponse = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SuggestedPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SellRequests_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SellRequests_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SenderType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SenderId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    SenderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_AspNetUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ChatMessages_ChatSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "ChatSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Statisticals_ProductId",
                table: "Statisticals",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_BrandId_CategoryId",
                table: "Products",
                columns: new[] { "BrandId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CreatedDate",
                table: "Products",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive",
                table: "Products",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Slug",
                table: "Products",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_CreatedDate",
                table: "ProductReviews",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedDate",
                table: "Orders",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderCode",
                table: "Orders",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MomoInfos_OrderCode",
                table: "MomoInfos",
                column: "OrderCode");

            migrationBuilder.CreateIndex(
                name: "IX_Compares_UserId",
                table: "Compares",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                table: "Categories",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductId",
                table: "CartItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_Slug",
                table: "Brands",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SenderId",
                table: "ChatMessages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SenderType",
                table: "ChatMessages",
                column: "SenderType");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SessionId",
                table: "ChatMessages",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_Timestamp",
                table: "ChatMessages",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_CreatedAt",
                table: "ChatSessions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_IsActive",
                table: "ChatSessions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_SessionId",
                table: "ChatSessions",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_UserId",
                table: "ChatSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SellRequests_CreatedAt",
                table: "SellRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SellRequests_ProductId",
                table: "SellRequests",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SellRequests_Status",
                table: "SellRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SellRequests_UserId",
                table: "SellRequests",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Carts_CartId",
                table: "CartItems",
                column: "CartId",
                principalTable: "Carts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Products_ProductId",
                table: "CartItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_AspNetUsers_UserId",
                table: "Carts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Compares_AspNetUsers_UserId",
                table: "Compares",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MomoInfos_Orders_OrderCode",
                table: "MomoInfos",
                column: "OrderCode",
                principalTable: "Orders",
                principalColumn: "OrderCode",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Orders_OrderCode",
                table: "OrderDetails",
                column: "OrderCode",
                principalTable: "Orders",
                principalColumn: "OrderCode",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_UserId",
                table: "Orders",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Coupons_CouponId",
                table: "Orders",
                column: "CouponId",
                principalTable: "Coupons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderCode",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductColorModel_ColorModel_ColorId",
                table: "ProductColorModel",
                column: "ColorId",
                principalTable: "ColorModel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductColorModel_Products_ProductId",
                table: "ProductColorModel",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductReviews_Orders_OrderId",
                table: "ProductReviews",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Brands_BrandId",
                table: "Products",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductSizeModel_Products_ProductId",
                table: "ProductSizeModel",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductSizeModel_SizeModel_SizeId",
                table: "ProductSizeModel",
                column: "SizeId",
                principalTable: "SizeModel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Statisticals_Products_ProductId",
                table: "Statisticals",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WishLists_AspNetUsers_UserId",
                table: "WishLists",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Carts_CartId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Products_ProductId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Carts_AspNetUsers_UserId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_Compares_AspNetUsers_UserId",
                table: "Compares");

            migrationBuilder.DropForeignKey(
                name: "FK_MomoInfos_Orders_OrderCode",
                table: "MomoInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Orders_OrderCode",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_UserId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Coupons_CouponId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductColorModel_ColorModel_ColorId",
                table: "ProductColorModel");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductColorModel_Products_ProductId",
                table: "ProductColorModel");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductReviews_Orders_OrderId",
                table: "ProductReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Brands_BrandId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductSizeModel_Products_ProductId",
                table: "ProductSizeModel");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductSizeModel_SizeModel_SizeId",
                table: "ProductSizeModel");

            migrationBuilder.DropForeignKey(
                name: "FK_Statisticals_Products_ProductId",
                table: "Statisticals");

            migrationBuilder.DropForeignKey(
                name: "FK_WishLists_AspNetUsers_UserId",
                table: "WishLists");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "LargePayments");

            migrationBuilder.DropTable(
                name: "SellRequests");

            migrationBuilder.DropTable(
                name: "ChatSessions");

            migrationBuilder.DropIndex(
                name: "IX_Statisticals_ProductId",
                table: "Statisticals");

            migrationBuilder.DropIndex(
                name: "IX_Products_BrandId_CategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CreatedDate",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsActive",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Slug",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ProductReviews_CreatedDate",
                table: "ProductReviews");

            migrationBuilder.DropIndex(
                name: "IX_Payments_OrderId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CreatedDate",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderCode",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_Status",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_MomoInfos_OrderCode",
                table: "MomoInfos");

            migrationBuilder.DropIndex(
                name: "IX_Compares_UserId",
                table: "Compares");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Slug",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_ProductId",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_Brands_Slug",
                table: "Brands");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SizeModel",
                table: "SizeModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductSizeModel",
                table: "ProductSizeModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductColorModel",
                table: "ProductColorModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ColorModel",
                table: "ColorModel");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Statisticals");

            migrationBuilder.DropColumn(
                name: "OrderCode",
                table: "MomoInfos");

            migrationBuilder.DropColumn(
                name: "Age",
                table: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "SizeModel",
                newName: "Sizes");

            migrationBuilder.RenameTable(
                name: "ProductSizeModel",
                newName: "ProductSizes");

            migrationBuilder.RenameTable(
                name: "ProductColorModel",
                newName: "ProductColors");

            migrationBuilder.RenameTable(
                name: "ColorModel",
                newName: "Colors");

            migrationBuilder.RenameIndex(
                name: "IX_ProductSizeModel_SizeId",
                table: "ProductSizes",
                newName: "IX_ProductSizes_SizeId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductSizeModel_ProductId",
                table: "ProductSizes",
                newName: "IX_ProductSizes_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductColorModel_ProductId",
                table: "ProductColors",
                newName: "IX_ProductColors_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductColorModel_ColorId",
                table: "ProductColors",
                newName: "IX_ProductColors_ColorId");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "ProductReviews",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "ProductImages",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImageName",
                table: "ProductImages",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OrderId",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "OrderId",
                table: "MomoInfos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Compares",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "SizeName",
                table: "CartItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SizeId",
                table: "CartItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "CartItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Image",
                table: "CartItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ColorName",
                table: "CartItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ColorId",
                table: "CartItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Brands",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Brands",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Brands",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sizes",
                table: "Sizes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductSizes",
                table: "ProductSizes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductColors",
                table: "ProductColors",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Colors",
                table: "Colors",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "SearchHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SearchCount = table.Column<int>(type: "int", nullable: false),
                    SearchTerm = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SearchedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserBehaviors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ActionType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBehaviors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBehaviors_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserBehaviors_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_BrandId",
                table: "Products",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ColorId",
                table: "ProductImages",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBehaviors_ProductId",
                table: "UserBehaviors",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBehaviors_UserId",
                table: "UserBehaviors",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Carts_CartId",
                table: "CartItems",
                column: "CartId",
                principalTable: "Carts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_AspNetUsers_UserId",
                table: "Carts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Orders_OrderCode",
                table: "OrderDetails",
                column: "OrderCode",
                principalTable: "Orders",
                principalColumn: "OrderCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_UserId",
                table: "Orders",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Coupons_CouponId",
                table: "Orders",
                column: "CouponId",
                principalTable: "Coupons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductColors_Colors_ColorId",
                table: "ProductColors",
                column: "ColorId",
                principalTable: "Colors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductColors_Products_ProductId",
                table: "ProductColors",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_Colors_ColorId",
                table: "ProductImages",
                column: "ColorId",
                principalTable: "Colors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductReviews_Orders_OrderId",
                table: "ProductReviews",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Brands_BrandId",
                table: "Products",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductSizes_Products_ProductId",
                table: "ProductSizes",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductSizes_Sizes_SizeId",
                table: "ProductSizes",
                column: "SizeId",
                principalTable: "Sizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WishLists_AspNetUsers_UserId",
                table: "WishLists",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
