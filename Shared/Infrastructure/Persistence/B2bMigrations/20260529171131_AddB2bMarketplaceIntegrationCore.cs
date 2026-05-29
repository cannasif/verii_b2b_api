using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wms.Shared.Infrastructure.Persistence.B2bMigrations
{
    /// <inheritdoc />
    public partial class AddB2bMarketplaceIntegrationCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RII_B2B_MARKETPLACE_CHANNEL",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SellerId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ApiBaseUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AuthType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CredentialsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupportsProductCreate = table.Column<bool>(type: "bit", nullable: false),
                    SupportsPriceUpdate = table.Column<bool>(type: "bit", nullable: false),
                    SupportsStockUpdate = table.Column<bool>(type: "bit", nullable: false),
                    SupportsOrderImport = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastSyncDate = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_RII_B2B_MARKETPLACE_CHANNEL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RII_B2B_MARKETPLACE_LISTING",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChannelId = table.Column<long>(type: "bigint", nullable: false),
                    CatalogProductId = table.Column<long>(type: "bigint", nullable: true),
                    ErpStockId = table.Column<long>(type: "bigint", nullable: true),
                    Sku = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    MarketplaceProductId = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    MarketplaceListingId = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    LastPushedPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    LastPushedQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    CurrencyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    LastProductSyncDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastPriceSyncDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastStockSyncDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_RII_B2B_MARKETPLACE_LISTING", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_B2B_MARKETPLACE_LISTING_RII_B2B_CATALOG_PRODUCT_CatalogProductId",
                        column: x => x.CatalogProductId,
                        principalTable: "RII_B2B_CATALOG_PRODUCT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RII_B2B_MARKETPLACE_LISTING_RII_B2B_MARKETPLACE_CHANNEL_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "RII_B2B_MARKETPLACE_CHANNEL",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RII_B2B_MARKETPLACE_SYNC_EVENT",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChannelId = table.Column<long>(type: "bigint", nullable: false),
                    ListingId = table.Column<long>(type: "bigint", nullable: true),
                    OperationType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ExternalBatchId = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    RequestJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_RII_B2B_MARKETPLACE_SYNC_EVENT", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_B2B_MARKETPLACE_SYNC_EVENT_RII_B2B_MARKETPLACE_CHANNEL_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "RII_B2B_MARKETPLACE_CHANNEL",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_B2B_MARKETPLACE_SYNC_EVENT_RII_B2B_MARKETPLACE_LISTING_ListingId",
                        column: x => x.ListingId,
                        principalTable: "RII_B2B_MARKETPLACE_LISTING",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_B2B_MarketplaceChannel_Code",
                table: "RII_B2B_MARKETPLACE_CHANNEL",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_MarketplaceChannel_IsActive",
                table: "RII_B2B_MARKETPLACE_CHANNEL",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_MarketplaceChannel_ProviderSeller",
                table: "RII_B2B_MARKETPLACE_CHANNEL",
                columns: new[] { "ProviderKey", "SellerId" });

            migrationBuilder.CreateIndex(
                name: "IX_B2B_MarketplaceListing_CatalogProduct",
                table: "RII_B2B_MARKETPLACE_LISTING",
                column: "CatalogProductId");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_MarketplaceListing_ChannelExternalId",
                table: "RII_B2B_MARKETPLACE_LISTING",
                columns: new[] { "ChannelId", "MarketplaceListingId" });

            migrationBuilder.CreateIndex(
                name: "IX_B2B_MarketplaceListing_ChannelSku",
                table: "RII_B2B_MARKETPLACE_LISTING",
                columns: new[] { "ChannelId", "Sku" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_MarketplaceListing_ErpStock",
                table: "RII_B2B_MARKETPLACE_LISTING",
                column: "ErpStockId");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_MarketplaceListing_Status",
                table: "RII_B2B_MARKETPLACE_LISTING",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_MarketplaceSyncEvent_ChannelStatus",
                table: "RII_B2B_MARKETPLACE_SYNC_EVENT",
                columns: new[] { "ChannelId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_B2B_MarketplaceSyncEvent_Listing",
                table: "RII_B2B_MARKETPLACE_SYNC_EVENT",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_MarketplaceSyncEvent_OperationStatus",
                table: "RII_B2B_MARKETPLACE_SYNC_EVENT",
                columns: new[] { "OperationType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_B2B_MarketplaceSyncEvent_RequestedDate",
                table: "RII_B2B_MARKETPLACE_SYNC_EVENT",
                column: "RequestedDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RII_B2B_MARKETPLACE_SYNC_EVENT");

            migrationBuilder.DropTable(
                name: "RII_B2B_MARKETPLACE_LISTING");

            migrationBuilder.DropTable(
                name: "RII_B2B_MARKETPLACE_CHANNEL");
        }
    }
}
