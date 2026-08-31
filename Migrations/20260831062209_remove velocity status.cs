using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class removevelocitystatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_VelocityStatuses_VelocityStatusId",
                table: "Inventories");

            migrationBuilder.DropTable(
                name: "VelocityStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_VelocityStatusId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "VelocityStatusId",
                table: "Inventories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VelocityStatusId",
                table: "Inventories",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VelocityStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Created_By = table.Column<int>(type: "integer", nullable: true),
                    Deleted_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted_By = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    Updated_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Updated_By = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VelocityStatuses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_VelocityStatusId",
                table: "Inventories",
                column: "VelocityStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_VelocityStatuses_VelocityStatusId",
                table: "Inventories",
                column: "VelocityStatusId",
                principalTable: "VelocityStatuses",
                principalColumn: "Id");
        }
    }
}
