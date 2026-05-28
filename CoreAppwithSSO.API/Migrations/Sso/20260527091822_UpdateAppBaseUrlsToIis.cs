using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreAppwithSSO.API.Migrations.Sso
{
    /// <inheritdoc />
    public partial class UpdateAppBaseUrlsToIis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Applications",
                keyColumn: "Id",
                keyValue: 1,
                column: "BaseUrl",
                value: "http://localhost:8081/hr/");

            migrationBuilder.UpdateData(
                table: "Applications",
                keyColumn: "Id",
                keyValue: 2,
                column: "BaseUrl",
                value: "http://localhost:8081/finance/");

            migrationBuilder.UpdateData(
                table: "Applications",
                keyColumn: "Id",
                keyValue: 5,
                column: "BaseUrl",
                value: "http://localhost:8081/inventory/");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Applications",
                keyColumn: "Id",
                keyValue: 1,
                column: "BaseUrl",
                value: "https://hr.yourcompany.com");

            migrationBuilder.UpdateData(
                table: "Applications",
                keyColumn: "Id",
                keyValue: 2,
                column: "BaseUrl",
                value: "https://finance.yourcompany.com");

            migrationBuilder.UpdateData(
                table: "Applications",
                keyColumn: "Id",
                keyValue: 5,
                column: "BaseUrl",
                value: "https://inventory.yourcompany.com");
        }
    }
}
