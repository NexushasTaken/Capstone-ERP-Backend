using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class removewarehouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Warehouses_WarehouseId",
                table: "Inventories");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_ProductId_WarehouseId",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_WarehouseId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "PickUpAddress",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "Inventories");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_ProductId",
                table: "Inventories",
                column: "ProductId",
                unique: true,
                filter: "\"IsActive\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Inventories_ProductId",
                table: "Inventories");

            migrationBuilder.AddColumn<string>(
                name: "PickUpAddress",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "Inventories",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Created_By = table.Column<int>(type: "integer", nullable: true),
                    Deleted_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted_By = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Updated_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Updated_By = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_ProductId_WarehouseId",
                table: "Inventories",
                columns: new[] { "ProductId", "WarehouseId" },
                unique: true,
                filter: "\"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_WarehouseId",
                table: "Inventories",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Warehouses_WarehouseId",
                table: "Inventories",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
