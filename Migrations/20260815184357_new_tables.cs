using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class new_tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "Inventory",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VelocityStatusId",
                table: "Inventory",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_Category", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryDriver",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_DeliveryDriver", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Status = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_OrderStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_OrderType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VelocityStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Status = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_VelocityStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_Product", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Product_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    OrderTypeId = table.Column<int>(type: "integer", nullable: false),
                    OrderStatusId = table.Column<int>(type: "integer", nullable: false),
                    DeliveryDriverId = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    CustomerName = table.Column<string>(type: "text", nullable: true),
                    PibkupAddress = table.Column<string>(type: "text", nullable: true),
                    DeliveryAddress = table.Column<string>(type: "text", nullable: true),
                    BundleCode = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_DeliveryDriver_DeliveryDriverId",
                        column: x => x.DeliveryDriverId,
                        principalTable: "DeliveryDriver",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Orders_OrderStatus_OrderStatusId",
                        column: x => x.OrderStatusId,
                        principalTable: "OrderStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_OrderType_OrderTypeId",
                        column: x => x.OrderTypeId,
                        principalTable: "OrderType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_ProductId",
                table: "Inventory",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_VelocityStatusId",
                table: "Inventory",
                column: "VelocityStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DeliveryDriverId",
                table: "Orders",
                column: "DeliveryDriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderStatusId",
                table: "Orders",
                column: "OrderStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderTypeId",
                table: "Orders",
                column: "OrderTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ProductId",
                table: "Orders",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_CategoryId",
                table: "Product",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventory_Product_ProductId",
                table: "Inventory",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventory_VelocityStatus_VelocityStatusId",
                table: "Inventory",
                column: "VelocityStatusId",
                principalTable: "VelocityStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventory_Product_ProductId",
                table: "Inventory");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventory_VelocityStatus_VelocityStatusId",
                table: "Inventory");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "VelocityStatus");

            migrationBuilder.DropTable(
                name: "DeliveryDriver");

            migrationBuilder.DropTable(
                name: "OrderStatus");

            migrationBuilder.DropTable(
                name: "OrderType");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropIndex(
                name: "IX_Inventory_ProductId",
                table: "Inventory");

            migrationBuilder.DropIndex(
                name: "IX_Inventory_VelocityStatusId",
                table: "Inventory");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Inventory");

            migrationBuilder.DropColumn(
                name: "VelocityStatusId",
                table: "Inventory");
        }
    }
}
