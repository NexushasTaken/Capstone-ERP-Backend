using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class addunits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Created_By = table.Column<int>(type: "integer", nullable: true),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Updated_By = table.Column<int>(type: "integer", nullable: true),
                    Updated_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted_By = table.Column<int>(type: "integer", nullable: true),
                    Deleted_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Units",
                columns: new[] { "Id", "Created_At", "Created_By", "Deleted_At", "Deleted_By", "IsActive", "Name", "Updated_At", "Updated_By" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, "pc", null, null },
                    { 2, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, "box", null, null },
                    { 3, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, "pack", null, null },
                    { 4, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, "bar", null, null },
                    { 5, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, "roll", null, null },
                    { 6, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, "set", null, null },
                    { 7, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, "pair", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_UnitId",
                table: "Products",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Units_UnitId",
                table: "Products",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // HasData set the ids, so move the sequence past them or the first "add unit" hits a duplicate key.
            migrationBuilder.Sql(
                @"SELECT setval(pg_get_serial_sequence('""Units""', 'Id'), (SELECT MAX(""Id"") FROM ""Units""));"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Units_UnitId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Products_UnitId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "Products");
        }
    }
}
