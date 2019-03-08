using Microsoft.EntityFrameworkCore.Migrations;

namespace WebAPIs.Migrations
{
    public partial class FormContentTableaddfield : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TemplateName",
                table: "ContentTable",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemplateName",
                table: "ContentTable");
        }
    }
}
