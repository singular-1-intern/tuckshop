using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Tuckshop.Reporting.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileDescriptors",
                columns: table => new
                {
                    FileDescriptorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileHash = table.Column<byte[]>(type: "varbinary(32)", maxLength: 32, nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    StorageKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    FileDataId = table.Column<int>(type: "int", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileDescriptors", x => x.FileDescriptorId);
                });

            migrationBuilder.CreateTable(
                name: "ReportRequestStatuses",
                columns: table => new
                {
                    ReportRequestStatusId = table.Column<int>(type: "int", nullable: false),
                    ReportRequestStatusName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportRequestStatuses", x => x.ReportRequestStatusId);
                });

            migrationBuilder.CreateTable(
                name: "ReportRequestTypes",
                columns: table => new
                {
                    ReportRequestTypeId = table.Column<int>(type: "int", nullable: false),
                    ReportRequestTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportRequestTypes", x => x.ReportRequestTypeId);
                });

            migrationBuilder.CreateTable(
                name: "UserLayouts",
                columns: table => new
                {
                    UserLayoutId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PropertyKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LayoutName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Layout = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLayouts", x => x.UserLayoutId);
                });

            migrationBuilder.CreateTable(
                name: "ReportRequests",
                columns: table => new
                {
                    ReportRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequestType = table.Column<int>(type: "int", maxLength: 50, nullable: false),
                    RequestDenied = table.Column<bool>(type: "bit", nullable: false),
                    Criteria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModelSchema = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime", nullable: false),
                    RequestedByUserGuid = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DataLoadReportRequestId = table.Column<int>(type: "int", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportRequests", x => x.ReportRequestId);
                    table.ForeignKey(
                        name: "FK_ReportRequests_ReportRequests_DataLoadReportRequestId",
                        column: x => x.DataLoadReportRequestId,
                        principalTable: "ReportRequests",
                        principalColumn: "ReportRequestId");
                    table.ForeignKey(
                        name: "ReportRequest_RequestType_ReportRequestType",
                        column: x => x.RequestType,
                        principalTable: "ReportRequestTypes",
                        principalColumn: "ReportRequestTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "ReportRequest_Status_ReportRequestStatus",
                        column: x => x.Status,
                        principalTable: "ReportRequestStatuses",
                        principalColumn: "ReportRequestStatusId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReportOperations",
                columns: table => new
                {
                    ReportOperationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportRequestId = table.Column<int>(type: "int", nullable: false),
                    Operation = table.Column<int>(type: "int", nullable: false),
                    FileDescriptorId = table.Column<int>(type: "int", nullable: true),
                    Error = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    DataLoadStartedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    DataLoadCompletedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    FailedOn = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportOperations", x => x.ReportOperationId);
                    table.ForeignKey(
                        name: "FK_ReportOperations_FileDescriptors_FileDescriptorId",
                        column: x => x.FileDescriptorId,
                        principalTable: "FileDescriptors",
                        principalColumn: "FileDescriptorId");
                    table.ForeignKey(
                        name: "FK_ReportOperations_ReportRequests_ReportRequestId",
                        column: x => x.ReportRequestId,
                        principalTable: "ReportRequests",
                        principalColumn: "ReportRequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportDataEntries",
                columns: table => new
                {
                    ReportDataEntryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportOperationId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Data = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportDataEntries", x => x.ReportDataEntryId);
                    table.ForeignKey(
                        name: "FK_ReportDataEntries_ReportOperations_ReportOperationId",
                        column: x => x.ReportOperationId,
                        principalTable: "ReportOperations",
                        principalColumn: "ReportOperationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ReportRequestStatuses",
                columns: new[] { "ReportRequestStatusId", "ReportRequestStatusName" },
                values: new object[,]
                {
                    { 1, "Default" },
                    { 2, "Request For Data" },
                    { 30, "Data No Longer Available" },
                    { 31, "Data Removed" },
                    { 32, "Data Expired" }
                });

            migrationBuilder.InsertData(
                table: "ReportRequestTypes",
                columns: new[] { "ReportRequestTypeId", "ReportRequestTypeName" },
                values: new object[,]
                {
                    { 1, "View" },
                    { 2, "View Data" },
                    { 101, "Download Pdf" },
                    { 102, "Download Excel" },
                    { 110, "Download Grid Report Excel" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportDataEntries_ReportOperationId",
                table: "ReportDataEntries",
                column: "ReportOperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportDataEntries_TenantId",
                table: "ReportDataEntries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportOperations_FileDescriptorId",
                table: "ReportOperations",
                column: "FileDescriptorId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportOperations_ReportRequestId",
                table: "ReportOperations",
                column: "ReportRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRequests_DataLoadReportRequestId",
                table: "ReportRequests",
                column: "DataLoadReportRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRequests_RequestType",
                table: "ReportRequests",
                column: "RequestType");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRequests_Status",
                table: "ReportRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRequests_TenantId",
                table: "ReportRequests",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportDataEntries");

            migrationBuilder.DropTable(
                name: "UserLayouts");

            migrationBuilder.DropTable(
                name: "ReportOperations");

            migrationBuilder.DropTable(
                name: "FileDescriptors");

            migrationBuilder.DropTable(
                name: "ReportRequests");

            migrationBuilder.DropTable(
                name: "ReportRequestTypes");

            migrationBuilder.DropTable(
                name: "ReportRequestStatuses");
        }
    }
}
