using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class seedlookuptables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "InventoryLabels",
                columns: new[] { "Id", "Created_At", "Created_By", "Deleted_At", "Deleted_By", "IsActive", "Type", "Updated_At", "Updated_By" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, "purchase", null, null },
                    { 2, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, "return", null, null },
                    { 3, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, "restock", null, null },
                    { 4, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, "damage", null, null }
                });

            migrationBuilder.InsertData(
                table: "InventoryStatuses",
                columns: new[] { "Id", "Created_At", "Created_By", "Deleted_At", "Deleted_By", "IsActive", "Status", "Updated_At", "Updated_By" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, "available", null, null },
                    { 2, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, "low stock", null, null },
                    { 3, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, "critical", null, null }
                });

            migrationBuilder.InsertData(
                table: "OrderStatuses",
                columns: new[] { "Id", "Created_At", "Created_By", "Deleted_At", "Deleted_By", "IsActive", "Status", "Updated_At", "Updated_By" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, "completed", null, null },
                    { 2, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, "processing", null, null },
                    { 3, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, "shipped", null, null },
                    { 4, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, "cancelled", null, null }
                });

            migrationBuilder.InsertData(
                table: "OrderTypes",
                columns: new[] { "Id", "Created_At", "Created_By", "Deleted_At", "Deleted_By", "IsActive", "Type", "Updated_At", "Updated_By" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, "walkin", null, null },
                    { 2, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, "delivery", null, null }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "Created_At", "Created_By", "Deleted_At", "Deleted_By", "IsActive", "Role", "Updated_At", "Updated_By" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, "owner", null, null },
                    { 2, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, "secretary", null, null }
                });

            // Seeded rows use explicit ids, so move each identity sequence past them
            foreach (var table in new[] { "InventoryLabels", "InventoryStatuses", "OrderStatuses", "OrderTypes", "UserRoles" })
            {
                migrationBuilder.Sql($@"SELECT setval(pg_get_serial_sequence('""{table}""', 'Id'), (SELECT MAX(""Id"") FROM ""{table}""));");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InventoryLabels",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "InventoryLabels",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "InventoryLabels",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "InventoryLabels",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "InventoryStatuses",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "InventoryStatuses",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "InventoryStatuses",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "OrderTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "OrderTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
