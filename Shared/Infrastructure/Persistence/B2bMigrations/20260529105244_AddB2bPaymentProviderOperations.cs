using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wms.Shared.Infrastructure.Persistence.B2bMigrations
{
    /// <inheritdoc />
    public partial class AddB2bPaymentProviderOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RII_B2B_PAYMENT_PROVIDER_OPERATION",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentTransactionId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentOrderId = table.Column<long>(type: "bigint", nullable: true),
                    PaymentInstallmentId = table.Column<long>(type: "bigint", nullable: true),
                    ProviderKey = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    OperationType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ExternalOperationId = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RequestJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RequestedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BranchCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "0"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RII_B2B_PAYMENT_PROVIDER_OPERATION", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_B2B_PAYMENT_PROVIDER_OPERATION_RII_B2B_PAYMENT_INSTALLMENT_PaymentInstallmentId",
                        column: x => x.PaymentInstallmentId,
                        principalTable: "RII_B2B_PAYMENT_INSTALLMENT",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_B2B_PAYMENT_PROVIDER_OPERATION_RII_B2B_PAYMENT_ORDER_PaymentOrderId",
                        column: x => x.PaymentOrderId,
                        principalTable: "RII_B2B_PAYMENT_ORDER",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_B2B_PAYMENT_PROVIDER_OPERATION_RII_B2B_PAYMENT_TRANSACTION_PaymentTransactionId",
                        column: x => x.PaymentTransactionId,
                        principalTable: "RII_B2B_PAYMENT_TRANSACTION",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_B2B_PaymentProviderOperation_External",
                table: "RII_B2B_PAYMENT_PROVIDER_OPERATION",
                columns: new[] { "ProviderKey", "ExternalOperationId" });

            migrationBuilder.CreateIndex(
                name: "IX_B2B_PaymentProviderOperation_Idempotency",
                table: "RII_B2B_PAYMENT_PROVIDER_OPERATION",
                column: "IdempotencyKey");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_PaymentProviderOperation_Transaction",
                table: "RII_B2B_PAYMENT_PROVIDER_OPERATION",
                columns: new[] { "PaymentTransactionId", "OperationType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RII_B2B_PAYMENT_PROVIDER_OPERATION_PaymentInstallmentId",
                table: "RII_B2B_PAYMENT_PROVIDER_OPERATION",
                column: "PaymentInstallmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_B2B_PAYMENT_PROVIDER_OPERATION_PaymentOrderId",
                table: "RII_B2B_PAYMENT_PROVIDER_OPERATION",
                column: "PaymentOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RII_B2B_PAYMENT_PROVIDER_OPERATION");
        }
    }
}
