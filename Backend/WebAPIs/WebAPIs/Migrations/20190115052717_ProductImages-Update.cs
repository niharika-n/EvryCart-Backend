using Microsoft.EntityFrameworkCore.Migrations;

namespace WebAPIs.Migrations
{
    public partial class ProductImagesUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_Images_Image",
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_Image",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "ProductImages");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Image",
                table: "ProductImages",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_Image",
                table: "ProductImages",
                column: "Image");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_Images_Image",
                table: "ProductImages",
                column: "Image",
                principalTable: "Images",
                principalColumn: "ImageID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
