using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopping_Demo.Migrations
{
    /// <inheritdoc />
    public partial class AddConnect2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductModelId",
                table: "ProductQuantities",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuantities_ProductModelId",
                table: "ProductQuantities",
                column: "ProductModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductQuantities_Products_ProductModelId",
                table: "ProductQuantities",
                column: "ProductModelId",
                principalTable: "Products",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductQuantities_Products_ProductModelId",
                table: "ProductQuantities");

            migrationBuilder.DropIndex(
                name: "IX_ProductQuantities_ProductModelId",
                table: "ProductQuantities");

            migrationBuilder.DropColumn(
                name: "ProductModelId",
                table: "ProductQuantities");
        }
    }
}
