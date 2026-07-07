using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tuckshop.Core.Models.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddedCustomerWallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "WalletBalance",
                table: "Customers",
                type: "money",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WalletBalance",
                table: "Customers");
        }
    }
}
