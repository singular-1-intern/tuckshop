using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tuckshop.AuthorisationServer.Models.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Audit_UserGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChangeAction = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ChangedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ChangedByGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserGroupId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    UserGroupName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsAdministratorGroup = table.Column<bool>(type: "bit", nullable: false),
                    Deleted_By = table.Column<int>(type: "int", nullable: true),
                    Deleted_On = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audit_UserGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    ResourceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResourceName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Hash = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.ResourceId);
                });

            migrationBuilder.CreateTable(
                name: "UserGroups",
                columns: table => new
                {
                    UserGroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    UserGroupName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsAdministratorGroup = table.Column<bool>(type: "bit", nullable: false),
                    Audit_CreatedBy = table.Column<int>(type: "int", nullable: true),
                    Audit_CreatedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    Audit_ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    Audit_ModifiedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    Deleted_By = table.Column<int>(type: "int", nullable: true),
                    Deleted_On = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroups", x => x.UserGroupId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsInvitedUser = table.Column<bool>(type: "bit", nullable: false),
                    IdentityGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PreferredName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "RoleCategories",
                columns: table => new
                {
                    RoleCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResourceId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleCategories", x => x.RoleCategoryId);
                    table.ForeignKey(
                        name: "FK_RoleCategories_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "ResourceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Memberships",
                columns: table => new
                {
                    MembershipId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    UserGroupId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Audit_CreatedBy = table.Column<int>(type: "int", nullable: true),
                    Audit_CreatedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    Audit_ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    Audit_ModifiedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    Deleted_By = table.Column<int>(type: "int", nullable: true),
                    Deleted_On = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Memberships", x => x.MembershipId);
                    table.ForeignKey(
                        name: "FK_Memberships_UserGroups_UserGroupId",
                        column: x => x.UserGroupId,
                        principalTable: "UserGroups",
                        principalColumn: "UserGroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Memberships_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Registrations",
                columns: table => new
                {
                    RegistrationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registrations", x => x.RegistrationId);
                    table.ForeignKey(
                        name: "FK_Registrations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleCategoryId = table.Column<int>(type: "int", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoleDescription = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                    table.ForeignKey(
                        name: "FK_Roles_RoleCategories_RoleCategoryId",
                        column: x => x.RoleCategoryId,
                        principalTable: "RoleCategories",
                        principalColumn: "RoleCategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssignedRoles",
                columns: table => new
                {
                    AssignedRoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserGroupId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    Audit_CreatedBy = table.Column<int>(type: "int", nullable: true),
                    Audit_CreatedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    Audit_ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    Audit_ModifiedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    Deleted_By = table.Column<int>(type: "int", nullable: true),
                    Deleted_On = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignedRoles", x => x.AssignedRoleId);
                    table.ForeignKey(
                        name: "FK_AssignedRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignedRoles_UserGroups_UserGroupId",
                        column: x => x.UserGroupId,
                        principalTable: "UserGroups",
                        principalColumn: "UserGroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignedRoles_Deleted_On",
                table: "AssignedRoles",
                column: "Deleted_On",
                filter: "Deleted_On IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AssignedRoles_RoleId",
                table: "AssignedRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignedRoles_UserGroupId_RoleId",
                table: "AssignedRoles",
                columns: new[] { "UserGroupId", "RoleId" },
                unique: true,
                filter: "Deleted_On IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Audit_UserGroups_UserGroupId",
                table: "Audit_UserGroups",
                column: "UserGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_Deleted_On",
                table: "Memberships",
                column: "Deleted_On",
                filter: "Deleted_On IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_TenantId",
                table: "Memberships",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_TenantId_UserGroupId_UserId",
                table: "Memberships",
                columns: new[] { "TenantId", "UserGroupId", "UserId" },
                unique: true,
                filter: "Deleted_On IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_UserGroupId",
                table: "Memberships",
                column: "UserGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_UserId",
                table: "Memberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_TenantId",
                table: "Registrations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_TenantId_UserId",
                table: "Registrations",
                columns: new[] { "TenantId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_UserId",
                table: "Registrations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_DeletedOn",
                table: "Resources",
                column: "DeletedOn",
                filter: "DeletedOn IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_ResourceName",
                table: "Resources",
                column: "ResourceName",
                unique: true,
                filter: "DeletedOn IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RoleCategories_DeletedOn",
                table: "RoleCategories",
                column: "DeletedOn",
                filter: "DeletedOn IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RoleCategories_ResourceId_Category",
                table: "RoleCategories",
                columns: new[] { "ResourceId", "Category" },
                unique: true,
                filter: "DeletedOn IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_DeletedOn",
                table: "Roles",
                column: "DeletedOn",
                filter: "DeletedOn IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_RoleCategoryId_RoleName",
                table: "Roles",
                columns: new[] { "RoleCategoryId", "RoleName" },
                unique: true,
                filter: "DeletedOn IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_Deleted_On",
                table: "UserGroups",
                column: "Deleted_On",
                filter: "Deleted_On IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_TenantId",
                table: "UserGroups",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_TenantId_UserGroupName",
                table: "UserGroups",
                columns: new[] { "TenantId", "UserGroupName" },
                unique: true,
                filter: "Deleted_On IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IdentityGuid_ClientId",
                table: "Users",
                columns: new[] { "IdentityGuid", "ClientId" },
                unique: true,
                filter: "[IdentityGuid] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignedRoles");

            migrationBuilder.DropTable(
                name: "Audit_UserGroups");

            migrationBuilder.DropTable(
                name: "Memberships");

            migrationBuilder.DropTable(
                name: "Registrations");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "UserGroups");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "RoleCategories");

            migrationBuilder.DropTable(
                name: "Resources");
        }
    }
}
