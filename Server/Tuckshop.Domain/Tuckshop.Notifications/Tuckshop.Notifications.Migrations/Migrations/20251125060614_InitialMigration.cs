using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Tuckshop.Notifications.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BulkNotificationStatuses",
                columns: table => new
                {
                    BulkNotificationStatusId = table.Column<int>(type: "int", nullable: false),
                    BulkNotificationStatusName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulkNotificationStatuses", x => x.BulkNotificationStatusId);
                });

            migrationBuilder.CreateTable(
                name: "BulkNotificationTemplates",
                columns: table => new
                {
                    BulkNotificationTemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BulkNotificationTypeKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CriteriaJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GridLayout = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Audit_CreatedBy = table.Column<int>(type: "int", nullable: true),
                    Audit_CreatedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    Audit_ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    Audit_ModifiedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulkNotificationTemplates", x => x.BulkNotificationTemplateId);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryStatuses",
                columns: table => new
                {
                    DeliveryStatusId = table.Column<int>(type: "int", nullable: false),
                    DeliveryStatusName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryStatuses", x => x.DeliveryStatusId);
                });

            migrationBuilder.CreateTable(
                name: "FileDescriptors",
                columns: table => new
                {
                    FileDescriptorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsExternal = table.Column<bool>(type: "bit", nullable: false),
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
                name: "NotificationSettings",
                columns: table => new
                {
                    NotificationSettingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailOverride_Default = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailOverride_AllowedList = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SmsOverride_Default = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SmsOverride_AllowedList = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sender_DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Sender_EmailAddress = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    SystemContacts_ClientAdminEmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SystemContacts_DeveloperEmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationSettings", x => x.NotificationSettingId);
                });

            migrationBuilder.CreateTable(
                name: "TemplateStyleSheets",
                columns: table => new
                {
                    StyleSheetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StyleSheetName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    StyleSheetBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    Audit_CreatedBy = table.Column<int>(type: "int", nullable: true),
                    Audit_CreatedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    Audit_ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    Audit_ModifiedOn = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateStyleSheets", x => x.StyleSheetId);
                });

            migrationBuilder.CreateTable(
                name: "BulkNotifications",
                columns: table => new
                {
                    BulkNotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BulkNotificationTemplateId = table.Column<int>(type: "int", nullable: false),
                    SendResult_NotificationBatchId = table.Column<int>(type: "int", nullable: true),
                    SendResult_NotificationCount = table.Column<int>(type: "int", nullable: false),
                    SendResult_SentCount = table.Column<int>(type: "int", nullable: false),
                    SendResult_FailedCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FailedReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulkNotifications", x => x.BulkNotificationId);
                    table.ForeignKey(
                        name: "BulkNotification_Status_BulkNotificationStatus",
                        column: x => x.Status,
                        principalTable: "BulkNotificationStatuses",
                        principalColumn: "BulkNotificationStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BulkNotifications_BulkNotificationTemplates_BulkNotificationTemplateId",
                        column: x => x.BulkNotificationTemplateId,
                        principalTable: "BulkNotificationTemplates",
                        principalColumn: "BulkNotificationTemplateId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemplateLayouts",
                columns: table => new
                {
                    TemplateLayoutId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LayoutKey = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    LayoutName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LayoutContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StyleSheetId = table.Column<int>(type: "int", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    Audit_CreatedBy = table.Column<int>(type: "int", nullable: true),
                    Audit_CreatedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    Audit_ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    Audit_ModifiedOn = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateLayouts", x => x.TemplateLayoutId);
                    table.ForeignKey(
                        name: "FK_TemplateLayouts_TemplateStyleSheets_StyleSheetId",
                        column: x => x.StyleSheetId,
                        principalTable: "TemplateStyleSheets",
                        principalColumn: "StyleSheetId");
                });

            migrationBuilder.CreateTable(
                name: "CustomTemplateLayouts",
                columns: table => new
                {
                    CustomTemplateLayoutId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateLayoutId = table.Column<int>(type: "int", nullable: false),
                    LayoutContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StyleSheetId = table.Column<int>(type: "int", nullable: true),
                    CultureCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomTemplateLayouts", x => x.CustomTemplateLayoutId);
                    table.ForeignKey(
                        name: "FK_CustomTemplateLayouts_TemplateLayouts_TemplateLayoutId",
                        column: x => x.TemplateLayoutId,
                        principalTable: "TemplateLayouts",
                        principalColumn: "TemplateLayoutId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomTemplateLayouts_TemplateStyleSheets_StyleSheetId",
                        column: x => x.StyleSheetId,
                        principalTable: "TemplateStyleSheets",
                        principalColumn: "StyleSheetId");
                });

            migrationBuilder.CreateTable(
                name: "TemplateLayoutOptions",
                columns: table => new
                {
                    TemplateLayoutOptionsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DefaultTemplateLayoutId = table.Column<int>(type: "int", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateLayoutOptions", x => x.TemplateLayoutOptionsId);
                    table.ForeignKey(
                        name: "FK_TemplateLayoutOptions_TemplateLayouts_DefaultTemplateLayoutId",
                        column: x => x.DefaultTemplateLayoutId,
                        principalTable: "TemplateLayouts",
                        principalColumn: "TemplateLayoutId");
                });

            migrationBuilder.CreateTable(
                name: "TemplateTypes",
                columns: table => new
                {
                    TemplateTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateTypeKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TemplateTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DefaultTemplate_Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultTemplate_Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultTemplate_ShortBody = table.Column<string>(type: "nvarchar(918)", maxLength: 918, nullable: true),
                    DefaultTemplate_AppBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersistType = table.Column<int>(type: "int", nullable: false),
                    NotificationTypes = table.Column<int>(type: "int", nullable: true),
                    DefaultPopupData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExampleData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JsonSchema = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TemplatePriority = table.Column<int>(type: "int", nullable: true),
                    TemplateLayoutId = table.Column<int>(type: "int", nullable: true),
                    SuppressLayout = table.Column<bool>(type: "bit", nullable: false),
                    ProcessorType = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateTypes", x => x.TemplateTypeId);
                    table.ForeignKey(
                        name: "FK_TemplateTypes_TemplateLayouts_TemplateLayoutId",
                        column: x => x.TemplateLayoutId,
                        principalTable: "TemplateLayouts",
                        principalColumn: "TemplateLayoutId");
                });

            migrationBuilder.CreateTable(
                name: "NotificationBatches",
                columns: table => new
                {
                    NotificationBatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    TemplateTypeId = table.Column<int>(type: "int", nullable: true),
                    Sender_DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Sender_EmailAddress = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    StaticMessage_Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StaticMessage_Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StaticMessage_ShortBody = table.Column<string>(type: "nvarchar(918)", maxLength: 918, nullable: true),
                    StaticMessage_AppBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Layout_OuterContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Layout_StyleSheetId = table.Column<int>(type: "int", nullable: true),
                    PopupData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Priority = table.Column<byte>(type: "tinyint", nullable: false),
                    Audit_CreatedBy = table.Column<int>(type: "int", nullable: true),
                    Audit_CreatedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    Audit_ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    Audit_ModifiedOn = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationBatches", x => x.NotificationBatchId);
                    table.ForeignKey(
                        name: "FK_NotificationBatches_TemplateStyleSheets_Layout_StyleSheetId",
                        column: x => x.Layout_StyleSheetId,
                        principalTable: "TemplateStyleSheets",
                        principalColumn: "StyleSheetId");
                    table.ForeignKey(
                        name: "FK_NotificationBatches_TemplateTypes_TemplateTypeId",
                        column: x => x.TemplateTypeId,
                        principalTable: "TemplateTypes",
                        principalColumn: "TemplateTypeId");
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    TemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateTypeId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CultureCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    TemplateDefinition_Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemplateDefinition_Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemplateDefinition_ShortBody = table.Column<string>(type: "nvarchar(918)", maxLength: 918, nullable: true),
                    TemplateDefinition_AppBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Audit_CreatedBy = table.Column<int>(type: "int", nullable: true),
                    Audit_CreatedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    Audit_ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    Audit_ModifiedOn = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.TemplateId);
                    table.ForeignKey(
                        name: "FK_Templates_TemplateTypes_TemplateTypeId",
                        column: x => x.TemplateTypeId,
                        principalTable: "TemplateTypes",
                        principalColumn: "TemplateTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemplateTypeRecordSettings",
                columns: table => new
                {
                    TemplateTypeRecordSettingsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateTypeId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    TemplateLayoutId = table.Column<int>(type: "int", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateTypeRecordSettings", x => x.TemplateTypeRecordSettingsId);
                    table.ForeignKey(
                        name: "FK_TemplateTypeRecordSettings_TemplateLayouts_TemplateLayoutId",
                        column: x => x.TemplateLayoutId,
                        principalTable: "TemplateLayouts",
                        principalColumn: "TemplateLayoutId");
                    table.ForeignKey(
                        name: "FK_TemplateTypeRecordSettings_TemplateTypes_TemplateTypeId",
                        column: x => x.TemplateTypeId,
                        principalTable: "TemplateTypes",
                        principalColumn: "TemplateTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemplateTypeSettings",
                columns: table => new
                {
                    TemplateTypeSettingsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateTypeId = table.Column<int>(type: "int", nullable: false),
                    SendType = table.Column<int>(type: "int", nullable: true),
                    TemplateLayoutId = table.Column<int>(type: "int", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateTypeSettings", x => x.TemplateTypeSettingsId);
                    table.ForeignKey(
                        name: "FK_TemplateTypeSettings_TemplateLayouts_TemplateLayoutId",
                        column: x => x.TemplateLayoutId,
                        principalTable: "TemplateLayouts",
                        principalColumn: "TemplateLayoutId");
                    table.ForeignKey(
                        name: "FK_TemplateTypeSettings_TemplateTypes_TemplateTypeId",
                        column: x => x.TemplateTypeId,
                        principalTable: "TemplateTypes",
                        principalColumn: "TemplateTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FileAttachments",
                columns: table => new
                {
                    FileAttachmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationBatchId = table.Column<int>(type: "int", nullable: false),
                    FileDescriptorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileAttachments", x => x.FileAttachmentId);
                    table.ForeignKey(
                        name: "FK_FileAttachments_FileDescriptors_FileDescriptorId",
                        column: x => x.FileDescriptorId,
                        principalTable: "FileDescriptors",
                        principalColumn: "FileDescriptorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FileAttachments_NotificationBatches_NotificationBatchId",
                        column: x => x.NotificationBatchId,
                        principalTable: "NotificationBatches",
                        principalColumn: "NotificationBatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationBatchId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<object>(type: "sql_variant", nullable: true),
                    Recipient_DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Recipient_EmailAddress = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    Recipient_CellNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Recipient_RecipientUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Recipient_CCEmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Recipient_BCCEmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message_Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message_Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message_ShortBody = table.Column<string>(type: "nvarchar(918)", maxLength: 918, nullable: true),
                    Message_AppBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PopupData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email_Identifier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email_Error = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email_SentDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Email_Status = table.Column<int>(type: "int", nullable: true),
                    Email_UsedOverrideRecipient = table.Column<bool>(type: "bit", nullable: true),
                    Sms_Identifier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Sms_Error = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sms_SentDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Sms_Status = table.Column<int>(type: "int", nullable: true),
                    Sms_UsedOverrideRecipient = table.Column<bool>(type: "bit", nullable: true),
                    PopupStatus = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "Email_Status_DeliveryStatus",
                        column: x => x.Email_Status,
                        principalTable: "DeliveryStatuses",
                        principalColumn: "DeliveryStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_NotificationBatches_NotificationBatchId",
                        column: x => x.NotificationBatchId,
                        principalTable: "NotificationBatches",
                        principalColumn: "NotificationBatchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "Notification_PopupStatus_DeliveryStatus",
                        column: x => x.PopupStatus,
                        principalTable: "DeliveryStatuses",
                        principalColumn: "DeliveryStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "Sms_Status_DeliveryStatus",
                        column: x => x.Sms_Status,
                        principalTable: "DeliveryStatuses",
                        principalColumn: "DeliveryStatusId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "BulkNotificationStatuses",
                columns: new[] { "BulkNotificationStatusId", "BulkNotificationStatusName" },
                values: new object[,]
                {
                    { 0, "None" },
                    { 1, "Pending" },
                    { 10, "Ready" },
                    { 20, "Scheduled" },
                    { 30, "Processing" },
                    { 35, "Sending" },
                    { 100, "Completed" },
                    { 500, "Failed Still Processing" },
                    { 501, "Failed" }
                });

            migrationBuilder.InsertData(
                table: "DeliveryStatuses",
                columns: new[] { "DeliveryStatusId", "DeliveryStatusName" },
                values: new object[,]
                {
                    { 1, "Unsent" },
                    { 20, "Queued" },
                    { 30, "Sent" },
                    { 40, "Delivered" },
                    { 50, "Not Available" },
                    { 60, "Failed" },
                    { 70, "Ignored" },
                    { 71, "Dont Send" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BulkNotifications_BulkNotificationTemplateId",
                table: "BulkNotifications",
                column: "BulkNotificationTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_BulkNotifications_Status",
                table: "BulkNotifications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BulkNotificationTemplates_TenantId",
                table: "BulkNotificationTemplates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomTemplateLayouts_StyleSheetId",
                table: "CustomTemplateLayouts",
                column: "StyleSheetId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomTemplateLayouts_TemplateLayoutId",
                table: "CustomTemplateLayouts",
                column: "TemplateLayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomTemplateLayouts_TenantId",
                table: "CustomTemplateLayouts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FileAttachments_FileDescriptorId",
                table: "FileAttachments",
                column: "FileDescriptorId");

            migrationBuilder.CreateIndex(
                name: "IX_FileAttachments_NotificationBatchId",
                table: "FileAttachments",
                column: "NotificationBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationBatches_Layout_StyleSheetId",
                table: "NotificationBatches",
                column: "Layout_StyleSheetId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationBatches_TemplateTypeId",
                table: "NotificationBatches",
                column: "TemplateTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationBatches_TenantId",
                table: "NotificationBatches",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Email_Status",
                table: "Notifications",
                column: "Email_Status",
                filter: "Email_Status = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_NotificationBatchId",
                table: "Notifications",
                column: "NotificationBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_PopupStatus",
                table: "Notifications",
                column: "PopupStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Sms_Status",
                table: "Notifications",
                column: "Sms_Status",
                filter: "Sms_Status = 1");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationSettings_TenantId",
                table: "NotificationSettings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateLayoutOptions_DefaultTemplateLayoutId",
                table: "TemplateLayoutOptions",
                column: "DefaultTemplateLayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateLayoutOptions_TenantId",
                table: "TemplateLayoutOptions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateLayouts_StyleSheetId",
                table: "TemplateLayouts",
                column: "StyleSheetId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateLayouts_TenantId",
                table: "TemplateLayouts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_TemplateTypeId_TenantId_CultureCode_RecordId",
                table: "Templates",
                columns: new[] { "TemplateTypeId", "TenantId", "CultureCode", "RecordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Templates_TenantId",
                table: "Templates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateStyleSheets_TenantId",
                table: "TemplateStyleSheets",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateTypeRecordSettings_TemplateLayoutId",
                table: "TemplateTypeRecordSettings",
                column: "TemplateLayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateTypeRecordSettings_TemplateTypeId",
                table: "TemplateTypeRecordSettings",
                column: "TemplateTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateTypeRecordSettings_TenantId",
                table: "TemplateTypeRecordSettings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateTypes_TemplateLayoutId",
                table: "TemplateTypes",
                column: "TemplateLayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateTypes_TemplateTypeKey",
                table: "TemplateTypes",
                column: "TemplateTypeKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TemplateTypeSettings_TemplateLayoutId",
                table: "TemplateTypeSettings",
                column: "TemplateLayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateTypeSettings_TemplateTypeId",
                table: "TemplateTypeSettings",
                column: "TemplateTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateTypeSettings_TenantId",
                table: "TemplateTypeSettings",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BulkNotifications");

            migrationBuilder.DropTable(
                name: "CustomTemplateLayouts");

            migrationBuilder.DropTable(
                name: "FileAttachments");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "NotificationSettings");

            migrationBuilder.DropTable(
                name: "TemplateLayoutOptions");

            migrationBuilder.DropTable(
                name: "Templates");

            migrationBuilder.DropTable(
                name: "TemplateTypeRecordSettings");

            migrationBuilder.DropTable(
                name: "TemplateTypeSettings");

            migrationBuilder.DropTable(
                name: "BulkNotificationStatuses");

            migrationBuilder.DropTable(
                name: "BulkNotificationTemplates");

            migrationBuilder.DropTable(
                name: "FileDescriptors");

            migrationBuilder.DropTable(
                name: "DeliveryStatuses");

            migrationBuilder.DropTable(
                name: "NotificationBatches");

            migrationBuilder.DropTable(
                name: "TemplateTypes");

            migrationBuilder.DropTable(
                name: "TemplateLayouts");

            migrationBuilder.DropTable(
                name: "TemplateStyleSheets");
        }
    }
}
