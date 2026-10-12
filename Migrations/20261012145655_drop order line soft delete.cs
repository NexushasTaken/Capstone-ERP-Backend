using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class droporderlinesoftdelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Lines removed by order edits were hidden by IsActive; hard-delete them so they don't reappear.
            migrationBuilder.Sql("DELETE FROM \"OrderLines\" WHERE \"IsActive\" IS NOT TRUE;");

            migrationBuilder.DropColumn(
                name: "Deleted_At",
                table: "OrderLines");

            migrationBuilder.DropColumn(
                name: "Deleted_By",
                table: "OrderLines");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "OrderLines");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Deleted_At",
                table: "OrderLines",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Deleted_By",
                table: "OrderLines",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "OrderLines",
                type: "boolean",
                nullable: true);
        }
    }
}
