using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wms.Shared.Infrastructure.Persistence.B2bMigrations
{
    /// <inheritdoc />
    public partial class AddB2bPaymentProviderPlanSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankCode",
                table: "RII_B2B_PAYMENT_TRANSACTION",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "RII_B2B_PAYMENT_TRANSACTION",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BinNumber",
                table: "RII_B2B_PAYMENT_TRANSACTION",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardAssociation",
                table: "RII_B2B_PAYMENT_TRANSACTION",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardFamily",
                table: "RII_B2B_PAYMENT_TRANSACTION",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardType",
                table: "RII_B2B_PAYMENT_TRANSACTION",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCommercialCard",
                table: "RII_B2B_PAYMENT_TRANSACTION",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProviderCommissionAmount",
                table: "RII_B2B_PAYMENT_TRANSACTION",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderConversationId",
                table: "RII_B2B_PAYMENT_TRANSACTION",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProviderRate",
                table: "RII_B2B_PAYMENT_TRANSACTION",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankCode",
                table: "RII_B2B_PAYMENT_ORDER",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "RII_B2B_PAYMENT_ORDER",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BinNumber",
                table: "RII_B2B_PAYMENT_ORDER",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardAssociation",
                table: "RII_B2B_PAYMENT_ORDER",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardFamily",
                table: "RII_B2B_PAYMENT_ORDER",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardType",
                table: "RII_B2B_PAYMENT_ORDER",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCommercialCard",
                table: "RII_B2B_PAYMENT_ORDER",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProviderCommissionAmount",
                table: "RII_B2B_PAYMENT_ORDER",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderConversationId",
                table: "RII_B2B_PAYMENT_ORDER",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProviderInstallmentNumber",
                table: "RII_B2B_PAYMENT_ORDER",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProviderInstallmentPrice",
                table: "RII_B2B_PAYMENT_ORDER",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderInstallmentSnapshotJson",
                table: "RII_B2B_PAYMENT_ORDER",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProviderRate",
                table: "RII_B2B_PAYMENT_ORDER",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProviderTotalPrice",
                table: "RII_B2B_PAYMENT_ORDER",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_B2B_Payment_ProviderBin",
                table: "RII_B2B_PAYMENT_TRANSACTION",
                columns: new[] { "ProviderKey", "BinNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_B2B_PaymentOrder_ProviderBin",
                table: "RII_B2B_PAYMENT_ORDER",
                columns: new[] { "ProviderKey", "BinNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_B2B_Payment_ProviderBin",
                table: "RII_B2B_PAYMENT_TRANSACTION");

            migrationBuilder.DropIndex(
                name: "IX_B2B_PaymentOrder_ProviderBin",
                table: "RII_B2B_PAYMENT_ORDER");

            migrationBuilder.DropColumn(
                name: "BankCode",
                table: "RII_B2B_PAYMENT_TRANSACTION");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "RII_B2B_PAYMENT_TRANSACTION");

            migrationBuilder.DropColumn(
                name: "BinNumber",
                table: "RII_B2B_PAYMENT_TRANSACTION");

            migrationBuilder.DropColumn(
                name: "CardAssociation",
                table: "RII_B2B_PAYMENT_TRANSACTION");

            migrationBuilder.DropColumn(
                name: "CardFamily",
                table: "RII_B2B_PAYMENT_TRANSACTION");

            migrationBuilder.DropColumn(
                name: "CardType",
                table: "RII_B2B_PAYMENT_TRANSACTION");

            migrationBuilder.DropColumn(
                name: "IsCommercialCard",
                table: "RII_B2B_PAYMENT_TRANSACTION");

            migrationBuilder.DropColumn(
                name: "ProviderCommissionAmount",
                table: "RII_B2B_PAYMENT_TRANSACTION");

            migrationBuilder.DropColumn(
                name: "ProviderConversationId",
                table: "RII_B2B_PAYMENT_TRANSACTION");

            migrationBuilder.DropColumn(
                name: "ProviderRate",
                table: "RII_B2B_PAYMENT_TRANSACTION");

            migrationBuilder.DropColumn(
                name: "BankCode",
                table: "RII_B2B_PAYMENT_ORDER");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "RII_B2B_PAYMENT_ORDER");

            migrationBuilder.DropColumn(
                name: "BinNumber",
                table: "RII_B2B_PAYMENT_ORDER");

            migrationBuilder.DropColumn(
                name: "CardAssociation",
                table: "RII_B2B_PAYMENT_ORDER");

            migrationBuilder.DropColumn(
                name: "CardFamily",
                table: "RII_B2B_PAYMENT_ORDER");

            migrationBuilder.DropColumn(
                name: "CardType",
                table: "RII_B2B_PAYMENT_ORDER");

            migrationBuilder.DropColumn(
                name: "IsCommercialCard",
                table: "RII_B2B_PAYMENT_ORDER");

            migrationBuilder.DropColumn(
                name: "ProviderCommissionAmount",
                table: "RII_B2B_PAYMENT_ORDER");

            migrationBuilder.DropColumn(
                name: "ProviderConversationId",
                table: "RII_B2B_PAYMENT_ORDER");

            migrationBuilder.DropColumn(
                name: "ProviderInstallmentNumber",
                table: "RII_B2B_PAYMENT_ORDER");

            migrationBuilder.DropColumn(
                name: "ProviderInstallmentPrice",
                table: "RII_B2B_PAYMENT_ORDER");

            migrationBuilder.DropColumn(
                name: "ProviderInstallmentSnapshotJson",
                table: "RII_B2B_PAYMENT_ORDER");

            migrationBuilder.DropColumn(
                name: "ProviderRate",
                table: "RII_B2B_PAYMENT_ORDER");

            migrationBuilder.DropColumn(
                name: "ProviderTotalPrice",
                table: "RII_B2B_PAYMENT_ORDER");
        }
    }
}
