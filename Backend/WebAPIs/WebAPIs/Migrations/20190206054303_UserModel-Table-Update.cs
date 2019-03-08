using Microsoft.EntityFrameworkCore.Migrations;

namespace WebAPIs.Migrations
{
    public partial class UserModelTableUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "Login",
                newName: "ImageContent");

            migrationBuilder.AddColumn<string>(
                name: "EmailID",
                table: "Login",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailID",
                table: "Login");

            migrationBuilder.RenameColumn(
                name: "ImageContent",
                table: "Login",
                newName: "ImagePath");
        }
    }
}
