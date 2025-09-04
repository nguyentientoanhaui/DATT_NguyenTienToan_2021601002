using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopping_Demo.Migrations
{
    /// <inheritdoc />
    public partial class RenameCartItemModelToCartItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItemModel_Carts_CartId",
                table: "CartItemModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CartItemModel",
                table: "CartItemModel");

            migrationBuilder.RenameTable(
                name: "CartItemModel",
                newName: "CartItems");

            migrationBuilder.RenameIndex(
                name: "IX_CartItemModel_CartId",
                table: "CartItems",
                newName: "IX_CartItems_CartId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CartItems",
                table: "CartItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Carts_CartId",
                table: "CartItems",
                column: "CartId",
                principalTable: "Carts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Carts_CartId",
                table: "CartItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CartItems",
                table: "CartItems");

            migrationBuilder.RenameTable(
                name: "CartItems",
                newName: "CartItemModel");

            migrationBuilder.RenameIndex(
                name: "IX_CartItems_CartId",
                table: "CartItemModel",
                newName: "IX_CartItemModel_CartId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CartItemModel",
                table: "CartItemModel",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItemModel_Carts_CartId",
                table: "CartItemModel",
                column: "CartId",
                principalTable: "Carts",
                principalColumn: "Id");
        }
    }
}
