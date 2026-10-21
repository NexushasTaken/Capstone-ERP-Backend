using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class stockadjustments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockAdjustments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryId = table.Column<int>(type: "integer", nullable: false),
                    ReasonId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    SaleReturnId = table.Column<int>(type: "integer", nullable: true),
                    Created_By = table.Column<int>(type: "integer", nullable: true),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockAdjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockAdjustments_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockAdjustments_SaleReturns_SaleReturnId",
                        column: x => x.SaleReturnId,
                        principalTable: "SaleReturns",
                        principalColumn: "Id");
                });

            // Keep existing damage reports as damaged adjustments, linked to the damaged return written at the
            // same moment when there is one, and move their ledger rows to the adjustment label.
            migrationBuilder.Sql(
                @"INSERT INTO ""StockAdjustments"" (""InventoryId"", ""ReasonId"", ""Quantity"", ""Note"", ""SaleReturnId"", ""Created_By"", ""Created_At"")
                  SELECT d.""InventoryId"", 1, -d.""Quantity"", d.""Reason"",
                         (SELECT r.""Id"" FROM ""SaleReturns"" r
                          WHERE r.""InventoryId"" = d.""InventoryId"" AND r.""Created_At"" = d.""Created_At""
                            AND r.""ConditionId"" = 2 AND r.""Quantity"" = d.""Quantity""
                          ORDER BY r.""Id"" LIMIT 1),
                         d.""Created_By"", d.""Created_At""
                  FROM ""DamagedInventories"" d
                  ORDER BY d.""Id"";
                  UPDATE ""InventoryTransactions"" SET ""InventoryLabelId"" = 3 WHERE ""InventoryLabelId"" = 4;"
            );

            migrationBuilder.DropTable(
                name: "DamagedInventories");

            migrationBuilder.DeleteData(
                table: "InventoryLabels",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "InventoryLabels",
                keyColumn: "Id",
                keyValue: 1,
                column: "Type",
                value: "sale");

            migrationBuilder.UpdateData(
                table: "InventoryLabels",
                keyColumn: "Id",
                keyValue: 3,
                column: "Type",
                value: "adjustment");

            migrationBuilder.InsertData(
                table: "InventoryLabels",
                columns: new[] { "Id", "Type" },
                values: new object[] { 5, "received" });

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustments_Created_At",
                table: "StockAdjustments",
                column: "Created_At");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustments_InventoryId",
                table: "StockAdjustments",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustments_SaleReturnId",
                table: "StockAdjustments",
                column: "SaleReturnId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockAdjustments");

            migrationBuilder.DeleteData(
                table: "InventoryLabels",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.CreateTable(
                name: "DamagedInventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryId = table.Column<int>(type: "integer", nullable: false),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Created_By = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DamagedInventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DamagedInventories_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "InventoryLabels",
                keyColumn: "Id",
                keyValue: 1,
                column: "Type",
                value: "purchase");

            migrationBuilder.UpdateData(
                table: "InventoryLabels",
                keyColumn: "Id",
                keyValue: 3,
                column: "Type",
                value: "restock");

            migrationBuilder.InsertData(
                table: "InventoryLabels",
                columns: new[] { "Id", "Type" },
                values: new object[] { 4, "damage" });

            migrationBuilder.CreateIndex(
                name: "IX_DamagedInventories_InventoryId",
                table: "DamagedInventories",
                column: "InventoryId");
        }
    }
}
