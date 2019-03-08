using Microsoft.EntityFrameworkCore.Migrations;

namespace WebAPIs.Migrations
{
    public partial class update_UserRoleTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Login_UserRoleModel_RoleID",
                table: "Login");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoleModel",
                table: "UserRoleModel");

            migrationBuilder.RenameTable(
                name: "UserRoleModel",
                newName: "UserRoleTable");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoleTable",
                table: "UserRoleTable",
                column: "RoleID");

            migrationBuilder.AddForeignKey(
                name: "FK_Login_UserRoleTable_RoleID",
                table: "Login",
                column: "RoleID",
                principalTable: "UserRoleTable",
                principalColumn: "RoleID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Login_UserRoleTable_RoleID",
                table: "Login");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoleTable",
                table: "UserRoleTable");

            migrationBuilder.RenameTable(
                name: "UserRoleTable",
                newName: "UserRoleModel");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoleModel",
                table: "UserRoleModel",
                column: "RoleID");

            migrationBuilder.AddForeignKey(
                name: "FK_Login_UserRoleModel_RoleID",
                table: "Login",
                column: "RoleID",
                principalTable: "UserRoleModel",
                principalColumn: "RoleID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
