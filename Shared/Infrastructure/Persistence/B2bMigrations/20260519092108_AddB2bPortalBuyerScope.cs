using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wms.Shared.Infrastructure.Persistence.B2bMigrations
{
    /// <inheritdoc />
    public partial class AddB2bPortalBuyerScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "BuyerId",
                table: "RII_B2B_QUOTATION",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "BuyerId",
                table: "RII_B2B_ORDER",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "BuyerId",
                table: "RII_B2B_CART",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_B2B_Quotation_CustomerBuyerStatus",
                table: "RII_B2B_QUOTATION",
                columns: new[] { "CustomerId", "BuyerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_B2B_Order_CustomerBuyerStatus",
                table: "RII_B2B_ORDER",
                columns: new[] { "CustomerId", "BuyerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_B2B_Cart_CustomerBuyerStatus",
                table: "RII_B2B_CART",
                columns: new[] { "CustomerId", "BuyerId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_B2B_Quotation_CustomerBuyerStatus",
                table: "RII_B2B_QUOTATION");

            migrationBuilder.DropIndex(
                name: "IX_B2B_Order_CustomerBuyerStatus",
                table: "RII_B2B_ORDER");

            migrationBuilder.DropIndex(
                name: "IX_B2B_Cart_CustomerBuyerStatus",
                table: "RII_B2B_CART");

            migrationBuilder.DropColumn(
                name: "BuyerId",
                table: "RII_B2B_QUOTATION");

            migrationBuilder.DropColumn(
                name: "BuyerId",
                table: "RII_B2B_ORDER");

            migrationBuilder.DropColumn(
                name: "BuyerId",
                table: "RII_B2B_CART");
        }
    }
}
