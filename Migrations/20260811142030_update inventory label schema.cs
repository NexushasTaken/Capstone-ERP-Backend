using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class updateinventorylabelschema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Label",
                table: "InventoryTransaction");

            migrationBuilder.AddColumn<int>(
                name: "InventoryLabelId",
                table: "InventoryTransaction",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReorderPoint",
                table: "Inventory",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DamagedInventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryId = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_DamagedInventory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DamagedInventory_Inventory_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryLabel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_InventoryLabel", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransaction_InventoryLabelId",
                table: "InventoryTransaction",
                column: "InventoryLabelId");

            migrationBuilder.CreateIndex(
                name: "IX_DamagedInventory_InventoryId",
                table: "DamagedInventory",
                column: "InventoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransaction_InventoryLabel_InventoryLabelId",
                table: "InventoryTransaction",
                column: "InventoryLabelId",
                principalTable: "InventoryLabel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransaction_InventoryLabel_InventoryLabelId",
                table: "InventoryTransaction");

            migrationBuilder.DropTable(
                name: "DamagedInventory");

            migrationBuilder.DropTable(
                name: "InventoryLabel");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransaction_InventoryLabelId",
                table: "InventoryTransaction");

            migrationBuilder.DropColumn(
                name: "InventoryLabelId",
                table: "InventoryTransaction");

            migrationBuilder.DropColumn(
                name: "ReorderPoint",
                table: "Inventory");

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "InventoryTransaction",
                type: "text",
                nullable: true);
        }
    }
}
