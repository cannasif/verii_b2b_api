using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wms.Shared.Infrastructure.Persistence.B2bMigrations
{
    /// <inheritdoc />
    public partial class AddB2bCatalogGovernance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RII_B2B_CATALOG_CATEGORY",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentCategoryId = table.Column<long>(type: "bigint", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    FullPath = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IconName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    ColorHex = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsLeaf = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_RII_B2B_CATALOG_CATEGORY", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_B2B_CATALOG_CATEGORY_RII_B2B_CATALOG_CATEGORY_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "RII_B2B_CATALOG_CATEGORY",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RII_B2B_CATALOG_PRODUCT_DOCUMENT",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CatalogProductId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_RII_B2B_CATALOG_PRODUCT_DOCUMENT", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_B2B_CATALOG_PRODUCT_DOCUMENT_RII_B2B_CATALOG_PRODUCT_CatalogProductId",
                        column: x => x.CatalogProductId,
                        principalTable: "RII_B2B_CATALOG_PRODUCT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RII_B2B_CATALOG_PRODUCT_MEDIA",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CatalogProductId = table.Column<long>(type: "bigint", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MediaType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    AltText = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_RII_B2B_CATALOG_PRODUCT_MEDIA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_B2B_CATALOG_PRODUCT_MEDIA_RII_B2B_CATALOG_PRODUCT_CatalogProductId",
                        column: x => x.CatalogProductId,
                        principalTable: "RII_B2B_CATALOG_PRODUCT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RII_B2B_CATALOG_ATTRIBUTE_DEFINITION",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CatalogCategoryId = table.Column<long>(type: "bigint", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsFilterable = table.Column<bool>(type: "bit", nullable: false),
                    IsComparable = table.Column<bool>(type: "bit", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    AllowedValuesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_RII_B2B_CATALOG_ATTRIBUTE_DEFINITION", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_B2B_CATALOG_ATTRIBUTE_DEFINITION_RII_B2B_CATALOG_CATEGORY_CatalogCategoryId",
                        column: x => x.CatalogCategoryId,
                        principalTable: "RII_B2B_CATALOG_CATEGORY",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RII_B2B_CATALOG_PRODUCT_CATEGORY",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CatalogProductId = table.Column<long>(type: "bigint", nullable: false),
                    CatalogCategoryId = table.Column<long>(type: "bigint", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    AssignmentSource = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
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
                    table.PrimaryKey("PK_RII_B2B_CATALOG_PRODUCT_CATEGORY", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_B2B_CATALOG_PRODUCT_CATEGORY_RII_B2B_CATALOG_CATEGORY_CatalogCategoryId",
                        column: x => x.CatalogCategoryId,
                        principalTable: "RII_B2B_CATALOG_CATEGORY",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RII_B2B_CATALOG_PRODUCT_CATEGORY_RII_B2B_CATALOG_PRODUCT_CatalogProductId",
                        column: x => x.CatalogProductId,
                        principalTable: "RII_B2B_CATALOG_PRODUCT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RII_B2B_CATALOG_PRODUCT_ATTRIBUTE",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CatalogProductId = table.Column<long>(type: "bigint", nullable: false),
                    AttributeDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    NormalizedValue = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_RII_B2B_CATALOG_PRODUCT_ATTRIBUTE", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_B2B_CATALOG_PRODUCT_ATTRIBUTE_RII_B2B_CATALOG_ATTRIBUTE_DEFINITION_AttributeDefinitionId",
                        column: x => x.AttributeDefinitionId,
                        principalTable: "RII_B2B_CATALOG_ATTRIBUTE_DEFINITION",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RII_B2B_CATALOG_PRODUCT_ATTRIBUTE_RII_B2B_CATALOG_PRODUCT_CatalogProductId",
                        column: x => x.CatalogProductId,
                        principalTable: "RII_B2B_CATALOG_PRODUCT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_B2B_CatalogAttributeDefinition_ActiveRequiredSort",
                table: "RII_B2B_CATALOG_ATTRIBUTE_DEFINITION",
                columns: new[] { "IsActive", "IsRequired", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_B2B_CatalogAttributeDefinition_CategoryCode",
                table: "RII_B2B_CATALOG_ATTRIBUTE_DEFINITION",
                columns: new[] { "CatalogCategoryId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_CatalogCategory_ActiveSort",
                table: "RII_B2B_CATALOG_CATEGORY",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_B2B_CatalogCategory_Code",
                table: "RII_B2B_CATALOG_CATEGORY",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_CatalogCategory_FullPath",
                table: "RII_B2B_CATALOG_CATEGORY",
                column: "FullPath");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_CatalogCategory_ParentId",
                table: "RII_B2B_CATALOG_CATEGORY",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_CatalogProductAttribute_Filter",
                table: "RII_B2B_CATALOG_PRODUCT_ATTRIBUTE",
                columns: new[] { "AttributeDefinitionId", "NormalizedValue" });

            migrationBuilder.CreateIndex(
                name: "IX_B2B_CatalogProductAttribute_ProductAttribute",
                table: "RII_B2B_CATALOG_PRODUCT_ATTRIBUTE",
                columns: new[] { "CatalogProductId", "AttributeDefinitionId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_CatalogProductCategory_CategorySort",
                table: "RII_B2B_CATALOG_PRODUCT_CATEGORY",
                columns: new[] { "CatalogCategoryId", "IsPrimary", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_B2B_CatalogProductCategory_ProductCategory",
                table: "RII_B2B_CATALOG_PRODUCT_CATEGORY",
                columns: new[] { "CatalogProductId", "CatalogCategoryId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_CatalogProductDocument_ProductSort",
                table: "RII_B2B_CATALOG_PRODUCT_DOCUMENT",
                columns: new[] { "CatalogProductId", "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_B2B_CatalogProductMedia_ProductSort",
                table: "RII_B2B_CATALOG_PRODUCT_MEDIA",
                columns: new[] { "CatalogProductId", "IsPrimary", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RII_B2B_CATALOG_PRODUCT_ATTRIBUTE");

            migrationBuilder.DropTable(
                name: "RII_B2B_CATALOG_PRODUCT_CATEGORY");

            migrationBuilder.DropTable(
                name: "RII_B2B_CATALOG_PRODUCT_DOCUMENT");

            migrationBuilder.DropTable(
                name: "RII_B2B_CATALOG_PRODUCT_MEDIA");

            migrationBuilder.DropTable(
                name: "RII_B2B_CATALOG_ATTRIBUTE_DEFINITION");

            migrationBuilder.DropTable(
                name: "RII_B2B_CATALOG_CATEGORY");
        }
    }
}
