using Microsoft.EntityFrameworkCore.Migrations;

namespace WebAPIs.Migrations
{
    public partial class ProductImagesNewFieldsUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ImageContent",
                table: "ProductImages",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageExtenstion",
                table: "ProductImages",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageName",
                table: "ProductImages",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageExtenstion",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "ImageName",
                table: "ProductImages");

            migrationBuilder.AlterColumn<string>(
                name: "ImageContent",
                table: "ProductImages",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
