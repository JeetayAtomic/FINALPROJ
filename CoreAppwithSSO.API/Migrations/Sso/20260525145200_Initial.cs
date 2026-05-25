using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CoreAppwithSSO.API.Migrations.Sso
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BaseUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IconName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IconColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SsoTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SsoTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SsoTokens_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantApplications",
                columns: table => new
                {
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantApplications", x => new { x.TenantId, x.ApplicationId });
                    table.ForeignKey(
                        name: "FK_TenantApplications_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    SsoTokenId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSessions_SsoTokens_SsoTokenId",
                        column: x => x.SsoTokenId,
                        principalTable: "SsoTokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Applications",
                columns: new[] { "Id", "BaseUrl", "CreatedAt", "Description", "DisplayOrder", "IconColor", "IconName", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "https://hr.yourcompany.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Human Resources Management", 1, "#4285F4", "people", true, "HR Portal" },
                    { 2, "https://finance.yourcompany.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Financial Management System", 2, "#EA4335", "account_balance", true, "Finance" },
                    { 3, "https://projects.yourcompany.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Project Management Tool", 3, "#FBBC05", "assignment", true, "Project Mgmt" },
                    { 4, "https://crm.yourcompany.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Customer Relationship Management", 4, "#34A853", "contacts", true, "CRM" },
                    { 5, "https://inventory.yourcompany.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Inventory Management System", 5, "#FF6D01", "inventory_2", true, "Inventory" },
                    { 6, "https://reports.yourcompany.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Business Intelligence & Reports", 6, "#46BDC6", "analytics", true, "Reports" },
                    { 7, "https://mail.yourcompany.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Corporate Email System", 7, "#7B1FA2", "email", true, "Email" },
                    { 8, "https://calendar.yourcompany.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Team Calendar & Scheduling", 8, "#0097A7", "calendar_month", true, "Calendar" },
                    { 9, "https://admin.yourcompany.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System Administration", 9, "#616161", "admin_panel_settings", true, "Admin" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_Name",
                table: "Applications",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SsoTokens_ApplicationId",
                table: "SsoTokens",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_SsoTokens_Token",
                table: "SsoTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantApplications_ApplicationId",
                table: "TenantApplications",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_SsoTokenId",
                table: "UserSessions",
                column: "SsoTokenId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId_TenantId",
                table: "UserSessions",
                columns: new[] { "UserId", "TenantId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantApplications");

            migrationBuilder.DropTable(
                name: "UserSessions");

            migrationBuilder.DropTable(
                name: "SsoTokens");

            migrationBuilder.DropTable(
                name: "Applications");
        }
    }
}
