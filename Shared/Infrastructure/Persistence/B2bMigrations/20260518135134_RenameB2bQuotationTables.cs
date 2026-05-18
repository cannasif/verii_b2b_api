using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wms.Shared.Infrastructure.Persistence.B2bMigrations
{
    /// <inheritdoc />
    public partial class RenameB2bQuotationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RII_B2B_QUOTE_REQUEST_LINE_RII_B2B_QUOTE_REQUEST_QuoteRequestId",
                table: "RII_B2B_QUOTE_REQUEST_LINE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_B2B_QUOTE_REQUEST_LINE",
                table: "RII_B2B_QUOTE_REQUEST_LINE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_B2B_QUOTE_REQUEST",
                table: "RII_B2B_QUOTE_REQUEST");

            migrationBuilder.RenameTable(
                name: "RII_B2B_QUOTE_REQUEST_LINE",
                newName: "RII_B2B_QUOTATION_LINE");

            migrationBuilder.RenameTable(
                name: "RII_B2B_QUOTE_REQUEST",
                newName: "RII_B2B_QUOTATION");

            migrationBuilder.RenameColumn(
                name: "QuoteRequestId",
                table: "RII_B2B_ORDER",
                newName: "QuotationId");

            migrationBuilder.RenameIndex(
                name: "IX_B2B_Order_QuoteRequestId",
                table: "RII_B2B_ORDER",
                newName: "IX_B2B_Order_QuotationId");

            migrationBuilder.RenameColumn(
                name: "QuoteRequestId",
                table: "RII_B2B_QUOTATION_LINE",
                newName: "QuotationId");

            migrationBuilder.RenameIndex(
                name: "IX_B2B_QuoteLine_QuoteId",
                table: "RII_B2B_QUOTATION_LINE",
                newName: "IX_B2B_QuotationLine_QuotationId");

            migrationBuilder.RenameIndex(
                name: "IX_B2B_Quote_QuoteNumber",
                table: "RII_B2B_QUOTATION",
                newName: "IX_B2B_Quotation_QuoteNumber");

            migrationBuilder.RenameIndex(
                name: "IX_B2B_Quote_OfferNo",
                table: "RII_B2B_QUOTATION",
                newName: "IX_B2B_Quotation_OfferNo");

            migrationBuilder.RenameIndex(
                name: "IX_B2B_Quote_OfferDate",
                table: "RII_B2B_QUOTATION",
                newName: "IX_B2B_Quotation_OfferDate");

            migrationBuilder.RenameIndex(
                name: "IX_B2B_Quote_CustomerStatus",
                table: "RII_B2B_QUOTATION",
                newName: "IX_B2B_Quotation_CustomerStatus");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_B2B_QUOTATION_LINE",
                table: "RII_B2B_QUOTATION_LINE",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_B2B_QUOTATION",
                table: "RII_B2B_QUOTATION",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_B2B_QUOTATION_LINE_RII_B2B_QUOTATION_QuotationId",
                table: "RII_B2B_QUOTATION_LINE",
                column: "QuotationId",
                principalTable: "RII_B2B_QUOTATION",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RII_B2B_QUOTATION_LINE_RII_B2B_QUOTATION_QuotationId",
                table: "RII_B2B_QUOTATION_LINE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_B2B_QUOTATION_LINE",
                table: "RII_B2B_QUOTATION_LINE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_B2B_QUOTATION",
                table: "RII_B2B_QUOTATION");

            migrationBuilder.RenameTable(
                name: "RII_B2B_QUOTATION_LINE",
                newName: "RII_B2B_QUOTE_REQUEST_LINE");

            migrationBuilder.RenameTable(
                name: "RII_B2B_QUOTATION",
                newName: "RII_B2B_QUOTE_REQUEST");

            migrationBuilder.RenameColumn(
                name: "QuotationId",
                table: "RII_B2B_ORDER",
                newName: "QuoteRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_B2B_Order_QuotationId",
                table: "RII_B2B_ORDER",
                newName: "IX_B2B_Order_QuoteRequestId");

            migrationBuilder.RenameColumn(
                name: "QuotationId",
                table: "RII_B2B_QUOTE_REQUEST_LINE",
                newName: "QuoteRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_B2B_QuotationLine_QuotationId",
                table: "RII_B2B_QUOTE_REQUEST_LINE",
                newName: "IX_B2B_QuoteLine_QuoteId");

            migrationBuilder.RenameIndex(
                name: "IX_B2B_Quotation_QuoteNumber",
                table: "RII_B2B_QUOTE_REQUEST",
                newName: "IX_B2B_Quote_QuoteNumber");

            migrationBuilder.RenameIndex(
                name: "IX_B2B_Quotation_OfferNo",
                table: "RII_B2B_QUOTE_REQUEST",
                newName: "IX_B2B_Quote_OfferNo");

            migrationBuilder.RenameIndex(
                name: "IX_B2B_Quotation_OfferDate",
                table: "RII_B2B_QUOTE_REQUEST",
                newName: "IX_B2B_Quote_OfferDate");

            migrationBuilder.RenameIndex(
                name: "IX_B2B_Quotation_CustomerStatus",
                table: "RII_B2B_QUOTE_REQUEST",
                newName: "IX_B2B_Quote_CustomerStatus");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_B2B_QUOTE_REQUEST_LINE",
                table: "RII_B2B_QUOTE_REQUEST_LINE",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_B2B_QUOTE_REQUEST",
                table: "RII_B2B_QUOTE_REQUEST",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_B2B_QUOTE_REQUEST_LINE_RII_B2B_QUOTE_REQUEST_QuoteRequestId",
                table: "RII_B2B_QUOTE_REQUEST_LINE",
                column: "QuoteRequestId",
                principalTable: "RII_B2B_QUOTE_REQUEST",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
