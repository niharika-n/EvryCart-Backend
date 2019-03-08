using Microsoft.EntityFrameworkCore.Migrations;

namespace WebAPIs.Migrations
{
    public partial class productattributevaluesupdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductAttributeValues_ProductAttributes_Attribute",
                table: "ProductAttributeValues");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductAttributeValues_Products_Product",
                table: "ProductAttributeValues");

            migrationBuilder.RenameColumn(
                name: "Product",
                table: "ProductAttributeValues",
                newName: "ProductID");

            migrationBuilder.RenameColumn(
                name: "Attribute",
                table: "ProductAttributeValues",
                newName: "AttributeID");

            migrationBuilder.RenameIndex(
                name: "IX_ProductAttributeValues_Product",
                table: "ProductAttributeValues",
                newName: "IX_ProductAttributeValues_ProductID");

            migrationBuilder.RenameIndex(
                name: "IX_ProductAttributeValues_Attribute",
                table: "ProductAttributeValues",
                newName: "IX_ProductAttributeValues_AttributeID");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductAttributeValues_ProductAttributes_AttributeID",
                table: "ProductAttributeValues",
                column: "AttributeID",
                principalTable: "ProductAttributes",
                principalColumn: "AttributeID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductAttributeValues_Products_ProductID",
                table: "ProductAttributeValues",
                column: "ProductID",
                principalTable: "Products",
                principalColumn: "ProductID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductAttributeValues_ProductAttributes_AttributeID",
                table: "ProductAttributeValues");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductAttributeValues_Products_ProductID",
                table: "ProductAttributeValues");

            migrationBuilder.RenameColumn(
                name: "ProductID",
                table: "ProductAttributeValues",
                newName: "Product");

            migrationBuilder.RenameColumn(
                name: "AttributeID",
                table: "ProductAttributeValues",
                newName: "Attribute");

            migrationBuilder.RenameIndex(
                name: "IX_ProductAttributeValues_ProductID",
                table: "ProductAttributeValues",
                newName: "IX_ProductAttributeValues_Product");

            migrationBuilder.RenameIndex(
                name: "IX_ProductAttributeValues_AttributeID",
                table: "ProductAttributeValues",
                newName: "IX_ProductAttributeValues_Attribute");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductAttributeValues_ProductAttributes_Attribute",
                table: "ProductAttributeValues",
                column: "Attribute",
                principalTable: "ProductAttributes",
                principalColumn: "AttributeID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductAttributeValues_Products_Product",
                table: "ProductAttributeValues",
                column: "Product",
                principalTable: "Products",
                principalColumn: "ProductID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
