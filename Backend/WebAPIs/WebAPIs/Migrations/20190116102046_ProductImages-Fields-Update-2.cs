using Microsoft.EntityFrameworkCore.Migrations;

namespace WebAPIs.Migrations
{
    public partial class ProductImagesFieldsUpdate2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImage_Products_Product",
                table: "ProductImage");

            migrationBuilder.RenameColumn(
                name: "Product",
                table: "ProductImage",
                newName: "ProductID");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImage_Product",
                table: "ProductImage",
                newName: "IX_ProductImage_ProductID");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImage_Products_ProductID",
                table: "ProductImage",
                column: "ProductID",
                principalTable: "Products",
                principalColumn: "ProductID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImage_Products_ProductID",
                table: "ProductImage");

            migrationBuilder.RenameColumn(
                name: "ProductID",
                table: "ProductImage",
                newName: "Product");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImage_ProductID",
                table: "ProductImage",
                newName: "IX_ProductImage_Product");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImage_Products_Product",
                table: "ProductImage",
                column: "Product",
                principalTable: "Products",
                principalColumn: "ProductID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
