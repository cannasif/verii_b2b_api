using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wms.Shared.Infrastructure.Persistence.B2bMigrations
{
    /// <inheritdoc />
    public partial class AddB2bPaymentProviderLookups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RII_B2B_PAYMENT_PROVIDER_INQUIRY_LOG",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderKey = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    InquiryType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    BinNumber = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ConversationId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RequestJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_RII_B2B_PAYMENT_PROVIDER_INQUIRY_LOG", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_B2B_ProviderInquiry_BinNumber",
                table: "RII_B2B_PAYMENT_PROVIDER_INQUIRY_LOG",
                column: "BinNumber");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_ProviderInquiry_ProviderTypeDate",
                table: "RII_B2B_PAYMENT_PROVIDER_INQUIRY_LOG",
                columns: new[] { "ProviderKey", "InquiryType", "RequestedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_B2B_ProviderInquiry_Status",
                table: "RII_B2B_PAYMENT_PROVIDER_INQUIRY_LOG",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RII_B2B_PAYMENT_PROVIDER_INQUIRY_LOG");
        }
    }
}
