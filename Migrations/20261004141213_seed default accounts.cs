using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class seeddefaultaccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "UserAccounts",
                columns: new[] { "Id", "Created_At", "Created_By", "Deleted_At", "Deleted_By", "Email", "IsActive", "Password", "Salt", "Updated_At", "Updated_By", "UserRoleId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "owner@gmail.com", true, "0ULmzYhoKo1MJ9WyeS3WAktEqN/QYeXPEb7z7akCUxM=", "OmUBGSIov6YDrqLo4SaFuw==", null, null, 1 },
                    { 2, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "secretary@gmail.com", true, "lgKIp/sJWDvi74qQ/TYwXAsb7Bv/Z1fpdrDxsanRmqA=", "ELFJLUKQLNZ1usWM/oILqA==", null, null, 2 }
                });

            migrationBuilder.InsertData(
                table: "UserInformations",
                columns: new[] { "Id", "Created_At", "Created_By", "Deleted_At", "Deleted_By", "FirstName", "IsActive", "LastName", "Updated_At", "Updated_By", "UserAccountId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Bill", true, "Gates", null, null, 1 },
                    { 2, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Mark", true, "Zucherbeard", null, null, 2 }
                });

            // Seeded rows use explicit ids, so move each identity sequence past them
            foreach (var table in new[] { "UserAccounts", "UserInformations" })
            {
                migrationBuilder.Sql($@"SELECT setval(pg_get_serial_sequence('""{table}""', 'Id'), (SELECT MAX(""Id"") FROM ""{table}""));");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserInformations",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "UserInformations",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
