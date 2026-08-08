using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class initialinventoryschema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventoryStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Status = table.Column<string>(type: "text", nullable: true),
                    Created_By = table.Column<int>(type: "integer", nullable: true),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Updated_By = table.Column<int>(type: "integer", nullable: true),
                    Updated_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted_By = table.Column<int>(type: "integer", nullable: true),
                    Deleted_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Created_By = table.Column<int>(type: "integer", nullable: true),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Updated_By = table.Column<int>(type: "integer", nullable: true),
                    Updated_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted_By = table.Column<int>(type: "integer", nullable: true),
                    Deleted_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouse", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    WarehouseId = table.Column<int>(type: "integer", nullable: false),
                    DateArrived = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created_By = table.Column<int>(type: "integer", nullable: true),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Updated_By = table.Column<int>(type: "integer", nullable: true),
                    Updated_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted_By = table.Column<int>(type: "integer", nullable: true),
                    Deleted_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventory_InventoryStatus_StatusId",
                        column: x => x.StatusId,
                        principalTable: "InventoryStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Inventory_Warehouse_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransaction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryId = table.Column<int>(type: "integer", nullable: false),
                    QuantityChanged = table.Column<int>(type: "integer", nullable: false),
                    Created_By = table.Column<int>(type: "integer", nullable: true),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Updated_By = table.Column<int>(type: "integer", nullable: true),
                    Updated_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted_By = table.Column<int>(type: "integer", nullable: true),
                    Deleted_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryTransaction_Inventory_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_StatusId",
                table: "Inventory",
                column: "StatusId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_WarehouseId",
                table: "Inventory",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransaction_InventoryId",
                table: "InventoryTransaction",
                column: "InventoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryTransaction");

            migrationBuilder.DropTable(
                name: "Inventory");

            migrationBuilder.DropTable(
                name: "InventoryStatus");

            migrationBuilder.DropTable(
                name: "Warehouse");
        }
    }
}
