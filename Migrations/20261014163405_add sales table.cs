using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class addsalestable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    Total = table.Column<decimal>(type: "numeric", nullable: false),
                    Created_By = table.Column<int>(type: "integer", nullable: true),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_Sales_Created_At",
                table: "Sales",
                column: "Created_At");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_OrderId",
                table: "Sales",
                column: "OrderId",
                unique: true);

            // One sale per order that is already completed. Completed is a terminal status, so the order's last
            // update is when it was completed. ROUND matches OrderMath.DiscountAmount (half away from zero).
            migrationBuilder.Sql(
                """
                INSERT INTO "Sales" ("OrderId", "Total", "Created_By", "Created_At")
                SELECT o."Id",
                       t.sub - ROUND(t.sub * o."DiscountPercent" / 100, 2),
                       COALESCE(o."Updated_By", o."Created_By"),
                       COALESCE(o."Updated_At", o."Created_At")
                FROM "Orders" o
                JOIN (SELECT "OrderId", SUM("Amount") AS sub FROM "OrderLines" GROUP BY "OrderId") t
                  ON t."OrderId" = o."Id"
                WHERE o."OrderStatusId" = 1
                ORDER BY COALESCE(o."Updated_At", o."Created_At"), o."Id";
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sales");
        }
    }
}
