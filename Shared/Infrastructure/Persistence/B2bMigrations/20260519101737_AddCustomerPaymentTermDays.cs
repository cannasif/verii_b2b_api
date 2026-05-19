using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wms.Shared.Infrastructure.Persistence.B2bMigrations
{
    /// <inheritdoc />
    public partial class AddCustomerPaymentTermDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "PaymentTermDays",
                table: "RII_WMS_CUSTOMER",
                type: "smallint",
                nullable: true);

            migrationBuilder.Sql("""
CREATE OR ALTER FUNCTION dbo.RII_FN_CARI
(
    @CariKodu NVARCHAR(MAX) = NULL,
    @SubeKodu NVARCHAR(MAX) = NULL
)
RETURNS TABLE
AS
RETURN
(
    WITH CARI_LIST AS
    (
        SELECT LTRIM(RTRIM(value)) AS CARI_KODU
        FROM STRING_SPLIT(ISNULL(@CariKodu, ''), ',')
        WHERE LTRIM(RTRIM(value)) <> ''
    ),
    SUBE_LIST AS
    (
        SELECT LTRIM(RTRIM(value)) AS SUBE_KODU
        FROM STRING_SPLIT(ISNULL(@SubeKodu, ''), ',')
        WHERE LTRIM(RTRIM(value)) <> ''
    )
    SELECT
        CAST(CS.SUBE_KODU AS smallint) AS SUBE_KODU,
        CAST(CS.ISLETME_KODU AS smallint) AS ISLETME_KODU,
        CAST(CS.CARI_KOD AS nvarchar(50)) AS CARI_KOD,
        CAST(CS.CARI_TEL AS nvarchar(100)) AS CARI_TEL,
        CAST(CS.CARI_IL AS nvarchar(100)) AS CARI_IL,
        CAST(CS.ULKE_KODU AS nvarchar(50)) AS ULKE_KODU,
        CAST(CS.CARI_ISIM AS nvarchar(250)) AS CARI_ISIM,
        CAST(CS.CARI_TIP AS nvarchar(20)) AS CARI_TIP,
        CAST(CS.GRUP_KODU AS nvarchar(50)) AS GRUP_KODU,
        CAST(CS.CARI_ADRES AS nvarchar(500)) AS CARI_ADRES,
        CAST(CS.CARI_ILCE AS nvarchar(100)) AS CARI_ILCE,
        CAST(CS.VERGI_DAIRESI AS nvarchar(100)) AS VERGI_DAIRESI,
        CAST(CS.VERGI_NUMARASI AS nvarchar(50)) AS VERGI_NUMARASI,
        CAST(CS.FAX AS nvarchar(100)) AS FAX,
        CAST(CS.POSTAKODU AS nvarchar(50)) AS POSTAKODU,
        CAST(CS.RISK_SINIRI AS decimal(18,4)) AS RISK_SINIRI,
        CAST(CS.LISTE_FIATI AS smallint) AS LISTE_FIATI,
        CAST(CS.VADE_GUNU AS smallint) AS VADE_GUNU,
        CAST(CS.ACIK1 AS nvarchar(250)) AS ACIK1,
        CAST(CS.ACIK2 AS nvarchar(250)) AS ACIK2,
        CAST(CS.ACIK3 AS nvarchar(250)) AS ACIK3
    FROM [V3RIICO]..TBLCASABIT CS WITH (NOLOCK)
    WHERE (NOT EXISTS (SELECT 1 FROM CARI_LIST) OR CS.CARI_KOD IN (SELECT CARI_KODU FROM CARI_LIST))
      AND (NOT EXISTS (SELECT 1 FROM SUBE_LIST) OR CAST(CS.SUBE_KODU AS nvarchar(20)) IN (SELECT SUBE_KODU FROM SUBE_LIST))
);
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
CREATE OR ALTER FUNCTION dbo.RII_FN_CARI
(
    @CariKodu NVARCHAR(MAX) = NULL,
    @SubeKodu NVARCHAR(MAX) = NULL
)
RETURNS TABLE
AS
RETURN
(
    WITH CARI_LIST AS
    (
        SELECT LTRIM(RTRIM(value)) AS CARI_KODU
        FROM STRING_SPLIT(ISNULL(@CariKodu, ''), ',')
        WHERE LTRIM(RTRIM(value)) <> ''
    ),
    SUBE_LIST AS
    (
        SELECT LTRIM(RTRIM(value)) AS SUBE_KODU
        FROM STRING_SPLIT(ISNULL(@SubeKodu, ''), ',')
        WHERE LTRIM(RTRIM(value)) <> ''
    )
    SELECT
        CAST(CS.SUBE_KODU AS smallint) AS SUBE_KODU,
        CAST(CS.ISLETME_KODU AS smallint) AS ISLETME_KODU,
        CAST(CS.CARI_KOD AS nvarchar(50)) AS CARI_KOD,
        CAST(CS.CARI_TEL AS nvarchar(100)) AS CARI_TEL,
        CAST(CS.CARI_IL AS nvarchar(100)) AS CARI_IL,
        CAST(CS.ULKE_KODU AS nvarchar(50)) AS ULKE_KODU,
        CAST(CS.CARI_ISIM AS nvarchar(250)) AS CARI_ISIM,
        CAST(CS.CARI_TIP AS nvarchar(20)) AS CARI_TIP,
        CAST(CS.GRUP_KODU AS nvarchar(50)) AS GRUP_KODU,
        CAST(CS.CARI_ADRES AS nvarchar(500)) AS CARI_ADRES,
        CAST(CS.CARI_ILCE AS nvarchar(100)) AS CARI_ILCE,
        CAST(CS.VERGI_DAIRESI AS nvarchar(100)) AS VERGI_DAIRESI,
        CAST(CS.VERGI_NUMARASI AS nvarchar(50)) AS VERGI_NUMARASI,
        CAST(CS.FAX AS nvarchar(100)) AS FAX,
        CAST(CS.POSTAKODU AS nvarchar(50)) AS POSTAKODU,
        CAST(CS.RISK_SINIRI AS decimal(18,4)) AS RISK_SINIRI,
        CAST(CS.LISTE_FIATI AS smallint) AS LISTE_FIATI,
        CAST(CS.ACIK1 AS nvarchar(250)) AS ACIK1,
        CAST(CS.ACIK2 AS nvarchar(250)) AS ACIK2,
        CAST(CS.ACIK3 AS nvarchar(250)) AS ACIK3
    FROM [V3RIICO]..TBLCASABIT CS WITH (NOLOCK)
    WHERE (NOT EXISTS (SELECT 1 FROM CARI_LIST) OR CS.CARI_KOD IN (SELECT CARI_KODU FROM CARI_LIST))
      AND (NOT EXISTS (SELECT 1 FROM SUBE_LIST) OR CAST(CS.SUBE_KODU AS nvarchar(20)) IN (SELECT SUBE_KODU FROM SUBE_LIST))
);
""");

            migrationBuilder.DropColumn(
                name: "PaymentTermDays",
                table: "RII_WMS_CUSTOMER");
        }
    }
}
