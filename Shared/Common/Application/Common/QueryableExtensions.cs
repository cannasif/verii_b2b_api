using System.Linq.Expressions;

namespace Wms.Application.Common;

/// <summary>
/// `_old` HelperExtensions içindeki filter/sort/pagination davranışının pragmatik çekirdeği.
/// Definitions batch'inde ihtiyaç duyulan kadar taşınır.
/// </summary>
public static class QueryableExtensions
{
    public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, string? search, params string[] searchableColumns)
    {
        if (string.IsNullOrWhiteSpace(search) || searchableColumns.Length == 0)
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? predicate = null;
        var searchConstant = Expression.Constant(search.Trim());

        foreach (var column in searchableColumns.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var resolved = ResolvePropertyPath(parameter, typeof(T), column);
            if (resolved == null || resolved.Value.type != typeof(string))
            {
                continue;
            }

            var left = resolved.Value.expression;
            var notNull = Expression.NotEqual(left, Expression.Constant(null, typeof(string)));
            var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;
            var contains = Expression.Call(left, containsMethod, searchConstant);
            var current = Expression.AndAlso(notNull, contains);

            predicate = predicate == null ? current : Expression.OrElse(predicate, current);
        }

        if (predicate == null)
        {
            return query;
        }

        return query.Where(Expression.Lambda<Func<T, bool>>(predicate, parameter));
    }

    private static (Expression expression, Type type)? ResolvePropertyPath(Expression parameter, Type rootType, string path)
    {
        var parts = path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Expression current = parameter;
        var currentType = rootType;

        foreach (var part in parts)
        {
            var property = currentType.GetProperty(part);
            if (property == null)
            {
                return null;
            }

            current = Expression.Property(current, property);
            currentType = property.PropertyType;
        }

        return (current, currentType);
    }

    public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> query, List<Filter>? filters, string filterLogic = "and")
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? basePredicate = null;

        var isDeletedProperty = typeof(T).GetProperty("IsDeleted");
        if (isDeletedProperty != null && (isDeletedProperty.PropertyType == typeof(bool) || isDeletedProperty.PropertyType == typeof(bool?)))
        {
            var isDeletedLeft = Expression.Property(parameter, isDeletedProperty);
            basePredicate = Expression.Equal(isDeletedLeft, Expression.Constant(false));
        }

        if (filters == null || filters.Count == 0)
        {
            if (basePredicate == null)
            {
                return query;
            }

            return query.Where(Expression.Lambda<Func<T, bool>>(basePredicate, parameter));
        }

        var useOr = string.Equals(filterLogic, "or", StringComparison.OrdinalIgnoreCase);
        Expression? filterPredicate = null;

        foreach (var filter in filters)
        {
            if (string.IsNullOrWhiteSpace(filter.Column) || string.IsNullOrWhiteSpace(filter.Value))
            {
                continue;
            }

            var resolved = ResolvePropertyPath(parameter, typeof(T), filter.Column);
            if (resolved == null)
            {
                continue;
            }

            var (left, type) = resolved.Value;
            var operatorLower = filter.Operator.ToLowerInvariant();
            Expression? currentPredicate = null;

            if (type == typeof(string))
            {
                var stringValue = Expression.Constant(filter.Value);
                var notNull = Expression.NotEqual(left, Expression.Constant(null, typeof(string)));
                var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;
                var startsWithMethod = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) })!;
                var endsWithMethod = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) })!;

                currentPredicate = operatorLower switch
                {
                    "contains" => Expression.AndAlso(notNull, Expression.Call(left, containsMethod, stringValue)),
                    "startswith" => Expression.AndAlso(notNull, Expression.Call(left, startsWithMethod, stringValue)),
                    "endswith" => Expression.AndAlso(notNull, Expression.Call(left, endsWithMethod, stringValue)),
                    _ => Expression.Equal(left, stringValue)
                };
            }
            else if (type == typeof(bool) || type == typeof(bool?))
            {
                if (bool.TryParse(filter.Value, out var boolValue))
                {
                    currentPredicate = Expression.Equal(left, Expression.Constant(boolValue, type));
                }
            }
            else if (type == typeof(int) || type == typeof(int?))
            {
                if (int.TryParse(filter.Value, out var intValue))
                {
                    currentPredicate = BuildComparablePredicate(left, Expression.Constant(intValue, type), operatorLower);
                }
            }
            else if (type == typeof(long) || type == typeof(long?))
            {
                if (long.TryParse(filter.Value, out var longValue))
                {
                    currentPredicate = BuildComparablePredicate(left, Expression.Constant(longValue, type), operatorLower);
                }
            }
            else if (type == typeof(decimal) || type == typeof(decimal?))
            {
                if (decimal.TryParse(filter.Value, out var decimalValue))
                {
                    currentPredicate = BuildComparablePredicate(left, Expression.Constant(decimalValue, type), operatorLower);
                }
            }
            else if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                if (DateTime.TryParse(filter.Value, out var dateValue))
                {
                    currentPredicate = BuildComparablePredicate(left, Expression.Constant(dateValue, type), operatorLower);
                }
            }

            if (currentPredicate == null)
            {
                continue;
            }

            filterPredicate = filterPredicate == null
                ? currentPredicate
                : useOr
                    ? Expression.OrElse(filterPredicate, currentPredicate)
                    : Expression.AndAlso(filterPredicate, currentPredicate);
        }

        var finalPredicate = basePredicate != null && filterPredicate != null
            ? Expression.AndAlso(basePredicate, filterPredicate)
            : basePredicate ?? filterPredicate;

        if (finalPredicate == null)
        {
            return query;
        }

        return query.Where(Expression.Lambda<Func<T, bool>>(finalPredicate, parameter));
    }

    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string sortBy, bool desc)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            sortBy = "Id";
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var resolved = ResolvePropertyPath(parameter, typeof(T), sortBy) ?? ResolvePropertyPath(parameter, typeof(T), "Id");
        if (resolved == null)
        {
            return query;
        }

        var keySelector = Expression.Lambda(
            typeof(Func<,>).MakeGenericType(typeof(T), resolved.Value.type),
            resolved.Value.expression,
            parameter);

        var methodName = desc ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);
        var call = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(T), resolved.Value.type },
            query.Expression,
            keySelector);

        return query.Provider.CreateQuery<T>(call);
    }

    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
        {
            pageNumber = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 20;
        }

        return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }

    private static Expression BuildComparablePredicate(Expression left, Expression right, string operatorLower)
    {
        return operatorLower switch
        {
            ">" or "gt" => Expression.GreaterThan(left, right),
            ">=" or "gte" => Expression.GreaterThanOrEqual(left, right),
            "<" or "lt" => Expression.LessThan(left, right),
            "<=" or "lte" => Expression.LessThanOrEqual(left, right),
            _ => Expression.Equal(left, right)
        };
    }
}
