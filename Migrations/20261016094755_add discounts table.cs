using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class adddiscountstable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiscountId",
                table: "Orders",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "Discounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "text", nullable: true),
                    Percent = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discounts", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Discounts",
                columns: new[] { "Id", "Percent", "Type" },
                values: new object[,]
                {
                    { 1, 0m, "none" },
                    { 2, 20m, "senior" },
                    { 3, 20m, "pwd" },
                    { 4, 10m, "solo_parent" },
                    { 5, null, "custom" }
                });

            // Carry each order's discount type over to the matching Discounts row before the old column goes.
            migrationBuilder.Sql(
                @"UPDATE ""Orders"" o SET ""DiscountId"" = d.""Id"" FROM ""Discounts"" d WHERE d.""Type"" = o.""DiscountType"";"
            );

            migrationBuilder.DropColumn(
                name: "DiscountType",
                table: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DiscountId",
                table: "Orders",
                column: "DiscountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Discounts_DiscountId",
                table: "Orders",
                column: "DiscountId",
                principalTable: "Discounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiscountType",
                table: "Orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "none");

            migrationBuilder.Sql(
                @"UPDATE ""Orders"" o SET ""DiscountType"" = d.""Type"" FROM ""Discounts"" d WHERE d.""Id"" = o.""DiscountId"";"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Discounts_DiscountId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "Discounts");

            migrationBuilder.DropIndex(
                name: "IX_Orders_DiscountId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DiscountId",
                table: "Orders");
        }
    }
}
