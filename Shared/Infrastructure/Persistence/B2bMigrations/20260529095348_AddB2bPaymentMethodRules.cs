using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wms.Shared.Infrastructure.Persistence.B2bMigrations
{
    /// <inheritdoc />
    public partial class AddB2bPaymentMethodRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RII_B2B_PAYMENT_METHOD_RULE",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<long>(type: "bigint", nullable: true),
                    CustomerId = table.Column<long>(type: "bigint", nullable: true),
                    CustomerGroupCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    ProviderKey = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    RuleType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    MinAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MaxAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_RII_B2B_PAYMENT_METHOD_RULE", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_B2B_PaymentMethodRule_ActiveDates",
                table: "RII_B2B_PAYMENT_METHOD_RULE",
                columns: new[] { "IsActive", "ValidFrom", "ValidTo" });

            migrationBuilder.CreateIndex(
                name: "IX_B2B_PaymentMethodRule_Method",
                table: "RII_B2B_PAYMENT_METHOD_RULE",
                columns: new[] { "ProviderKey", "PaymentMethod", "RuleType" });

            migrationBuilder.CreateIndex(
                name: "IX_B2B_PaymentMethodRule_Scope",
                table: "RII_B2B_PAYMENT_METHOD_RULE",
                columns: new[] { "CompanyId", "CustomerId", "CustomerGroupCode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RII_B2B_PAYMENT_METHOD_RULE");
        }
    }
}
