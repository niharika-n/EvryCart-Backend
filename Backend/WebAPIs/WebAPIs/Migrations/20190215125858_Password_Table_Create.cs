using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WebAPIs.Migrations
{
    public partial class Password_Table_Create : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PasswordResetTable",
                columns: table => new
                {
                    ChangeID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    OldPassword = table.Column<string>(nullable: false),
                    Token = table.Column<string>(nullable: false),
                    TokenTimeOut = table.Column<DateTime>(nullable: false),
                    PasswordChanged = table.Column<bool>(nullable: false),
                    ResetDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTable", x => x.ChangeID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordResetTable");
        }
    }
}
