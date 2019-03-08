using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WebAPIs.Migrations
{
    public partial class ProductAttributesFieldsAdd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "ProductAttributes",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "ProductAttributes",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ModifiedBy",
                table: "ProductAttributes",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "ProductAttributes",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ProductAttributes");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "ProductAttributes");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "ProductAttributes");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "ProductAttributes");
        }
    }
}
