namespace Wms.Application.Common;

public interface ITraceableApiResponse
{
    string? TraceId { get; set; }
}

public class ApiResponse<T> : ITraceableApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ExceptionMessage { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public object? Details { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int StatusCode { get; set; } = 200;
    public string ClassName { get; set; } = string.Empty;
    public string? TraceId { get; set; }

    private static string GetGenericTypeDisplayName(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetGenericTypeDisplayName));
        var name = type.Name[..type.Name.IndexOf('`')];
        return $"{name}<{genericArgs}>";
    }

    public override string ToString()
    {
        return $"{ClassName} [Success={Success}, StatusCode={StatusCode}, Message={Message}]";
    }

    public static ApiResponse<T> SuccessResult(T data, string message)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = 200,
            ClassName = $"ApiResponse<{GetGenericTypeDisplayName(typeof(T))}>"
        };
    }

    public static ApiResponse<T> ErrorResult(
        string message,
        string? exceptionMessage = null,
        int statusCode = 500,
        string? error = null,
        string? errorCode = null,
        object? details = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            ExceptionMessage = exceptionMessage ?? string.Empty,
            ErrorCode = errorCode,
            Details = details,
            Errors = error is null ? new List<string>() : new List<string> { error },
            StatusCode = statusCode,
            ClassName = $"ApiResponse<{GetGenericTypeDisplayName(typeof(T))}>"
        };
    }
}

public sealed class PagedResponse<T>
{
    public List<T> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }

    public PagedResponse(List<T> data, int totalCount, int pageNumber, int pageSize)
    {
        Data = data;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        HasPreviousPage = pageNumber > 1;
        HasNextPage = pageNumber < TotalPages;
    }
}
