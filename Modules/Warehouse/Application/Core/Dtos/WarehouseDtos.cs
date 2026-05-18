using System.ComponentModel.DataAnnotations;
using Wms.Application.Common;

namespace Wms.Application.Warehouse.Dtos;

public sealed class WarehouseDto : BaseEntityDto
{
    public int WarehouseCode { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
}

public sealed class CreateWarehouseDto
{
    [Required]
    public int WarehouseCode { get; set; }

    [Required, StringLength(200)]
    public string WarehouseName { get; set; } = string.Empty;

    public string BranchCode { get; set; } = "0";
}

public sealed class UpdateWarehouseDto
{
    public int? WarehouseCode { get; set; }
    [StringLength(200)] public string? WarehouseName { get; set; }
    public string? BranchCode { get; set; }
}

public sealed class SyncWarehouseDto
{
    [Required]
    public int WarehouseCode { get; set; }
    [Required, StringLength(200)]
    public string WarehouseName { get; set; } = string.Empty;
    public string BranchCode { get; set; } = "0";
}
