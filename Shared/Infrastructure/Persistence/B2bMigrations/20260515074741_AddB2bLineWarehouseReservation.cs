using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wms.Shared.Infrastructure.Persistence.B2bMigrations
{
    /// <inheritdoc />
    public partial class AddB2bLineWarehouseReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WarehouseCode",
                table: "RII_B2B_ORDER_LINE",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseCode",
                table: "RII_B2B_CART_LINE",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_B2B_OrderLine_StockWarehouse",
                table: "RII_B2B_ORDER_LINE",
                columns: new[] { "ErpStockId", "WarehouseCode" });

            migrationBuilder.CreateIndex(
                name: "IX_B2B_CartLine_StockWarehouse",
                table: "RII_B2B_CART_LINE",
                columns: new[] { "ErpStockId", "WarehouseCode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_B2B_OrderLine_StockWarehouse",
                table: "RII_B2B_ORDER_LINE");

            migrationBuilder.DropIndex(
                name: "IX_B2B_CartLine_StockWarehouse",
                table: "RII_B2B_CART_LINE");

            migrationBuilder.DropColumn(
                name: "WarehouseCode",
                table: "RII_B2B_ORDER_LINE");

            migrationBuilder.DropColumn(
                name: "WarehouseCode",
                table: "RII_B2B_CART_LINE");
        }
    }
}
