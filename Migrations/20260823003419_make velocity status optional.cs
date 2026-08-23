using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class makevelocitystatusoptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_VelocityStatuses_VelocityStatusId",
                table: "Inventories");

            migrationBuilder.AlterColumn<int>(
                name: "WarehouseId",
                table: "Inventories",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "VelocityStatusId",
                table: "Inventories",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_VelocityStatuses_VelocityStatusId",
                table: "Inventories",
                column: "VelocityStatusId",
                principalTable: "VelocityStatuses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_VelocityStatuses_VelocityStatusId",
                table: "Inventories");

            migrationBuilder.AlterColumn<int>(
                name: "WarehouseId",
                table: "Inventories",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "VelocityStatusId",
                table: "Inventories",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_VelocityStatuses_VelocityStatusId",
                table: "Inventories",
                column: "VelocityStatusId",
                principalTable: "VelocityStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
