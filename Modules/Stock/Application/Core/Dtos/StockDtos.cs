using System.ComponentModel.DataAnnotations;
using Wms.Application.Common;

namespace Wms.Application.Stock.Dtos;

public sealed class StockDto : BaseEntityDto
{
    public string ErpStockCode { get; set; } = string.Empty;
    public string StockName { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? UreticiKodu { get; set; }
    public string? GrupKodu { get; set; }
    public string? GrupAdi { get; set; }
    public string? Kod1 { get; set; }
    public string? Kod1Adi { get; set; }
    public string? Kod2 { get; set; }
    public string? Kod2Adi { get; set; }
    public string? Kod3 { get; set; }
    public string? Kod3Adi { get; set; }
    public string? Kod4 { get; set; }
    public string? Kod4Adi { get; set; }
    public string? Kod5 { get; set; }
    public string? Kod5Adi { get; set; }
    public decimal? SalesPrice1 { get; set; }
    public decimal? SalesPrice2 { get; set; }
    public decimal? SalesPrice3 { get; set; }
    public decimal? SalesPrice4 { get; set; }
    public decimal? VatRate { get; set; }
    public DateTime? LastSyncDate { get; set; }
}

public sealed class CreateStockDto
{
    [Required, StringLength(50)]
    public string ErpStockCode { get; set; } = string.Empty;
    [Required, StringLength(250)]
    public string StockName { get; set; } = string.Empty;
    [StringLength(20)] public string? Unit { get; set; }
    [StringLength(50)] public string? UreticiKodu { get; set; }
    [StringLength(50)] public string? GrupKodu { get; set; }
    [StringLength(250)] public string? GrupAdi { get; set; }
    [StringLength(50)] public string? Kod1 { get; set; }
    [StringLength(250)] public string? Kod1Adi { get; set; }
    [StringLength(50)] public string? Kod2 { get; set; }
    [StringLength(250)] public string? Kod2Adi { get; set; }
    [StringLength(50)] public string? Kod3 { get; set; }
    [StringLength(250)] public string? Kod3Adi { get; set; }
    [StringLength(50)] public string? Kod4 { get; set; }
    [StringLength(250)] public string? Kod4Adi { get; set; }
    [StringLength(50)] public string? Kod5 { get; set; }
    [StringLength(250)] public string? Kod5Adi { get; set; }
    public decimal? SalesPrice1 { get; set; }
    public decimal? SalesPrice2 { get; set; }
    public decimal? SalesPrice3 { get; set; }
    public decimal? SalesPrice4 { get; set; }
    public decimal? VatRate { get; set; }
    public string BranchCode { get; set; } = "0";
}

public sealed class UpdateStockDto
{
    [StringLength(50)] public string? ErpStockCode { get; set; }
    [StringLength(250)] public string? StockName { get; set; }
    [StringLength(20)] public string? Unit { get; set; }
    [StringLength(50)] public string? UreticiKodu { get; set; }
    [StringLength(50)] public string? GrupKodu { get; set; }
    [StringLength(250)] public string? GrupAdi { get; set; }
    [StringLength(50)] public string? Kod1 { get; set; }
    [StringLength(250)] public string? Kod1Adi { get; set; }
    [StringLength(50)] public string? Kod2 { get; set; }
    [StringLength(250)] public string? Kod2Adi { get; set; }
    [StringLength(50)] public string? Kod3 { get; set; }
    [StringLength(250)] public string? Kod3Adi { get; set; }
    [StringLength(50)] public string? Kod4 { get; set; }
    [StringLength(250)] public string? Kod4Adi { get; set; }
    [StringLength(50)] public string? Kod5 { get; set; }
    [StringLength(250)] public string? Kod5Adi { get; set; }
    public decimal? SalesPrice1 { get; set; }
    public decimal? SalesPrice2 { get; set; }
    public decimal? SalesPrice3 { get; set; }
    public decimal? SalesPrice4 { get; set; }
    public decimal? VatRate { get; set; }
    public string? BranchCode { get; set; }
}

public sealed class SyncStockDto
{
    [Required, StringLength(50)]
    public string ErpStockCode { get; set; } = string.Empty;
    [Required, StringLength(250)]
    public string StockName { get; set; } = string.Empty;
    [StringLength(20)] public string? Unit { get; set; }
    [StringLength(50)] public string? UreticiKodu { get; set; }
    [StringLength(50)] public string? GrupKodu { get; set; }
    [StringLength(250)] public string? GrupAdi { get; set; }
    [StringLength(50)] public string? Kod1 { get; set; }
    [StringLength(250)] public string? Kod1Adi { get; set; }
    [StringLength(50)] public string? Kod2 { get; set; }
    [StringLength(250)] public string? Kod2Adi { get; set; }
    [StringLength(50)] public string? Kod3 { get; set; }
    [StringLength(250)] public string? Kod3Adi { get; set; }
    [StringLength(50)] public string? Kod4 { get; set; }
    [StringLength(250)] public string? Kod4Adi { get; set; }
    [StringLength(50)] public string? Kod5 { get; set; }
    [StringLength(250)] public string? Kod5Adi { get; set; }
    public decimal? SalesPrice1 { get; set; }
    public decimal? SalesPrice2 { get; set; }
    public decimal? SalesPrice3 { get; set; }
    public decimal? SalesPrice4 { get; set; }
    public decimal? VatRate { get; set; }
    public string BranchCode { get; set; } = "0";
}

public sealed class StockDetailDto : BaseEntityDto
{
    public long StockId { get; set; }
    public string ErpStockCode { get; set; } = string.Empty;
    public string StockName { get; set; } = string.Empty;
    public string HtmlDescription { get; set; } = string.Empty;
    public string? TechnicalSpecsJson { get; set; }
}

public sealed class CreateStockDetailDto
{
    [Required] public long StockId { get; set; }
    public string HtmlDescription { get; set; } = string.Empty;
    public string? TechnicalSpecsJson { get; set; }
}

public sealed class UpdateStockDetailDto
{
    [Required] public long StockId { get; set; }
    public string HtmlDescription { get; set; } = string.Empty;
    public string? TechnicalSpecsJson { get; set; }
}

public sealed class StockImageDto : BaseEntityDto
{
    public long StockId { get; set; }
    public string ErpStockCode { get; set; } = string.Empty;
    public string StockName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
}
