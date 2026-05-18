using System.ComponentModel.DataAnnotations;
using Wms.Application.Common;

namespace Wms.Application.YapKod.Dtos;

public sealed class YapKodDto : BaseEntityDto
{
    public string YapKod { get; set; } = string.Empty;
    public string YapAcik { get; set; } = string.Empty;
    public string? YplndrStokKod { get; set; }
    public long? StockId { get; set; }
    public DateTime? LastSyncDate { get; set; }
}

public sealed class CreateYapKodDto
{
    [Required, StringLength(15)]
    public string YapKod { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string YapAcik { get; set; } = string.Empty;

    [StringLength(35)]
    public string? YplndrStokKod { get; set; }

    public long? StockId { get; set; }

    public string BranchCode { get; set; } = "0";
}

public sealed class UpdateYapKodDto
{
    [StringLength(15)]
    public string? YapKod { get; set; }

    [StringLength(255)]
    public string? YapAcik { get; set; }

    [StringLength(35)]
    public string? YplndrStokKod { get; set; }

    public long? StockId { get; set; }

    public string? BranchCode { get; set; }
}

public sealed class SyncYapKodDto
{
    [Required, StringLength(15)]
    public string YapKod { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string YapAcik { get; set; } = string.Empty;

    [StringLength(35)]
    public string? YplndrStokKod { get; set; }

    public long? StockId { get; set; }

    public string BranchCode { get; set; } = "0";
}
