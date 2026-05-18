namespace Wms.Application.Common;

public sealed class AuditSnapshotDelta
{
    public IReadOnlyDictionary<string, object?>? OldValues { get; init; }
    public IReadOnlyDictionary<string, object?>? NewValues { get; init; }
    public IReadOnlyList<string> ChangedFields { get; init; } = Array.Empty<string>();
}
