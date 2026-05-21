using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wms.Shared.Infrastructure.Persistence.B2bMigrations
{
    /// <inheritdoc />
    public partial class AddB2bCatalogFavorites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RII_B2B_CATALOG_CATEGORY_FAVORITE",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false),
                    BuyerId = table.Column<long>(type: "bigint", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    CatalogCategoryId = table.Column<long>(type: "bigint", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_RII_B2B_CATALOG_CATEGORY_FAVORITE", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_B2B_CATALOG_CATEGORY_FAVORITE_RII_B2B_BUYER_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "RII_B2B_BUYER",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RII_B2B_CATALOG_CATEGORY_FAVORITE_RII_B2B_CATALOG_CATEGORY_CatalogCategoryId",
                        column: x => x.CatalogCategoryId,
                        principalTable: "RII_B2B_CATALOG_CATEGORY",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RII_B2B_CATALOG_CATEGORY_FAVORITE_RII_B2B_COMPANY_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "RII_B2B_COMPANY",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RII_B2B_CATALOG_PRODUCT_FAVORITE",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false),
                    BuyerId = table.Column<long>(type: "bigint", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    CatalogProductId = table.Column<long>(type: "bigint", nullable: true),
                    CatalogVariantId = table.Column<long>(type: "bigint", nullable: true),
                    ErpStockId = table.Column<long>(type: "bigint", nullable: true),
                    FavoriteKey = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_RII_B2B_CATALOG_PRODUCT_FAVORITE", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_B2B_CATALOG_PRODUCT_FAVORITE_RII_B2B_BUYER_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "RII_B2B_BUYER",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RII_B2B_CATALOG_PRODUCT_FAVORITE_RII_B2B_CATALOG_PRODUCT_CatalogProductId",
                        column: x => x.CatalogProductId,
                        principalTable: "RII_B2B_CATALOG_PRODUCT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RII_B2B_CATALOG_PRODUCT_FAVORITE_RII_B2B_CATALOG_VARIANT_CatalogVariantId",
                        column: x => x.CatalogVariantId,
                        principalTable: "RII_B2B_CATALOG_VARIANT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RII_B2B_CATALOG_PRODUCT_FAVORITE_RII_B2B_COMPANY_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "RII_B2B_COMPANY",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_B2B_CategoryFavorite_CategoryId",
                table: "RII_B2B_CATALOG_CATEGORY_FAVORITE",
                column: "CatalogCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_CategoryFavorite_ScopeCategory",
                table: "RII_B2B_CATALOG_CATEGORY_FAVORITE",
                columns: new[] { "CompanyId", "BuyerId", "UserId", "CatalogCategoryId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RII_B2B_CATALOG_CATEGORY_FAVORITE_BuyerId",
                table: "RII_B2B_CATALOG_CATEGORY_FAVORITE",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_ProductFavorite_ErpStockId",
                table: "RII_B2B_CATALOG_PRODUCT_FAVORITE",
                column: "ErpStockId");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_ProductFavorite_ProductId",
                table: "RII_B2B_CATALOG_PRODUCT_FAVORITE",
                column: "CatalogProductId");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_ProductFavorite_ScopeKey",
                table: "RII_B2B_CATALOG_PRODUCT_FAVORITE",
                columns: new[] { "CompanyId", "BuyerId", "UserId", "FavoriteKey" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_ProductFavorite_VariantId",
                table: "RII_B2B_CATALOG_PRODUCT_FAVORITE",
                column: "CatalogVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_B2B_CATALOG_PRODUCT_FAVORITE_BuyerId",
                table: "RII_B2B_CATALOG_PRODUCT_FAVORITE",
                column: "BuyerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RII_B2B_CATALOG_CATEGORY_FAVORITE");

            migrationBuilder.DropTable(
                name: "RII_B2B_CATALOG_PRODUCT_FAVORITE");
        }
    }
}
