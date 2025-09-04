using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopping_Demo.Migrations
{
    /// <inheritdoc />
    public partial class AddConnect : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OrderCode",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OrderCode",
                table: "OrderDetails",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Orders_OrderCode",
                table: "Orders",
                column: "OrderCode");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderCode",
                table: "OrderDetails",
                column: "OrderCode");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Orders_OrderCode",
                table: "OrderDetails",
                column: "OrderCode",
                principalTable: "Orders",
                principalColumn: "OrderCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Orders_OrderCode",
                table: "OrderDetails");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Orders_OrderCode",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_OrderCode",
                table: "OrderDetails");

            migrationBuilder.AlterColumn<string>(
                name: "OrderCode",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "OrderCode",
                table: "OrderDetails",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
