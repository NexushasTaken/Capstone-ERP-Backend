using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class slimbasemodeldropsales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sales");

            migrationBuilder.DropColumn(
                name: "Created_At",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "Created_By",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "Deleted_At",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "Deleted_By",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "Updated_At",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "Updated_By",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "Created_At",
                table: "OrderTypes");

            migrationBuilder.DropColumn(
                name: "Created_By",
                table: "OrderTypes");

            migrationBuilder.DropColumn(
                name: "Deleted_At",
                table: "OrderTypes");

            migrationBuilder.DropColumn(
                name: "Deleted_By",
                table: "OrderTypes");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "OrderTypes");

            migrationBuilder.DropColumn(
                name: "Updated_At",
                table: "OrderTypes");

            migrationBuilder.DropColumn(
                name: "Updated_By",
                table: "OrderTypes");

            migrationBuilder.DropColumn(
                name: "Created_At",
                table: "OrderStatuses");

            migrationBuilder.DropColumn(
                name: "Created_By",
                table: "OrderStatuses");

            migrationBuilder.DropColumn(
                name: "Deleted_At",
                table: "OrderStatuses");

            migrationBuilder.DropColumn(
                name: "Deleted_By",
                table: "OrderStatuses");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "OrderStatuses");

            migrationBuilder.DropColumn(
                name: "Updated_At",
                table: "OrderStatuses");

            migrationBuilder.DropColumn(
                name: "Updated_By",
                table: "OrderStatuses");

            migrationBuilder.DropColumn(
                name: "Deleted_At",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "Deleted_By",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "Updated_At",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "Updated_By",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "Created_At",
                table: "InventoryStatuses");

            migrationBuilder.DropColumn(
                name: "Created_By",
                table: "InventoryStatuses");

            migrationBuilder.DropColumn(
                name: "Deleted_At",
                table: "InventoryStatuses");

            migrationBuilder.DropColumn(
                name: "Deleted_By",
                table: "InventoryStatuses");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "InventoryStatuses");

            migrationBuilder.DropColumn(
                name: "Updated_At",
                table: "InventoryStatuses");

            migrationBuilder.DropColumn(
                name: "Updated_By",
                table: "InventoryStatuses");

            migrationBuilder.DropColumn(
                name: "Created_At",
                table: "InventoryLabels");

            migrationBuilder.DropColumn(
                name: "Created_By",
                table: "InventoryLabels");

            migrationBuilder.DropColumn(
                name: "Deleted_At",
                table: "InventoryLabels");

            migrationBuilder.DropColumn(
                name: "Deleted_By",
                table: "InventoryLabels");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "InventoryLabels");

            migrationBuilder.DropColumn(
                name: "Updated_At",
                table: "InventoryLabels");

            migrationBuilder.DropColumn(
                name: "Updated_By",
                table: "InventoryLabels");

            migrationBuilder.DropColumn(
                name: "Created_At",
                table: "ForecastWeeks");

            migrationBuilder.DropColumn(
                name: "Created_By",
                table: "ForecastWeeks");

            migrationBuilder.DropColumn(
                name: "Deleted_At",
                table: "ForecastWeeks");

            migrationBuilder.DropColumn(
                name: "Deleted_By",
                table: "ForecastWeeks");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ForecastWeeks");

            migrationBuilder.DropColumn(
                name: "Updated_At",
                table: "ForecastWeeks");

            migrationBuilder.DropColumn(
                name: "Updated_By",
                table: "ForecastWeeks");

            migrationBuilder.DropColumn(
                name: "Created_By",
                table: "ForecastResults");

            migrationBuilder.DropColumn(
                name: "Deleted_At",
                table: "ForecastResults");

            migrationBuilder.DropColumn(
                name: "Deleted_By",
                table: "ForecastResults");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ForecastResults");

            migrationBuilder.DropColumn(
                name: "Updated_At",
                table: "ForecastResults");

            migrationBuilder.DropColumn(
                name: "Updated_By",
                table: "ForecastResults");

            migrationBuilder.DropColumn(
                name: "Deleted_At",
                table: "DamagedInventories");

            migrationBuilder.DropColumn(
                name: "Deleted_By",
                table: "DamagedInventories");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "DamagedInventories");

            migrationBuilder.DropColumn(
                name: "Updated_At",
                table: "DamagedInventories");

            migrationBuilder.DropColumn(
                name: "Updated_By",
                table: "DamagedInventories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Created_At",
                table: "UserRoles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Created_By",
                table: "UserRoles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Deleted_At",
                table: "UserRoles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Deleted_By",
                table: "UserRoles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "UserRoles",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated_At",
                table: "UserRoles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Updated_By",
                table: "UserRoles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created_At",
                table: "OrderTypes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Created_By",
                table: "OrderTypes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Deleted_At",
                table: "OrderTypes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Deleted_By",
                table: "OrderTypes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "OrderTypes",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated_At",
                table: "OrderTypes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Updated_By",
                table: "OrderTypes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created_At",
                table: "OrderStatuses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Created_By",
                table: "OrderStatuses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Deleted_At",
                table: "OrderStatuses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Deleted_By",
                table: "OrderStatuses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "OrderStatuses",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated_At",
                table: "OrderStatuses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Updated_By",
                table: "OrderStatuses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Deleted_At",
                table: "InventoryTransactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Deleted_By",
                table: "InventoryTransactions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "InventoryTransactions",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated_At",
                table: "InventoryTransactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Updated_By",
                table: "InventoryTransactions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created_At",
                table: "InventoryStatuses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Created_By",
                table: "InventoryStatuses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Deleted_At",
                table: "InventoryStatuses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Deleted_By",
                table: "InventoryStatuses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "InventoryStatuses",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated_At",
                table: "InventoryStatuses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Updated_By",
                table: "InventoryStatuses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created_At",
                table: "InventoryLabels",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Created_By",
                table: "InventoryLabels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Deleted_At",
                table: "InventoryLabels",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Deleted_By",
                table: "InventoryLabels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "InventoryLabels",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated_At",
                table: "InventoryLabels",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Updated_By",
                table: "InventoryLabels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created_At",
                table: "ForecastWeeks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Created_By",
                table: "ForecastWeeks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Deleted_At",
                table: "ForecastWeeks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Deleted_By",
                table: "ForecastWeeks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ForecastWeeks",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated_At",
                table: "ForecastWeeks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Updated_By",
                table: "ForecastWeeks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Created_By",
                table: "ForecastResults",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Deleted_At",
                table: "ForecastResults",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Deleted_By",
                table: "ForecastResults",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ForecastResults",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated_At",
                table: "ForecastResults",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Updated_By",
                table: "ForecastResults",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Deleted_At",
                table: "DamagedInventories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Deleted_By",
                table: "DamagedInventories",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "DamagedInventories",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated_At",
                table: "DamagedInventories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Updated_By",
                table: "DamagedInventories",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Created_By = table.Column<int>(type: "integer", nullable: true),
                    Deleted_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted_By = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    Updated_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Updated_By = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sales_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "InventoryLabels",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created_At", "Created_By", "Deleted_At", "Deleted_By", "IsActive", "Updated_At", "Updated_By" },
                values: new object[] { new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, null, null });

            migrationBuilder.UpdateData(
                table: "InventoryLabels",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created_At", "Created_By", "Deleted_At", "Deleted_By", "IsActive", "Updated_At", "Updated_By" },
                values: new object[] { new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, null, null });

            migrationBuilder.UpdateData(
                table: "InventoryLabels",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Created_At", "Created_By", "Deleted_At", "Deleted_By", "IsActive", "Updated_At", "Updated_By" },
                values: new object[] { new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, null, null });

            migrationBuilder.UpdateData(
                table: "InventoryLabels",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Created_At", "Created_By", "Deleted_At", "Deleted_By", "IsActive", "Updated_At", "Updated_By" },
                values: new object[] { new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, null, null });

            migrationBuilder.UpdateData(
                table: "InventoryStatuses",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created_At", "Created_By", "Deleted_At", "Deleted_By", "IsActive", "Updated_At", "Updated_By" },
                values: new object[] { new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, null, null });

            migrationBuilder.UpdateData(
                table: "InventoryStatuses",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created_At", "Created_By", "Deleted_At", "Deleted_By", "IsActive", "Updated_At", "Updated_By" },
                values: new object[] { new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, null, null });

            migrationBuilder.UpdateData(
                table: "InventoryStatuses",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Created_At", "Created_By", "Deleted_At", "Deleted_By", "IsActive", "Updated_At", "Updated_By" },
                values: new object[] { new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, null, null });

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created_At", "Created_By", "Deleted_At", "Deleted_By", "IsActive", "Updated_At", "Updated_By" },
                values: new object[] { new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, null, null });

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created_At", "Created_By", "Deleted_At", "Deleted_By", "IsActive", "Updated_At", "Updated_By" },
                values: new object[] { new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, null, null });

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Created_At", "Created_By", "Deleted_At", "Deleted_By", "IsActive", "Updated_At", "Updated_By" },
                values: new object[] { new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, null, null });

            migrationBuilder.UpdateData(
                table: "OrderStatuses",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Created_At", "Created_By", "Deleted_At", "Deleted_By", "IsActive", "Updated_At", "Updated_By" },
                values: new object[] { new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, null, null });

            migrationBuilder.UpdateData(
                table: "OrderTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created_At", "Created_By", "Deleted_At", "Deleted_By", "IsActive", "Updated_At", "Updated_By" },
                values: new object[] { new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, null, null });

            migrationBuilder.UpdateData(
                table: "OrderTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created_At", "Created_By", "Deleted_At", "Deleted_By", "IsActive", "Updated_At", "Updated_By" },
                values: new object[] { new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, null, null });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created_At", "Created_By", "Deleted_At", "Deleted_By", "IsActive", "Updated_At", "Updated_By" },
                values: new object[] { new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, null, null });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created_At", "Created_By", "Deleted_At", "Deleted_By", "IsActive", "Updated_At", "Updated_By" },
                values: new object[] { new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Sales_OrderId",
                table: "Sales",
                column: "OrderId");
        }
    }
}
