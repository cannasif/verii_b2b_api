using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wms.Shared.Infrastructure.Persistence.B2bMigrations
{
    /// <inheritdoc />
    public partial class AddB2bCrmHeaderProjectCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErpProjectCode",
                table: "RII_B2B_QUOTE_REQUEST",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErpProjectCode",
                table: "RII_B2B_ORDER",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErpProjectCode",
                table: "RII_B2B_QUOTE_REQUEST");

            migrationBuilder.DropColumn(
                name: "ErpProjectCode",
                table: "RII_B2B_ORDER");
        }
    }
}
