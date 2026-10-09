using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class forecastperproduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Old per-stock-record results can't be converted; the next run remakes them.
            migrationBuilder.Sql("DELETE FROM \"ForecastResults\";");

            migrationBuilder.DropForeignKey(
                name: "FK_ForecastResults_Inventories_InventoryId",
                table: "ForecastResults");

            migrationBuilder.DropIndex(
                name: "IX_ForecastResults_InventoryId",
                table: "ForecastResults");

            migrationBuilder.DropColumn(
                name: "EarliestStockOutDay",
                table: "ForecastResults");

            migrationBuilder.DropColumn(
                name: "InventoryId",
                table: "ForecastResults");

            migrationBuilder.AddColumn<int>(
                name: "SuggestedOrder",
                table: "ForecastResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "AiAbsError",
                table: "ForecastResults",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "BacktestSold",
                table: "ForecastResults",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "BaselineAbsError",
                table: "ForecastResults",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "BusyDemand",
                table: "ForecastResults",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ExpectedDemand",
                table: "ForecastResults",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "HistoryWeeks",
                table: "ForecastResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "LowDemand",
                table: "ForecastResults",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "Method",
                table: "ForecastResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "ForecastResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "RunsOutAround",
                table: "ForecastResults",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StockOnHand",
                table: "ForecastResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "WeeksLeft",
                table: "ForecastResults",
                type: "double precision",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ForecastWeeks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ForecastResultId = table.Column<int>(type: "integer", nullable: false),
                    WeekStart = table.Column<DateOnly>(type: "date", nullable: false),
                    Low = table.Column<double>(type: "double precision", nullable: false),
                    Expected = table.Column<double>(type: "double precision", nullable: false),
                    BusyCase = table.Column<double>(type: "double precision", nullable: false),
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
                    table.PrimaryKey("PK_ForecastWeeks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForecastWeeks_ForecastResults_ForecastResultId",
                        column: x => x.ForecastResultId,
                        principalTable: "ForecastResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ForecastResults_ProductId",
                table: "ForecastResults",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastWeeks_ForecastResultId",
                table: "ForecastWeeks",
                column: "ForecastResultId");

            migrationBuilder.AddForeignKey(
                name: "FK_ForecastResults_Products_ProductId",
                table: "ForecastResults",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"ForecastResults\";");

            migrationBuilder.DropForeignKey(
                name: "FK_ForecastResults_Products_ProductId",
                table: "ForecastResults");

            migrationBuilder.DropTable(
                name: "ForecastWeeks");

            migrationBuilder.DropIndex(
                name: "IX_ForecastResults_ProductId",
                table: "ForecastResults");

            migrationBuilder.DropColumn(
                name: "AiAbsError",
                table: "ForecastResults");

            migrationBuilder.DropColumn(
                name: "BacktestSold",
                table: "ForecastResults");

            migrationBuilder.DropColumn(
                name: "BaselineAbsError",
                table: "ForecastResults");

            migrationBuilder.DropColumn(
                name: "BusyDemand",
                table: "ForecastResults");

            migrationBuilder.DropColumn(
                name: "ExpectedDemand",
                table: "ForecastResults");

            migrationBuilder.DropColumn(
                name: "HistoryWeeks",
                table: "ForecastResults");

            migrationBuilder.DropColumn(
                name: "LowDemand",
                table: "ForecastResults");

            migrationBuilder.DropColumn(
                name: "Method",
                table: "ForecastResults");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "ForecastResults");

            migrationBuilder.DropColumn(
                name: "RunsOutAround",
                table: "ForecastResults");

            migrationBuilder.DropColumn(
                name: "StockOnHand",
                table: "ForecastResults");

            migrationBuilder.DropColumn(
                name: "WeeksLeft",
                table: "ForecastResults");

            migrationBuilder.DropColumn(
                name: "SuggestedOrder",
                table: "ForecastResults");

            migrationBuilder.AddColumn<int>(
                name: "InventoryId",
                table: "ForecastResults",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EarliestStockOutDay",
                table: "ForecastResults",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForecastResults_InventoryId",
                table: "ForecastResults",
                column: "InventoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ForecastResults_Inventories_InventoryId",
                table: "ForecastResults",
                column: "InventoryId",
                principalTable: "Inventories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
