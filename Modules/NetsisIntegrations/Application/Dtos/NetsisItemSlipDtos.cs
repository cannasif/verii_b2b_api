using System.Text.Json.Serialization;

namespace Wms.Modules.NetsisIntegrations.Application.Dtos;

public enum NetsisItemSlipInvoiceType
{
    DomesticClosed = 1,
    DomesticOpen = 2
}

public sealed class NetsisItemSlipCreateRequestDto
{
    public NetsisItemSlipHeaderDto FatUst { get; set; } = new();
    public List<NetsisItemSlipLineDto> Kalems { get; set; } = [];
}

public sealed class NetsisItemSlipHeaderDto
{
    public string? CariKod { get; set; }
    public string? FisNo { get; set; }
    public string? BelgeNo { get; set; }
    public DateTime? Tarih { get; set; }
    public DateTime? FiiliTarih { get; set; }
    public int? Tip { get; set; }
    public NetsisItemSlipInvoiceType? Tipi { get; set; } = NetsisItemSlipInvoiceType.DomesticClosed;
    public int? SubeKodu { get; set; }
    public string? ProjeKodu { get; set; }
    public string? Aciklama { get; set; }
    public string? PlasiyerKodu { get; set; }
    public int? DepoKodu { get; set; }
    public string? Seri { get; set; }
}

public sealed class NetsisItemSlipLineDto
{
    public string? StokKodu { get; set; }
    public decimal Miktar { get; set; }
    public int? DepoKodu { get; set; }
    public int? GirisDepoKodu { get; set; }
    public int? CikisDepoKodu { get; set; }
    public string? ProjeKodu { get; set; }
    public string? YapKod { get; set; }
    public string? SeriNo { get; set; }
    public string? Aciklama { get; set; }
    public string? SiparisNo { get; set; }
    public string? IncKeyNo { get; set; }
}

public sealed class NetsisItemSlipCreateResponseDto
{
    public bool IsSuccessful { get; set; }
    public bool? IsSuccessStatusCode { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorDesc { get; set; }
    public string? ErrorDescription { get; set; }
    public NetsisItemSlipResponseDataDto? Data { get; set; }

    [JsonIgnore]
    public string? RawResponse { get; set; }
}

public sealed class NetsisItemSlipResponseDataDto
{
    public string? FisNo { get; set; }
    public string? BelgeNo { get; set; }
    public string? KayitNo { get; set; }
    public string? ReferenceNumber { get; set; }
}
