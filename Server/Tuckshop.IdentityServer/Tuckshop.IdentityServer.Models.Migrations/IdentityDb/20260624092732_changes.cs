using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tuckshop.IdentityServer.Models.Migrations.IdentityDb
{
    /// <inheritdoc />
    public partial class changes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OidcConfig_AuthenticationFlowType",
                table: "IdentityProviders",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OidcConfig_AuthenticationFlowType",
                table: "IdentityProviders");
        }
    }
}
