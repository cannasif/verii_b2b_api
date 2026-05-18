using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Wms.Infrastructure.Persistence.Context;

#nullable disable

namespace Wms.Shared.Infrastructure.Persistence.B2bMigrations;

[Migration("20260514155500_AddB2bErpSqlFunctions")]
[DbContext(typeof(WmsDbContext))]
public partial class AddB2bErpSqlFunctions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var sql in GetSqlModules())
        {
            migrationBuilder.Sql(sql);
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        var names = new[]
        {
            "RII_FN_2SHIPPING",
            "RII_FN_CARIBAKIYE",
            "RII_FN_KUR",
            "RII_FN_ESNYAPMAS",
            "RII_FN_DEPO",
            "RII_FN_STOK",
            "RII_FN_CARI",
            "RII_FN_BRANCHES"
        };

        foreach (var name in names)
        {
            migrationBuilder.Sql($"DROP FUNCTION IF EXISTS dbo.{name};");
        }
    }

    private static IReadOnlyList<string> GetSqlModules() => new[]
    {
        """
CREATE OR ALTER FUNCTION dbo.RII_FN_BRANCHES
(
    @branchNo INT = NULL
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        CAST(SUBE_KODU AS smallint) AS SUBE_KODU,
        CAST(UNVAN AS nvarchar(200)) AS UNVAN
    FROM [V3RIICO]..TBLSUBELER WITH (NOLOCK)
    WHERE SUBE_KODU NOT IN (-1, 32767)
      AND (@branchNo IS NULL OR SUBE_KODU = @branchNo)
);
""",
        """
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
""",
        """
CREATE OR ALTER FUNCTION dbo.RII_FN_STOK
(
    @StokKodu NVARCHAR(MAX) = NULL,
    @SubeKodu NVARCHAR(MAX) = NULL
)
RETURNS TABLE
AS
RETURN
(
    WITH STOK_LIST AS
    (
        SELECT LTRIM(RTRIM(value)) AS STOK_KODU
        FROM STRING_SPLIT(ISNULL(@StokKodu, ''), ',')
        WHERE LTRIM(RTRIM(value)) <> ''
    ),
    SUBE_LIST AS
    (
        SELECT LTRIM(RTRIM(value)) AS SUBE_KODU
        FROM STRING_SPLIT(ISNULL(@SubeKodu, ''), ',')
        WHERE LTRIM(RTRIM(value)) <> ''
    )
    SELECT
        CAST(S.SUBE_KODU AS smallint) AS SUBE_KODU,
        CAST(S.ISLETME_KODU AS smallint) AS ISLETME_KODU,
        CAST(S.STOK_KODU AS nvarchar(80)) AS STOK_KODU,
        CAST(S.URETICI_KODU AS nvarchar(80)) AS URETICI_KODU,
        CAST(S.STOK_ADI AS nvarchar(250)) AS STOK_ADI,
        CAST(S.GRUP_KODU AS nvarchar(80)) AS GRUP_KODU,
        CAST(S.KOD_1 AS nvarchar(80)) AS KOD_1,
        CAST(S.KOD_2 AS nvarchar(80)) AS KOD_2,
        CAST(S.KOD_3 AS nvarchar(80)) AS KOD_3,
        CAST(S.KOD_4 AS nvarchar(80)) AS KOD_4,
        CAST(S.KOD_5 AS nvarchar(80)) AS KOD_5,
        CAST(S.OLCU_BR1 AS nvarchar(30)) AS OLCU_BR1,
        CAST(S.SATIS_FIAT1 AS decimal(18,4)) AS SATIS_FIAT1,
        CAST(S.SATIS_FIAT2 AS decimal(18,4)) AS SATIS_FIAT2,
        CAST(S.SATIS_FIAT3 AS decimal(18,4)) AS SATIS_FIAT3,
        CAST(S.SATIS_FIAT4 AS decimal(18,4)) AS SATIS_FIAT4,
        CAST(S.KDV_ORANI AS decimal(18,4)) AS KDV_ORANI,
        CAST(S.BARKOD1 AS nvarchar(120)) AS BARKOD1,
        CAST(S.BARKOD2 AS nvarchar(120)) AS BARKOD2,
        CAST(S.BARKOD3 AS nvarchar(120)) AS BARKOD3,
        CAST(SE.INGISIM AS nvarchar(250)) AS INGISIM
    FROM [V3RIICO]..TBLSTSABIT S WITH (NOLOCK)
    LEFT JOIN [V3RIICO]..TBLSTSABITEK SE WITH (NOLOCK) ON SE.STOK_KODU = S.STOK_KODU
    WHERE (NOT EXISTS (SELECT 1 FROM STOK_LIST) OR S.STOK_KODU IN (SELECT STOK_KODU FROM STOK_LIST))
      AND (NOT EXISTS (SELECT 1 FROM SUBE_LIST) OR CAST(S.SUBE_KODU AS nvarchar(20)) IN (SELECT SUBE_KODU FROM SUBE_LIST))
);
""",
        """
CREATE OR ALTER FUNCTION dbo.RII_FN_DEPO
(
    @DepoKodu NVARCHAR(MAX) = NULL,
    @SubeKodu NVARCHAR(MAX) = NULL
)
RETURNS TABLE
AS
RETURN
(
    WITH DEPO_LIST AS
    (
        SELECT LTRIM(RTRIM(value)) AS DEPO_KODU
        FROM STRING_SPLIT(ISNULL(@DepoKodu, ''), ',')
        WHERE LTRIM(RTRIM(value)) <> ''
    ),
    SUBE_LIST AS
    (
        SELECT LTRIM(RTRIM(value)) AS SUBE_KODU
        FROM STRING_SPLIT(ISNULL(@SubeKodu, ''), ',')
        WHERE LTRIM(RTRIM(value)) <> ''
    )
    SELECT
        CAST(D.DEPO_KODU AS smallint) AS DEPO_KODU,
        CAST(D.DEPO_ISMI AS nvarchar(180)) AS DEPO_ISMI,
        CAST(D.SUBE_KODU AS smallint) AS SUBE_KODU
    FROM [V3RIICO]..TBLSTOKDP D WITH (NOLOCK)
    WHERE (NOT EXISTS (SELECT 1 FROM DEPO_LIST) OR CAST(D.DEPO_KODU AS nvarchar(20)) IN (SELECT DEPO_KODU FROM DEPO_LIST))
      AND (NOT EXISTS (SELECT 1 FROM SUBE_LIST) OR CAST(D.SUBE_KODU AS nvarchar(20)) IN (SELECT SUBE_KODU FROM SUBE_LIST))
);
""",
        """
CREATE OR ALTER FUNCTION dbo.RII_FN_ESNYAPMAS()
RETURNS TABLE
AS
RETURN
(
    SELECT
        CAST(Y.YAPKOD AS nvarchar(80)) AS YAPKOD,
        CAST(Y.YAPACIK AS nvarchar(250)) AS YAPACIK,
        CAST(Y.SUBE_KODU AS smallint) AS SUBE_KODU,
        CAST(Y.YPLNDRSTOKKOD AS nvarchar(80)) AS YPLNDRSTOKKOD,
        CAST(NULL AS bigint) AS StockId
    FROM [V3RIICO]..TBLESNYAPMAS Y WITH (NOLOCK)
);
""",
        """
CREATE OR ALTER FUNCTION dbo.RII_FN_KUR
(
    @Tarih DATETIME = NULL,
    @FiyatTipi INT = NULL
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        CAST(NULL AS int) AS DOVIZ_TIPI,
        CAST(NULL AS nvarchar(80)) AS DOVIZ_ISMI,
        CAST(NULL AS float) AS KUR_DEGERI
    WHERE 1 = 0
);
""",
        """
CREATE OR ALTER FUNCTION dbo.RII_FN_CARIBAKIYE
(
    @CariKodu NVARCHAR(50)
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        CAST(@CariKodu AS nvarchar(50)) AS CARI_KOD,
        CAST(ISNULL(SUM(ISNULL(BORC, 0) - ISNULL(ALACAK, 0)), 0) AS decimal(18,4)) AS NET_BAKIYE,
        CAST(CASE WHEN ISNULL(SUM(ISNULL(BORC, 0) - ISNULL(ALACAK, 0)), 0) >= 0 THEN N'BORC' ELSE N'ALACAK' END AS nvarchar(14)) AS BAKIYE_DURUMU,
        CAST(ABS(ISNULL(SUM(ISNULL(BORC, 0) - ISNULL(ALACAK, 0)), 0)) AS decimal(18,4)) AS BAKIYE_TUTARI
    FROM [V3RIICO]..TBLCAHAR WITH (NOLOCK)
    WHERE CARI_KOD = @CariKodu
);
""",
        """
CREATE OR ALTER FUNCTION dbo.RII_FN_2SHIPPING
(
    @CariKodu NVARCHAR(50)
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        CAST(CARI_KOD AS nvarchar(50)) AS CARI_KOD,
        CAST(CARI_ISIM AS nvarchar(250)) AS CARI_ISIM,
        CAST(CARI_ADRES AS nvarchar(500)) AS CARI_ADRES,
        CAST(CARI_IL AS nvarchar(100)) AS CARI_IL,
        CAST(CARI_ILCE AS nvarchar(100)) AS CARI_ILCE
    FROM [V3RIICO]..TBLCASABIT WITH (NOLOCK)
    WHERE CARI_KOD = @CariKodu
);
"""
    };
}
