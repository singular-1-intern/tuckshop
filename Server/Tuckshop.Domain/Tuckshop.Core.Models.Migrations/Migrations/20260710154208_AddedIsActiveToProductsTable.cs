using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tuckshop.Core.Models.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsActiveToProductsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isActive",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isActive",
                table: "Products");
        }
    }
}
