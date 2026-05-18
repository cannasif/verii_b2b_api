using System.ComponentModel.DataAnnotations;

namespace Wms.Modules.NetsisIntegrations.Application.Dtos;

public sealed class OnHandQuantityDto
{
    public int DepoKodu { get; set; }
    public string? StokKodu { get; set; }
    public string? ProjeKodu { get; set; }
    public string? SeriNo { get; set; }
    public string? HucreKodu { get; set; }
    public string? Kaynak { get; set; }
    public decimal? Bakiye { get; set; }
    public string? StokAdi { get; set; }
    public string? DepoIsmi { get; set; }
}
public sealed class CariDto
{
    public short SubeKodu { get; set; }
    public short IsletmeKodu { get; set; }
    public string CariKod { get; set; } = string.Empty;
    public string? CariIsim { get; set; }
}
public sealed class StokDto
{
    public short SubeKodu { get; set; }
    public short IsletmeKodu { get; set; }
    public string StokKodu { get; set; } = string.Empty;
    public string UreticiKodu { get; set; } = string.Empty;
    public string StokAdi { get; set; } = string.Empty;
    public string GrupKodu { get; set; } = string.Empty;
    public string Kod1 { get; set; } = string.Empty;
    public string Kod2 { get; set; } = string.Empty;
    public string Kod3 { get; set; } = string.Empty;
    public string Kod4 { get; set; } = string.Empty;
    public string Kod5 { get; set; } = string.Empty;
}
public sealed class StokGrupDto
{
    public short SubeKodu { get; set; }
    public string GrupKod { get; set; } = string.Empty;
    public string? GrupIsim { get; set; }
}
public sealed class DepoDto { public short DepoKodu { get; set; } public string DepoIsmi { get; set; } = string.Empty; }
public sealed class ProjeDto { public string ProjeKod { get; set; } = string.Empty; public string ProjeAciklama { get; set; } = string.Empty; }
public sealed class StokBarcodeDto { public string? Barkod { get; set; } public string? StokKodu { get; set; } public string? StokAdi { get; set; } public string? DepoKodu { get; set; } public string? DepoAdi { get; set; } public string? RafKodu { get; set; } public string? YapKod { get; set; } public string? YapAcik { get; set; } public bool? SeriBarkodMu { get; set; } public string? IsemriNo { get; set; } }
public sealed class BranchDto { public short SubeKodu { get; set; } public string? Unvan { get; set; } }
public sealed class WarehouseAndShelvesDto { [MaxLength(100)] public string? DepoKodu { get; set; } [MaxLength(100)] public string? HucreKodu { get; set; } }
public sealed class WarehouseShelvesWithStockInformationDto { public string? DepoKodu { get; set; } public string? HucreKodu { get; set; } public string? StokKodu { get; set; } public string? StokAdi { get; set; } public string? YapKod { get; set; } public string? YapAcik { get; set; } public string? SeriNo { get; set; } public decimal? Bakiye { get; set; } }
public sealed class WarehouseShelfStocksDto { public string? DepoKodu { get; set; } public string? HucreKodu { get; set; } public string? StokKodu { get; set; } public string? StokAdi { get; set; } public string? SeriNo { get; set; } public decimal? Bakiye { get; set; } }
