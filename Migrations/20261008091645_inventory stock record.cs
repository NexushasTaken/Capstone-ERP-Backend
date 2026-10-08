using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class inventorystockrecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Inventories_DateArrived",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_ProductId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "DateArrived",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Inventories");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_ProductId_WarehouseId",
                table: "Inventories",
                columns: new[] { "ProductId", "WarehouseId" },
                unique: true,
                filter: "\"IsActive\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Inventories_ProductId_WarehouseId",
                table: "Inventories");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateArrived",
                table: "Inventories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_DateArrived",
                table: "Inventories",
                column: "DateArrived");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_ProductId",
                table: "Inventories",
                column: "ProductId");
        }
    }
}
