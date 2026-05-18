using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wms.Shared.Infrastructure.Persistence.B2bMigrations
{
    /// <inheritdoc />
    public partial class AddB2bCrmCommercialFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "RII_B2B_QUOTE_REQUEST_LINE",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description1",
                table: "RII_B2B_QUOTE_REQUEST_LINE",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description2",
                table: "RII_B2B_QUOTE_REQUEST_LINE",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description3",
                table: "RII_B2B_QUOTE_REQUEST_LINE",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount1",
                table: "RII_B2B_QUOTE_REQUEST_LINE",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount2",
                table: "RII_B2B_QUOTE_REQUEST_LINE",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount3",
                table: "RII_B2B_QUOTE_REQUEST_LINE",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountRate1",
                table: "RII_B2B_QUOTE_REQUEST_LINE",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountRate2",
                table: "RII_B2B_QUOTE_REQUEST_LINE",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountRate3",
                table: "RII_B2B_QUOTE_REQUEST_LINE",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ErpProjectCode",
                table: "RII_B2B_QUOTE_REQUEST_LINE",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMainRelatedProduct",
                table: "RII_B2B_QUOTE_REQUEST_LINE",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LineGrandTotal",
                table: "RII_B2B_QUOTE_REQUEST_LINE",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LineTotal",
                table: "RII_B2B_QUOTE_REQUEST_LINE",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "PricingRuleHeaderId",
                table: "RII_B2B_QUOTE_REQUEST_LINE",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedProductKey",
                table: "RII_B2B_QUOTE_REQUEST_LINE",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VatAmount",
                table: "RII_B2B_QUOTE_REQUEST_LINE",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VatRate",
                table: "RII_B2B_QUOTE_REQUEST_LINE",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryDate",
                table: "RII_B2B_QUOTE_REQUEST",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryMethod",
                table: "RII_B2B_QUOTE_REQUEST",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GeneralDiscountAmount",
                table: "RII_B2B_QUOTE_REQUEST",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GeneralDiscountRate",
                table: "RII_B2B_QUOTE_REQUEST",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OfferDate",
                table: "RII_B2B_QUOTE_REQUEST",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfferNo",
                table: "RII_B2B_QUOTE_REQUEST",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfferType",
                table: "RII_B2B_QUOTE_REQUEST",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PaymentTypeId",
                table: "RII_B2B_QUOTE_REQUEST",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RevisionId",
                table: "RII_B2B_QUOTE_REQUEST",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RevisionNo",
                table: "RII_B2B_QUOTE_REQUEST",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "RII_B2B_QUOTE_REQUEST",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidUntil",
                table: "RII_B2B_QUOTE_REQUEST",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "RII_B2B_ORDER_LINE",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description1",
                table: "RII_B2B_ORDER_LINE",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description2",
                table: "RII_B2B_ORDER_LINE",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description3",
                table: "RII_B2B_ORDER_LINE",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount1",
                table: "RII_B2B_ORDER_LINE",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount2",
                table: "RII_B2B_ORDER_LINE",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount3",
                table: "RII_B2B_ORDER_LINE",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountRate1",
                table: "RII_B2B_ORDER_LINE",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountRate2",
                table: "RII_B2B_ORDER_LINE",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountRate3",
                table: "RII_B2B_ORDER_LINE",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ErpProjectCode",
                table: "RII_B2B_ORDER_LINE",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMainRelatedProduct",
                table: "RII_B2B_ORDER_LINE",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LineGrandTotal",
                table: "RII_B2B_ORDER_LINE",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "PricingRuleHeaderId",
                table: "RII_B2B_ORDER_LINE",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedProductKey",
                table: "RII_B2B_ORDER_LINE",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VatAmount",
                table: "RII_B2B_ORDER_LINE",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VatRate",
                table: "RII_B2B_ORDER_LINE",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryDate",
                table: "RII_B2B_ORDER",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryMethod",
                table: "RII_B2B_ORDER",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "RII_B2B_ORDER",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GeneralDiscountAmount",
                table: "RII_B2B_ORDER",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GeneralDiscountRate",
                table: "RII_B2B_ORDER",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OfferDate",
                table: "RII_B2B_ORDER",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfferNo",
                table: "RII_B2B_ORDER",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfferType",
                table: "RII_B2B_ORDER",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PaymentTypeId",
                table: "RII_B2B_ORDER",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "QuoteRequestId",
                table: "RII_B2B_ORDER",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RevisionId",
                table: "RII_B2B_ORDER",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RevisionNo",
                table: "RII_B2B_ORDER",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidUntil",
                table: "RII_B2B_ORDER",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_B2B_Quote_OfferDate",
                table: "RII_B2B_QUOTE_REQUEST",
                column: "OfferDate");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_Quote_OfferNo",
                table: "RII_B2B_QUOTE_REQUEST",
                column: "OfferNo");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_Order_OfferDate",
                table: "RII_B2B_ORDER",
                column: "OfferDate");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_Order_OfferNo",
                table: "RII_B2B_ORDER",
                column: "OfferNo");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_Order_QuoteRequestId",
                table: "RII_B2B_ORDER",
                column: "QuoteRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_B2B_Quote_OfferDate",
                table: "RII_B2B_QUOTE_REQUEST");

            migrationBuilder.DropIndex(
                name: "IX_B2B_Quote_OfferNo",
                table: "RII_B2B_QUOTE_REQUEST");

            migrationBuilder.DropIndex(
                name: "IX_B2B_Order_OfferDate",
                table: "RII_B2B_ORDER");

            migrationBuilder.DropIndex(
                name: "IX_B2B_Order_OfferNo",
                table: "RII_B2B_ORDER");

            migrationBuilder.DropIndex(
                name: "IX_B2B_Order_QuoteRequestId",
                table: "RII_B2B_ORDER");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "RII_B2B_QUOTE_REQUEST_LINE");

            migrationBuilder.DropColumn(
                name: "Description1",
                table: "RII_B2B_QUOTE_REQUEST_LINE");

            migrationBuilder.DropColumn(
                name: "Description2",
                table: "RII_B2B_QUOTE_REQUEST_LINE");

            migrationBuilder.DropColumn(
                name: "Description3",
                table: "RII_B2B_QUOTE_REQUEST_LINE");

            migrationBuilder.DropColumn(
                name: "DiscountAmount1",
                table: "RII_B2B_QUOTE_REQUEST_LINE");

            migrationBuilder.DropColumn(
                name: "DiscountAmount2",
                table: "RII_B2B_QUOTE_REQUEST_LINE");

            migrationBuilder.DropColumn(
                name: "DiscountAmount3",
                table: "RII_B2B_QUOTE_REQUEST_LINE");

            migrationBuilder.DropColumn(
                name: "DiscountRate1",
                table: "RII_B2B_QUOTE_REQUEST_LINE");

            migrationBuilder.DropColumn(
                name: "DiscountRate2",
                table: "RII_B2B_QUOTE_REQUEST_LINE");

            migrationBuilder.DropColumn(
                name: "DiscountRate3",
                table: "RII_B2B_QUOTE_REQUEST_LINE");

            migrationBuilder.DropColumn(
                name: "ErpProjectCode",
                table: "RII_B2B_QUOTE_REQUEST_LINE");

            migrationBuilder.DropColumn(
                name: "IsMainRelatedProduct",
                table: "RII_B2B_QUOTE_REQUEST_LINE");

            migrationBuilder.DropColumn(
                name: "LineGrandTotal",
                table: "RII_B2B_QUOTE_REQUEST_LINE");

            migrationBuilder.DropColumn(
                name: "LineTotal",
                table: "RII_B2B_QUOTE_REQUEST_LINE");

            migrationBuilder.DropColumn(
                name: "PricingRuleHeaderId",
                table: "RII_B2B_QUOTE_REQUEST_LINE");

            migrationBuilder.DropColumn(
                name: "RelatedProductKey",
                table: "RII_B2B_QUOTE_REQUEST_LINE");

            migrationBuilder.DropColumn(
                name: "VatAmount",
                table: "RII_B2B_QUOTE_REQUEST_LINE");

            migrationBuilder.DropColumn(
                name: "VatRate",
                table: "RII_B2B_QUOTE_REQUEST_LINE");

            migrationBuilder.DropColumn(
                name: "DeliveryDate",
                table: "RII_B2B_QUOTE_REQUEST");

            migrationBuilder.DropColumn(
                name: "DeliveryMethod",
                table: "RII_B2B_QUOTE_REQUEST");

            migrationBuilder.DropColumn(
                name: "GeneralDiscountAmount",
                table: "RII_B2B_QUOTE_REQUEST");

            migrationBuilder.DropColumn(
                name: "GeneralDiscountRate",
                table: "RII_B2B_QUOTE_REQUEST");

            migrationBuilder.DropColumn(
                name: "OfferDate",
                table: "RII_B2B_QUOTE_REQUEST");

            migrationBuilder.DropColumn(
                name: "OfferNo",
                table: "RII_B2B_QUOTE_REQUEST");

            migrationBuilder.DropColumn(
                name: "OfferType",
                table: "RII_B2B_QUOTE_REQUEST");

            migrationBuilder.DropColumn(
                name: "PaymentTypeId",
                table: "RII_B2B_QUOTE_REQUEST");

            migrationBuilder.DropColumn(
                name: "RevisionId",
                table: "RII_B2B_QUOTE_REQUEST");

            migrationBuilder.DropColumn(
                name: "RevisionNo",
                table: "RII_B2B_QUOTE_REQUEST");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "RII_B2B_QUOTE_REQUEST");

            migrationBuilder.DropColumn(
                name: "ValidUntil",
                table: "RII_B2B_QUOTE_REQUEST");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "RII_B2B_ORDER_LINE");

            migrationBuilder.DropColumn(
                name: "Description1",
                table: "RII_B2B_ORDER_LINE");

            migrationBuilder.DropColumn(
                name: "Description2",
                table: "RII_B2B_ORDER_LINE");

            migrationBuilder.DropColumn(
                name: "Description3",
                table: "RII_B2B_ORDER_LINE");

            migrationBuilder.DropColumn(
                name: "DiscountAmount1",
                table: "RII_B2B_ORDER_LINE");

            migrationBuilder.DropColumn(
                name: "DiscountAmount2",
                table: "RII_B2B_ORDER_LINE");

            migrationBuilder.DropColumn(
                name: "DiscountAmount3",
                table: "RII_B2B_ORDER_LINE");

            migrationBuilder.DropColumn(
                name: "DiscountRate1",
                table: "RII_B2B_ORDER_LINE");

            migrationBuilder.DropColumn(
                name: "DiscountRate2",
                table: "RII_B2B_ORDER_LINE");

            migrationBuilder.DropColumn(
                name: "DiscountRate3",
                table: "RII_B2B_ORDER_LINE");

            migrationBuilder.DropColumn(
                name: "ErpProjectCode",
                table: "RII_B2B_ORDER_LINE");

            migrationBuilder.DropColumn(
                name: "IsMainRelatedProduct",
                table: "RII_B2B_ORDER_LINE");

            migrationBuilder.DropColumn(
                name: "LineGrandTotal",
                table: "RII_B2B_ORDER_LINE");

            migrationBuilder.DropColumn(
                name: "PricingRuleHeaderId",
                table: "RII_B2B_ORDER_LINE");

            migrationBuilder.DropColumn(
                name: "RelatedProductKey",
                table: "RII_B2B_ORDER_LINE");

            migrationBuilder.DropColumn(
                name: "VatAmount",
                table: "RII_B2B_ORDER_LINE");

            migrationBuilder.DropColumn(
                name: "VatRate",
                table: "RII_B2B_ORDER_LINE");

            migrationBuilder.DropColumn(
                name: "DeliveryDate",
                table: "RII_B2B_ORDER");

            migrationBuilder.DropColumn(
                name: "DeliveryMethod",
                table: "RII_B2B_ORDER");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "RII_B2B_ORDER");

            migrationBuilder.DropColumn(
                name: "GeneralDiscountAmount",
                table: "RII_B2B_ORDER");

            migrationBuilder.DropColumn(
                name: "GeneralDiscountRate",
                table: "RII_B2B_ORDER");

            migrationBuilder.DropColumn(
                name: "OfferDate",
                table: "RII_B2B_ORDER");

            migrationBuilder.DropColumn(
                name: "OfferNo",
                table: "RII_B2B_ORDER");

            migrationBuilder.DropColumn(
                name: "OfferType",
                table: "RII_B2B_ORDER");

            migrationBuilder.DropColumn(
                name: "PaymentTypeId",
                table: "RII_B2B_ORDER");

            migrationBuilder.DropColumn(
                name: "QuoteRequestId",
                table: "RII_B2B_ORDER");

            migrationBuilder.DropColumn(
                name: "RevisionId",
                table: "RII_B2B_ORDER");

            migrationBuilder.DropColumn(
                name: "RevisionNo",
                table: "RII_B2B_ORDER");

            migrationBuilder.DropColumn(
                name: "ValidUntil",
                table: "RII_B2B_ORDER");
        }
    }
}
