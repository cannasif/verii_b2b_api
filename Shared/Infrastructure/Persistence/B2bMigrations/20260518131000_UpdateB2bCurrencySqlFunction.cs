using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Wms.Infrastructure.Persistence.Context;

#nullable disable

namespace Wms.Shared.Infrastructure.Persistence.B2bMigrations;

[Migration("20260518131000_UpdateB2bCurrencySqlFunction")]
[DbContext(typeof(WmsDbContext))]
public partial class UpdateB2bCurrencySqlFunction : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
IF OBJECT_ID(N'dbo.RII_FN_KUR', N'IF') IS NOT NULL DROP FUNCTION dbo.RII_FN_KUR;
IF OBJECT_ID(N'dbo.RII_FN_KUR', N'TF') IS NOT NULL DROP FUNCTION dbo.RII_FN_KUR;
""");

        migrationBuilder.Sql(
            """
CREATE FUNCTION dbo.RII_FN_KUR
(
    @Tarih DATETIME = NULL,
    @FiyatTipi INT = NULL
)
RETURNS @Result TABLE
(
    DOVIZ_TIPI INT NOT NULL,
    DOVIZ_ISMI NVARCHAR(80) NULL,
    KUR_DEGERI FLOAT NULL
)
AS
BEGIN
    DECLARE @TargetDate DATE = CONVERT(date, ISNULL(@Tarih, GETDATE()));
    DECLARE @EffectivePriceType INT = ISNULL(NULLIF(@FiyatTipi, 0), 1);

    INSERT INTO @Result (DOVIZ_TIPI, DOVIZ_ISMI, KUR_DEGERI)
    VALUES (1, N'TL', 1);

    INSERT INTO @Result (DOVIZ_TIPI, DOVIZ_ISMI, KUR_DEGERI)
    SELECT DISTINCT
        CAST(K.SIRA AS int) AS DOVIZ_TIPI,
        CAST(COALESCE(NULLIF(D.ISIM, N''), NULLIF(D.DOVIZ_ISMI, N''), CONCAT(N'Döviz ', K.SIRA)) AS nvarchar(80)) AS DOVIZ_ISMI,
        CAST(COALESCE(
            CASE @EffectivePriceType
                WHEN 1 THEN K.ALIS
                WHEN 2 THEN K.SATIS
                WHEN 3 THEN K.EFEKTIFALIS
                WHEN 4 THEN K.EFEKTIFSATIS
                ELSE K.ALIS
            END,
            K.ALIS,
            K.SATIS,
            1
        ) AS float) AS KUR_DEGERI
    FROM [V3RIICO]..DOVIZKUR K WITH (NOLOCK)
    LEFT JOIN [V3RIICO]..DOVIZ D WITH (NOLOCK) ON D.SIRA = K.SIRA
    WHERE CONVERT(date, K.TARIH) = @TargetDate
      AND K.SIRA <> 1
      AND NOT EXISTS (SELECT 1 FROM @Result R WHERE R.DOVIZ_TIPI = K.SIRA);

    RETURN;
END;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
IF OBJECT_ID(N'dbo.RII_FN_KUR', N'IF') IS NOT NULL DROP FUNCTION dbo.RII_FN_KUR;
IF OBJECT_ID(N'dbo.RII_FN_KUR', N'TF') IS NOT NULL DROP FUNCTION dbo.RII_FN_KUR;
""");

        migrationBuilder.Sql(
            """
CREATE FUNCTION dbo.RII_FN_KUR
(
    @Tarih DATETIME = NULL,
    @FiyatTipi INT = NULL
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        CAST(1 AS int) AS DOVIZ_TIPI,
        CAST(N'TL' AS nvarchar(80)) AS DOVIZ_ISMI,
        CAST(1 AS float) AS KUR_DEGERI
);
""");
    }
}
