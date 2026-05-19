using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wms.Shared.Infrastructure.Persistence.B2bMigrations
{
    /// <inheritdoc />
    public partial class AddB2bCatalogPimFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                table: "RII_B2B_CATALOG_VARIANT",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaGalleryJson",
                table: "RII_B2B_CATALOG_VARIANT",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "RII_B2B_CATALOG_VARIANT",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "RII_B2B_CATALOG_VARIANT",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttributesJson",
                table: "RII_B2B_CATALOG_PRODUCT",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                table: "RII_B2B_CATALOG_PRODUCT",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BulletPointsJson",
                table: "RII_B2B_CATALOG_PRODUCT",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompletenessScore",
                table: "RII_B2B_CATALOG_PRODUCT",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DocumentsJson",
                table: "RII_B2B_CATALOG_PRODUCT",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManufacturerCode",
                table: "RII_B2B_CATALOG_PRODUCT",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaGalleryJson",
                table: "RII_B2B_CATALOG_PRODUCT",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaDescription",
                table: "RII_B2B_CATALOG_PRODUCT",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaTitle",
                table: "RII_B2B_CATALOG_PRODUCT",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinOrderQuantity",
                table: "RII_B2B_CATALOG_PRODUCT",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PackageQuantity",
                table: "RII_B2B_CATALOG_PRODUCT",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductType",
                table: "RII_B2B_CATALOG_PRODUCT",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SearchKeywords",
                table: "RII_B2B_CATALOG_PRODUCT",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortDescription",
                table: "RII_B2B_CATALOG_PRODUCT",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "RII_B2B_CATALOG_PRODUCT",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "RII_B2B_CATALOG_PRODUCT",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_B2B_CatalogProduct_CategoryPath",
                table: "RII_B2B_CATALOG_PRODUCT",
                column: "CategoryPath");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_CatalogProduct_ManufacturerCode",
                table: "RII_B2B_CATALOG_PRODUCT",
                column: "ManufacturerCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_B2B_CatalogProduct_CategoryPath",
                table: "RII_B2B_CATALOG_PRODUCT");

            migrationBuilder.DropIndex(
                name: "IX_B2B_CatalogProduct_ManufacturerCode",
                table: "RII_B2B_CATALOG_PRODUCT");

            migrationBuilder.DropColumn(
                name: "Barcode",
                table: "RII_B2B_CATALOG_VARIANT");

            migrationBuilder.DropColumn(
                name: "MediaGalleryJson",
                table: "RII_B2B_CATALOG_VARIANT");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "RII_B2B_CATALOG_VARIANT");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "RII_B2B_CATALOG_VARIANT");

            migrationBuilder.DropColumn(
                name: "AttributesJson",
                table: "RII_B2B_CATALOG_PRODUCT");

            migrationBuilder.DropColumn(
                name: "Barcode",
                table: "RII_B2B_CATALOG_PRODUCT");

            migrationBuilder.DropColumn(
                name: "BulletPointsJson",
                table: "RII_B2B_CATALOG_PRODUCT");

            migrationBuilder.DropColumn(
                name: "CompletenessScore",
                table: "RII_B2B_CATALOG_PRODUCT");

            migrationBuilder.DropColumn(
                name: "DocumentsJson",
                table: "RII_B2B_CATALOG_PRODUCT");

            migrationBuilder.DropColumn(
                name: "ManufacturerCode",
                table: "RII_B2B_CATALOG_PRODUCT");

            migrationBuilder.DropColumn(
                name: "MediaGalleryJson",
                table: "RII_B2B_CATALOG_PRODUCT");

            migrationBuilder.DropColumn(
                name: "MetaDescription",
                table: "RII_B2B_CATALOG_PRODUCT");

            migrationBuilder.DropColumn(
                name: "MetaTitle",
                table: "RII_B2B_CATALOG_PRODUCT");

            migrationBuilder.DropColumn(
                name: "MinOrderQuantity",
                table: "RII_B2B_CATALOG_PRODUCT");

            migrationBuilder.DropColumn(
                name: "PackageQuantity",
                table: "RII_B2B_CATALOG_PRODUCT");

            migrationBuilder.DropColumn(
                name: "ProductType",
                table: "RII_B2B_CATALOG_PRODUCT");

            migrationBuilder.DropColumn(
                name: "SearchKeywords",
                table: "RII_B2B_CATALOG_PRODUCT");

            migrationBuilder.DropColumn(
                name: "ShortDescription",
                table: "RII_B2B_CATALOG_PRODUCT");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "RII_B2B_CATALOG_PRODUCT");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "RII_B2B_CATALOG_PRODUCT");
        }
    }
}
