using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wms.Shared.Infrastructure.Persistence.B2bMigrations
{
    /// <inheritdoc />
    public partial class AddB2bHybridPricingSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "PriceListNumber",
                table: "RII_WMS_CUSTOMER",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalesPrice1",
                table: "RII_WMS_STOCK",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalesPrice2",
                table: "RII_WMS_STOCK",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalesPrice3",
                table: "RII_WMS_STOCK",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalesPrice4",
                table: "RII_WMS_STOCK",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VatRate",
                table: "RII_WMS_STOCK",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            AddSnapshotColumns(migrationBuilder, "RII_B2B_CART_LINE", hasCartDiscount: true);
            AddSnapshotColumns(migrationBuilder, "RII_B2B_ORDER_LINE", hasCartDiscount: false);
            AddSnapshotColumns(migrationBuilder, "RII_B2B_QUOTATION_LINE", hasCartDiscount: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DropSnapshotColumns(migrationBuilder, "RII_B2B_QUOTATION_LINE", hasCartDiscount: false);
            DropSnapshotColumns(migrationBuilder, "RII_B2B_ORDER_LINE", hasCartDiscount: false);
            DropSnapshotColumns(migrationBuilder, "RII_B2B_CART_LINE", hasCartDiscount: true);

            migrationBuilder.DropColumn(name: "VatRate", table: "RII_WMS_STOCK");
            migrationBuilder.DropColumn(name: "SalesPrice4", table: "RII_WMS_STOCK");
            migrationBuilder.DropColumn(name: "SalesPrice3", table: "RII_WMS_STOCK");
            migrationBuilder.DropColumn(name: "SalesPrice2", table: "RII_WMS_STOCK");
            migrationBuilder.DropColumn(name: "SalesPrice1", table: "RII_WMS_STOCK");
            migrationBuilder.DropColumn(name: "PriceListNumber", table: "RII_WMS_CUSTOMER");
        }

        private static void AddSnapshotColumns(MigrationBuilder migrationBuilder, string table, bool hasCartDiscount)
        {
            migrationBuilder.AddColumn<string>(
                name: "PriceSource",
                table: table,
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PriceListId",
                table: table,
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: table,
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<DateTime>(
                name: "PriceResolvedAt",
                table: table,
                type: "datetime2",
                nullable: true);

            if (!hasCartDiscount)
            {
                return;
            }

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountRate",
                table: table,
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VatRate",
                table: table,
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VatAmount",
                table: table,
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }

        private static void DropSnapshotColumns(MigrationBuilder migrationBuilder, string table, bool hasCartDiscount)
        {
            if (hasCartDiscount)
            {
                migrationBuilder.DropColumn(name: "VatAmount", table: table);
                migrationBuilder.DropColumn(name: "VatRate", table: table);
                migrationBuilder.DropColumn(name: "DiscountRate", table: table);
            }

            migrationBuilder.DropColumn(name: "PriceResolvedAt", table: table);
            migrationBuilder.DropColumn(name: "ExchangeRate", table: table);
            migrationBuilder.DropColumn(name: "PriceListId", table: table);
            migrationBuilder.DropColumn(name: "PriceSource", table: table);
        }
    }
}
