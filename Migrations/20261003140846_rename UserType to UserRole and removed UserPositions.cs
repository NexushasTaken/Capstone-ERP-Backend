using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class renameUserTypetoUserRoleandremovedUserPositions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAccounts_UserPositions_UserPositionId",
                table: "UserAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAccounts_UserTypes_UserTypeId",
                table: "UserAccounts");

            migrationBuilder.DropTable(
                name: "UserPositions");

            migrationBuilder.DropTable(
                name: "UserTypes");

            migrationBuilder.DropIndex(
                name: "IX_UserAccounts_UserPositionId",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "UserPositionId",
                table: "UserAccounts");

            migrationBuilder.RenameColumn(
                name: "UserTypeId",
                table: "UserAccounts",
                newName: "UserRoleId");

            migrationBuilder.RenameIndex(
                name: "IX_UserAccounts_UserTypeId",
                table: "UserAccounts",
                newName: "IX_UserAccounts_UserRoleId");

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Role = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_UserAccounts_UserRoles_UserRoleId",
                table: "UserAccounts",
                column: "UserRoleId",
                principalTable: "UserRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAccounts_UserRoles_UserRoleId",
                table: "UserAccounts");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.RenameColumn(
                name: "UserRoleId",
                table: "UserAccounts",
                newName: "UserTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_UserAccounts_UserRoleId",
                table: "UserAccounts",
                newName: "IX_UserAccounts_UserTypeId");

            migrationBuilder.AddColumn<int>(
                name: "UserPositionId",
                table: "UserAccounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UserPositions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Created_By = table.Column<int>(type: "integer", nullable: true),
                    Deleted_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted_By = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    Position = table.Column<string>(type: "text", nullable: true),
                    Updated_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Updated_By = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPositions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Created_By = table.Column<int>(type: "integer", nullable: true),
                    Deleted_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted_By = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    Updated_At = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Updated_By = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_UserPositionId",
                table: "UserAccounts",
                column: "UserPositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAccounts_UserPositions_UserPositionId",
                table: "UserAccounts",
                column: "UserPositionId",
                principalTable: "UserPositions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAccounts_UserTypes_UserTypeId",
                table: "UserAccounts",
                column: "UserTypeId",
                principalTable: "UserTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
