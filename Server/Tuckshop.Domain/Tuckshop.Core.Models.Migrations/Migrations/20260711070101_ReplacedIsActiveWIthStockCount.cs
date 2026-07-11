using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tuckshop.Core.Models.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ReplacedIsActiveWIthStockCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isActive",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "stockCount",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "stockCount",
                table: "Products");

            migrationBuilder.AddColumn<bool>(
                name: "isActive",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
