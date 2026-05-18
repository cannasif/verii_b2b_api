using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Wms.Infrastructure.Persistence.Context;

#nullable disable

namespace Wms.Shared.Infrastructure.Persistence.B2bMigrations
{
    /// <inheritdoc />
    [Migration("20260514161000_DropUnusedB2bArtifacts")]
    [DbContext(typeof(WmsDbContext))]
    public partial class DropUnusedB2bArtifacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[RII_BARCODE_DEFINITION]', N'U') IS NOT NULL
                    DROP TABLE [RII_BARCODE_DEFINITION];
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RII_BARCODE_DEFINITION",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DefinitionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "pattern"),
                    SegmentPattern = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    RequiredSegments = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AllowMirrorLookup = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    HintText = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_RII_BARCODE_DEFINITION", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UX_RII_BARCODE_DEFINITION_BRANCH_MODULE",
                table: "RII_BARCODE_DEFINITION",
                columns: new[] { "BranchCode", "ModuleKey" },
                unique: true);
        }
    }
}
