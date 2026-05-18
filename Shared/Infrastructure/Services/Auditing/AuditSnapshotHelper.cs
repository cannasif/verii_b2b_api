using System.Collections;
using System.Reflection;
using Wms.Application.Common;

namespace Wms.Infrastructure.Services.Auditing;

public sealed class AuditSnapshotHelper : IAuditSnapshotHelper
{
    private const int MaxStringLength = 1000;
    private const int MaxCollectionItems = 50;

    public IReadOnlyDictionary<string, object?>? CreateSnapshot(object? source)
    {
        return source == null ? null : NormalizeObject(source);
    }

    public IReadOnlyList<string> GetChangedFields(object? oldValues, object? newValues)
    {
        var oldSnapshot = CreateSnapshot(oldValues) ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var newSnapshot = CreateSnapshot(newValues) ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        var keys = oldSnapshot.Keys
            .Concat(newSnapshot.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static x => x, StringComparer.OrdinalIgnoreCase);

        var changedFields = new List<string>();
        foreach (var key in keys)
        {
            oldSnapshot.TryGetValue(key, out var oldValue);
            newSnapshot.TryGetValue(key, out var newValue);

            if (!ValuesEqual(oldValue, newValue))
            {
                changedFields.Add(key);
            }
        }

        return changedFields;
    }

    public AuditSnapshotDelta BuildDelta(object? oldValues, object? newValues)
    {
        var oldSnapshot = CreateSnapshot(oldValues);
        var newSnapshot = CreateSnapshot(newValues);

        return new AuditSnapshotDelta
        {
            OldValues = oldSnapshot,
            NewValues = newSnapshot,
            ChangedFields = GetChangedFields(oldSnapshot, newSnapshot)
        };
    }

    private static IReadOnlyDictionary<string, object?> NormalizeObject(object source)
    {
        if (source is IReadOnlyDictionary<string, object?> readOnlyDictionary)
        {
            return NormalizeDictionary(readOnlyDictionary);
        }

        if (source is IDictionary<string, object?> dictionary)
        {
            return NormalizeDictionary(dictionary);
        }

        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!property.CanRead || property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            result[property.Name] = NormalizeValue(property.GetValue(source));
        }

        return result;
    }

    private static IReadOnlyDictionary<string, object?> NormalizeDictionary(IEnumerable<KeyValuePair<string, object?>> dictionary)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in dictionary)
        {
            result[entry.Key] = NormalizeValue(entry.Value);
        }

        return result;
    }

    private static object? NormalizeValue(object? value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is string stringValue)
        {
            var trimmed = stringValue.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return null;
            }

            return trimmed.Length <= MaxStringLength ? trimmed : trimmed[..MaxStringLength];
        }

        if (value is DateTime dateTime)
        {
            return dateTime.Kind == DateTimeKind.Unspecified ? dateTime : dateTime.ToUniversalTime();
        }

        if (value is DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.ToUniversalTime();
        }

        if (value is Enum)
        {
            return value.ToString();
        }

        if (value is IEnumerable enumerable and not byte[] and not string)
        {
            return NormalizeEnumerable(enumerable);
        }

        var type = value.GetType();
        if (type.IsPrimitive || type.IsValueType)
        {
            return value;
        }

        return NormalizeObject(value);
    }

    private static IReadOnlyList<object?> NormalizeEnumerable(IEnumerable enumerable)
    {
        var items = new List<object?>();
        foreach (var item in enumerable)
        {
            if (items.Count >= MaxCollectionItems)
            {
                items.Add($"__truncated__:{MaxCollectionItems}+");
                break;
            }

            items.Add(NormalizeValue(item));
        }

        return items;
    }

    private static bool ValuesEqual(object? left, object? right)
    {
        if (left == null && right == null)
        {
            return true;
        }

        if (left == null || right == null)
        {
            return false;
        }

        if (left is IReadOnlyDictionary<string, object?> leftDictionary
            && right is IReadOnlyDictionary<string, object?> rightDictionary)
        {
            return DictionariesEqual(leftDictionary, rightDictionary);
        }

        if (left is IReadOnlyList<object?> leftList
            && right is IReadOnlyList<object?> rightList)
        {
            if (leftList.Count != rightList.Count)
            {
                return false;
            }

            for (var i = 0; i < leftList.Count; i++)
            {
                if (!ValuesEqual(leftList[i], rightList[i]))
                {
                    return false;
                }
            }

            return true;
        }

        return object.Equals(left, right);
    }

    private static bool DictionariesEqual(
        IReadOnlyDictionary<string, object?> left,
        IReadOnlyDictionary<string, object?> right)
    {
        if (left.Count != right.Count)
        {
            return false;
        }

        foreach (var entry in left)
        {
            if (!right.TryGetValue(entry.Key, out var rightValue))
            {
                return false;
            }

            if (!ValuesEqual(entry.Value, rightValue))
            {
                return false;
            }
        }

        return true;
    }

}
