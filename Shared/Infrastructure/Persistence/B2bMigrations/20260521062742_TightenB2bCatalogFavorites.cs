using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wms.Shared.Infrastructure.Persistence.B2bMigrations
{
    /// <inheritdoc />
    public partial class TightenB2bCatalogFavorites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_B2B_ProductFavorite_IsDeleted",
                table: "RII_B2B_CATALOG_PRODUCT_FAVORITE",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_ProductFavorite_UserId",
                table: "RII_B2B_CATALOG_PRODUCT_FAVORITE",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_CategoryFavorite_IsDeleted",
                table: "RII_B2B_CATALOG_CATEGORY_FAVORITE",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_B2B_CategoryFavorite_UserId",
                table: "RII_B2B_CATALOG_CATEGORY_FAVORITE",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_B2B_CATALOG_CATEGORY_FAVORITE_RII_USERS_UserId",
                table: "RII_B2B_CATALOG_CATEGORY_FAVORITE",
                column: "UserId",
                principalTable: "RII_USERS",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RII_B2B_CATALOG_PRODUCT_FAVORITE_RII_USERS_UserId",
                table: "RII_B2B_CATALOG_PRODUCT_FAVORITE",
                column: "UserId",
                principalTable: "RII_USERS",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RII_B2B_CATALOG_CATEGORY_FAVORITE_RII_USERS_UserId",
                table: "RII_B2B_CATALOG_CATEGORY_FAVORITE");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_B2B_CATALOG_PRODUCT_FAVORITE_RII_USERS_UserId",
                table: "RII_B2B_CATALOG_PRODUCT_FAVORITE");

            migrationBuilder.DropIndex(
                name: "IX_B2B_ProductFavorite_IsDeleted",
                table: "RII_B2B_CATALOG_PRODUCT_FAVORITE");

            migrationBuilder.DropIndex(
                name: "IX_B2B_ProductFavorite_UserId",
                table: "RII_B2B_CATALOG_PRODUCT_FAVORITE");

            migrationBuilder.DropIndex(
                name: "IX_B2B_CategoryFavorite_IsDeleted",
                table: "RII_B2B_CATALOG_CATEGORY_FAVORITE");

            migrationBuilder.DropIndex(
                name: "IX_B2B_CategoryFavorite_UserId",
                table: "RII_B2B_CATALOG_CATEGORY_FAVORITE");
        }
    }
}
