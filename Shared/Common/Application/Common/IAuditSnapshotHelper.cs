namespace Wms.Application.Common;

public interface IAuditSnapshotHelper
{
    IReadOnlyDictionary<string, object?>? CreateSnapshot(object? source);
    IReadOnlyList<string> GetChangedFields(object? oldValues, object? newValues);
    AuditSnapshotDelta BuildDelta(object? oldValues, object? newValues);
}
