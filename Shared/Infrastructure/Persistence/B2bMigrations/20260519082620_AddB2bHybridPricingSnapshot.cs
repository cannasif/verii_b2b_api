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

            migrationBuilder.AddColumn<short>(
                name: "PriceListNumber",
                table: "RII_WMS_CUSTOMER",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "RII_B2B_QUOTATION_LINE",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<long>(
                name: "PriceListId",
                table: "RII_B2B_QUOTATION_LINE",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PriceResolvedAt",
                table: "RII_B2B_QUOTATION_LINE",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriceSource",
                table: "RII_B2B_QUOTATION_LINE",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "RII_B2B_ORDER_LINE",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<long>(
                name: "PriceListId",
                table: "RII_B2B_ORDER_LINE",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PriceResolvedAt",
                table: "RII_B2B_ORDER_LINE",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriceSource",
                table: "RII_B2B_ORDER_LINE",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountRate",
                table: "RII_B2B_CART_LINE",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "RII_B2B_CART_LINE",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<long>(
                name: "PriceListId",
                table: "RII_B2B_CART_LINE",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PriceResolvedAt",
                table: "RII_B2B_CART_LINE",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriceSource",
                table: "RII_B2B_CART_LINE",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VatAmount",
                table: "RII_B2B_CART_LINE",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VatRate",
                table: "RII_B2B_CART_LINE",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SalesPrice1",
                table: "RII_WMS_STOCK");

            migrationBuilder.DropColumn(
                name: "SalesPrice2",
                table: "RII_WMS_STOCK");

            migrationBuilder.DropColumn(
                name: "SalesPrice3",
                table: "RII_WMS_STOCK");

            migrationBuilder.DropColumn(
                name: "SalesPrice4",
                table: "RII_WMS_STOCK");

            migrationBuilder.DropColumn(
                name: "VatRate",
                table: "RII_WMS_STOCK");

            migrationBuilder.DropColumn(
                name: "PriceListNumber",
                table: "RII_WMS_CUSTOMER");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "RII_B2B_QUOTATION_LINE");

            migrationBuilder.DropColumn(
                name: "PriceListId",
                table: "RII_B2B_QUOTATION_LINE");

            migrationBuilder.DropColumn(
                name: "PriceResolvedAt",
                table: "RII_B2B_QUOTATION_LINE");

            migrationBuilder.DropColumn(
                name: "PriceSource",
                table: "RII_B2B_QUOTATION_LINE");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "RII_B2B_ORDER_LINE");

            migrationBuilder.DropColumn(
                name: "PriceListId",
                table: "RII_B2B_ORDER_LINE");

            migrationBuilder.DropColumn(
                name: "PriceResolvedAt",
                table: "RII_B2B_ORDER_LINE");

            migrationBuilder.DropColumn(
                name: "PriceSource",
                table: "RII_B2B_ORDER_LINE");

            migrationBuilder.DropColumn(
                name: "DiscountRate",
                table: "RII_B2B_CART_LINE");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "RII_B2B_CART_LINE");

            migrationBuilder.DropColumn(
                name: "PriceListId",
                table: "RII_B2B_CART_LINE");

            migrationBuilder.DropColumn(
                name: "PriceResolvedAt",
                table: "RII_B2B_CART_LINE");

            migrationBuilder.DropColumn(
                name: "PriceSource",
                table: "RII_B2B_CART_LINE");

            migrationBuilder.DropColumn(
                name: "VatAmount",
                table: "RII_B2B_CART_LINE");

            migrationBuilder.DropColumn(
                name: "VatRate",
                table: "RII_B2B_CART_LINE");
        }
    }
}
