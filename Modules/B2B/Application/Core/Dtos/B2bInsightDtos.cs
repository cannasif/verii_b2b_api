using Wms.Application.Common;

namespace Wms.Application.B2B.Dtos;

public sealed class B2bInsightSummaryDto
{
    public DateTime GeneratedAt { get; set; }
    public B2bInsightScoreDto Readiness { get; set; } = new();
    public List<B2bInsightMetricDto> Metrics { get; set; } = new();
    public List<B2bInsightActionDto> Actions { get; set; } = new();
}

public sealed class B2bInsightScoreDto
{
    public int Score { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public sealed class B2bInsightMetricDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal? SecondaryValue { get; set; }
    public string? Unit { get; set; }
    public string Status { get; set; } = "Neutral";
    public string? Description { get; set; }
}

public sealed class B2bInsightActionDto
{
    public string Severity { get; set; } = "Info";
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? TargetRoute { get; set; }
}
