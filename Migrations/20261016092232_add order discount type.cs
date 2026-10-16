using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class addorderdiscounttype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiscountIdNumber",
                table: "Orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscountType",
                table: "Orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "none");

            // Discounts entered before this had no type, so any non-zero one becomes a custom discount.
            migrationBuilder.Sql(@"UPDATE ""Orders"" SET ""DiscountType"" = 'custom' WHERE ""DiscountPercent"" > 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountIdNumber",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DiscountType",
                table: "Orders");
        }
    }
}
