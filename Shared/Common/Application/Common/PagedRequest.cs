namespace Wms.Application.Common;

/// <summary>
/// `_old/reference/verii_wms_api.Application/DTOs/Common/PagedRequest.cs` paging/filter/search kontratını korur.
/// </summary>
public sealed class Filter
{
    public string Column { get; set; } = string.Empty;
    public string Operator { get; set; } = "Equals";
    public string Value { get; set; } = string.Empty;
}

public sealed class PagedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "Id";
    public string? SortDirection { get; set; } = "desc";
    public string? Search { get; set; }
    public List<Filter> Filters { get; set; } = new();
    public string FilterLogic { get; set; } = "and";
}
