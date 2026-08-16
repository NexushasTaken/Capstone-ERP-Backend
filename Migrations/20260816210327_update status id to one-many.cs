using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class updatestatusidtoonemany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Inventories_StatusId",
                table: "Inventories");

            migrationBuilder.AddColumn<int>(
                name: "Price",
                table: "Inventories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_StatusId",
                table: "Inventories",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Inventories_StatusId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Inventories");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_StatusId",
                table: "Inventories",
                column: "StatusId",
                unique: true);
        }
    }
}
