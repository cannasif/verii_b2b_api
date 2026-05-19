using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wms.Shared.Infrastructure.Persistence.B2bMigrations
{
    /// <inheritdoc />
    public partial class AddB2bPaymentOrdersAndInstallments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "RII_B2B_PAYMENT_TRANSACTION",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InstallmentCount",
                table: "RII_B2B_PAYMENT_TRANSACTION",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "InstallmentPlanJson",
                table: "RII_B2B_PAYMENT_TRANSACTION",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PaymentInstallmentId",
                table: "RII_B2B_PAYMENT_TRANSACTION",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PaymentOrderId",
                table: "RII_B2B_PAYMENT_TRANSACTION",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "PaymentTermDays",
                table: "RII_B2B_PAYMENT_TRANSACTION",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProviderCollectedAmount",
                table: "RII_B2B_PAYMENT_TRANSACTION",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProviderPaymentAmount",
                table: "RII_B2B_PAYMENT_TRANSACTION",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RII_B2B_PAYMENT_ORDER",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentOrderNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    BuyerId = table.Column<long>(type: "bigint", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RemainingAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    PaymentTermDays = table.Column<short>(type: "smallint", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDueDateOverridden = table.Column<bool>(type: "bit", nullable: false),
                    InstallmentCount = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    ProviderKey = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_RII_B2B_PAYMENT_ORDER", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_B2B_PAYMENT_ORDER_RII_B2B_ORDER_OrderId",
                        column: x => x.OrderId,
                        principalTable: "RII_B2B_ORDER",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RII_B2B_PAYMENT_INSTALLMENT",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentOrderId = table.Column<long>(type: "bigint", nullable: false),
                    InstallmentNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_RII_B2B_PAYMENT_INSTALLMENT", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_B2B_PAYMENT_INSTALLMENT_RII_B2B_PAYMENT_ORDER_PaymentOrderId",
                        column: x => x.PaymentOrderId,
                        principalTable: "RII_B2B_PAYMENT_ORDER",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_B2B_Payment_InstallmentId",
                table: "RII_B2B_PAYMENT_TRANSACTION",
                column: "PaymentInstallmentId");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_Payment_PaymentOrderId",
                table: "RII_B2B_PAYMENT_TRANSACTION",
                column: "PaymentOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_PaymentInstallment_DueDate",
                table: "RII_B2B_PAYMENT_INSTALLMENT",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_PaymentInstallment_Number",
                table: "RII_B2B_PAYMENT_INSTALLMENT",
                columns: new[] { "PaymentOrderId", "InstallmentNumber" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_PaymentInstallment_OrderId",
                table: "RII_B2B_PAYMENT_INSTALLMENT",
                column: "PaymentOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_PaymentOrder_CustomerStatus",
                table: "RII_B2B_PAYMENT_ORDER",
                columns: new[] { "CustomerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_B2B_PaymentOrder_Number",
                table: "RII_B2B_PAYMENT_ORDER",
                column: "PaymentOrderNumber",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_PaymentOrder_OrderId",
                table: "RII_B2B_PAYMENT_ORDER",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_B2B_PAYMENT_TRANSACTION_RII_B2B_PAYMENT_INSTALLMENT_PaymentInstallmentId",
                table: "RII_B2B_PAYMENT_TRANSACTION",
                column: "PaymentInstallmentId",
                principalTable: "RII_B2B_PAYMENT_INSTALLMENT",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_B2B_PAYMENT_TRANSACTION_RII_B2B_PAYMENT_ORDER_PaymentOrderId",
                table: "RII_B2B_PAYMENT_TRANSACTION",
                column: "PaymentOrderId",
                principalTable: "RII_B2B_PAYMENT_ORDER",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RII_B2B_PAYMENT_TRANSACTION_RII_B2B_PAYMENT_INSTALLMENT_PaymentInstallmentId",
                table: "RII_B2B_PAYMENT_TRANSACTION");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_B2B_PAYMENT_TRANSACTION_RII_B2B_PAYMENT_ORDER_PaymentOrderId",
                table: "RII_B2B_PAYMENT_TRANSACTION");

            migrationBuilder.DropTable(
                name: "RII_B2B_PAYMENT_INSTALLMENT");

            migrationBuilder.DropTable(
                name: "RII_B2B_PAYMENT_ORDER");

            migrationBuilder.DropIndex(
                name: "IX_B2B_Payment_InstallmentId",
                table: "RII_B2B_PAYMENT_TRANSACTION");

            migrationBuilder.DropIndex(
                name: "IX_B2B_Payment_PaymentOrderId",
                table: "RII_B2B_PAYMENT_TRANSACTION");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "RII_B2B_PAYMENT_TRANSACTION");

            migrationBuilder.DropColumn(
                name: "InstallmentCount",
                table: "RII_B2B_PAYMENT_TRANSACTION");

            migrationBuilder.DropColumn(
                name: "InstallmentPlanJson",
                table: "RII_B2B_PAYMENT_TRANSACTION");

            migrationBuilder.DropColumn(
                name: "PaymentInstallmentId",
                table: "RII_B2B_PAYMENT_TRANSACTION");

            migrationBuilder.DropColumn(
                name: "PaymentOrderId",
                table: "RII_B2B_PAYMENT_TRANSACTION");

            migrationBuilder.DropColumn(
                name: "PaymentTermDays",
                table: "RII_B2B_PAYMENT_TRANSACTION");

            migrationBuilder.DropColumn(
                name: "ProviderCollectedAmount",
                table: "RII_B2B_PAYMENT_TRANSACTION");

            migrationBuilder.DropColumn(
                name: "ProviderPaymentAmount",
                table: "RII_B2B_PAYMENT_TRANSACTION");
        }
    }
}
