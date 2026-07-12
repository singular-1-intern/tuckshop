using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tuckshop.Core.Models.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddedCustomerCellNoToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerCellNo",
                table: "Customers",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerCellNo",
                table: "Customers");
        }
    }
}
