using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WebAPIs.Migrations
{
    public partial class Create_UserRoleModel_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoleID",
                table: "Login",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UserRoleModel",
                columns: table => new
                {
                    RoleID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RoleName = table.Column<string>(nullable: false),
                    RoleDescription = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoleModel", x => x.RoleID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Login_RoleID",
                table: "Login",
                column: "RoleID");

            migrationBuilder.AddForeignKey(
                name: "FK_Login_UserRoleModel_RoleID",
                table: "Login",
                column: "RoleID",
                principalTable: "UserRoleModel",
                principalColumn: "RoleID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Login_UserRoleModel_RoleID",
                table: "Login");

            migrationBuilder.DropTable(
                name: "UserRoleModel");

            migrationBuilder.DropIndex(
                name: "IX_Login_RoleID",
                table: "Login");

            migrationBuilder.DropColumn(
                name: "RoleID",
                table: "Login");
        }
    }
}
