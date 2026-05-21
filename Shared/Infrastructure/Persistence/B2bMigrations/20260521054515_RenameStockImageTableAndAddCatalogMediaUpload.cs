using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wms.Shared.Infrastructure.Persistence.B2bMigrations
{
    /// <inheritdoc />
    public partial class RenameStockImageTableAndAddCatalogMediaUpload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RII_WMS_STOCK_IMAGE_RII_WMS_STOCK_StockId",
                table: "RII_WMS_STOCK_IMAGE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_WMS_STOCK_IMAGE",
                table: "RII_WMS_STOCK_IMAGE");

            migrationBuilder.RenameTable(
                name: "RII_WMS_STOCK_IMAGE",
                newName: "RII_STOCK_IMAGE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_STOCK_IMAGE",
                table: "RII_STOCK_IMAGE",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_STOCK_IMAGE_RII_WMS_STOCK_StockId",
                table: "RII_STOCK_IMAGE",
                column: "StockId",
                principalTable: "RII_WMS_STOCK",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RII_STOCK_IMAGE_RII_WMS_STOCK_StockId",
                table: "RII_STOCK_IMAGE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_STOCK_IMAGE",
                table: "RII_STOCK_IMAGE");

            migrationBuilder.RenameTable(
                name: "RII_STOCK_IMAGE",
                newName: "RII_WMS_STOCK_IMAGE");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_WMS_STOCK_IMAGE",
                table: "RII_WMS_STOCK_IMAGE",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_WMS_STOCK_IMAGE_RII_WMS_STOCK_StockId",
                table: "RII_WMS_STOCK_IMAGE",
                column: "StockId",
                principalTable: "RII_WMS_STOCK",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
