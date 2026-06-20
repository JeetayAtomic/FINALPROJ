using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreAppwithSSO.API.Migrations.Sso
{
    /// <inheritdoc />
    public partial class RemoveSeededApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove the example/placeholder catalog rows. The Application catalog is
            // now populated at runtime via POST /api/admin/applications.
            migrationBuilder.DeleteData(
                table: "Applications",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Applications",
                columns: new[] { "Id", "BaseUrl", "CreatedAt", "Description", "DisplayOrder", "IconColor", "IconName", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "http://localhost:8081/hr/", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Human Resources Management", 1, "#4285F4", "people", true, "HR Portal" },
                    { 2, "http://localhost:8081/finance/", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Financial Management System", 2, "#EA4335", "account_balance", true, "Finance" },
                    { 3, "https://projects.yourcompany.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Project Management Tool", 3, "#FBBC05", "assignment", true, "Project Mgmt" },
                    { 4, "https://crm.yourcompany.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Customer Relationship Management", 4, "#34A853", "contacts", true, "CRM" },
                    { 5, "http://localhost:8081/inventory/", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Inventory Management System", 5, "#FF6D01", "inventory_2", true, "Inventory" },
                    { 6, "https://reports.yourcompany.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Business Intelligence & Reports", 6, "#46BDC6", "analytics", true, "Reports" },
                    { 7, "https://mail.yourcompany.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Corporate Email System", 7, "#7B1FA2", "email", true, "Email" },
                    { 8, "https://calendar.yourcompany.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Team Calendar & Scheduling", 8, "#0097A7", "calendar_month", true, "Calendar" },
                    { 9, "https://admin.yourcompany.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System Administration", 9, "#616161", "admin_panel_settings", true, "Admin" }
                });
        }
    }
}
