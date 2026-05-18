using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Wms.Application.Common;
using Wms.Modules.NetsisIntegrations.Application.Services;

namespace Wms.Modules.NetsisIntegrations.Infrastructure.Persistence;

public sealed partial class NetsisQueryExecutor : INetsisQueryExecutor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IConfiguration _configuration;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IRequestTraceAccessor _requestTraceAccessor;
    private readonly IIntegrationLogWriter _integrationLogWriter;
    private readonly ILogger<NetsisQueryExecutor> _logger;

    public NetsisQueryExecutor(
        IConfiguration configuration,
        ICurrentUserAccessor currentUserAccessor,
        IRequestTraceAccessor requestTraceAccessor,
        IIntegrationLogWriter integrationLogWriter,
        ILogger<NetsisQueryExecutor> logger)
    {
        _configuration = configuration;
        _currentUserAccessor = currentUserAccessor;
        _requestTraceAccessor = requestTraceAccessor;
        _integrationLogWriter = integrationLogWriter;
        _logger = logger;
    }

    public async Task<List<T>> QueryAsync<T>(
        NetsisQueryDefinition definition,
        Func<SqlDataReader, T> map,
        CancellationToken cancellationToken = default)
    {
        var connectionString = _configuration.GetConnectionString("ErpConnection")
            ?? _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ErpConnection is not configured.");
        }

        var operation = NormalizeOperation(definition.Operation, definition.Sql);
        var traceId = _requestTraceAccessor.TraceId;
        var branchCode = _currentUserAccessor.BranchCode;
        var startedAt = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        var result = new List<T>();

        _logger.LogInformation(
            "Netsis read started TraceId={TraceId} BranchCode={BranchCode} Operation={Operation} Source={Source} ParameterCount={ParameterCount}",
            traceId,
            branchCode,
            operation,
            definition.Source,
            definition.Parameters.Count);

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = definition.Sql;
            command.Parameters.AddRange(definition.Parameters.ToArray());

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(map(reader));
            }

            stopwatch.Stop();

            _logger.LogInformation(
                "Netsis read completed TraceId={TraceId} BranchCode={BranchCode} Operation={Operation} Source={Source} RowCount={RowCount}",
                traceId,
                branchCode,
                operation,
                definition.Source,
                result.Count);

            await WriteIntegrationLogAsync(
                definition,
                operation,
                "Succeeded",
                startedAt,
                stopwatch.ElapsedMilliseconds,
                result.Count,
                null,
                cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            var safeException = BuildSafeException(ex, operation);

            await WriteIntegrationLogAsync(
                definition,
                operation,
                "Failed",
                startedAt,
                stopwatch.ElapsedMilliseconds,
                null,
                safeException,
                cancellationToken);

            throw safeException;
        }
    }

    private async Task WriteIntegrationLogAsync(
        NetsisQueryDefinition definition,
        string operation,
        string status,
        DateTime startedAt,
        long elapsedMilliseconds,
        int? rowCount,
        Exception? exception,
        CancellationToken cancellationToken)
    {
        await _integrationLogWriter.TryWriteAsync(new IntegrationLogWriteRequest
        {
            TraceId = _requestTraceAccessor.TraceId,
            IntegrationType = "ERP",
            TargetSystem = "Netsis",
            Operation = operation,
            Status = status,
            Source = definition.Source,
            RequestMetadata = BuildRequestMetadata(definition),
            ResponseMetadata = rowCount.HasValue ? JsonSerializer.Serialize(new { rowCount }, JsonOptions) : null,
            ErrorMessage = exception?.Message,
            ErrorType = exception?.GetType().FullName,
            StartedAt = startedAt,
            FinishedAt = DateTime.UtcNow,
            DurationMs = (int)Math.Min(elapsedMilliseconds, int.MaxValue),
        }, cancellationToken);
    }

    private string BuildRequestMetadata(NetsisQueryDefinition definition)
    {
        return JsonSerializer.Serialize(new
        {
            branchCode = _currentUserAccessor.BranchCode,
            parameterCount = definition.Parameters.Count,
            parameters = definition.Parameters.Select(parameter => parameter.ParameterName).ToArray(),
        }, JsonOptions);
    }

    private static Exception BuildSafeException(Exception exception, string operation)
    {
        if (IsMissingSqlObject(exception, operation))
        {
            return new InvalidOperationException($"{operation} fonksiyonu ERP veritabanında bulunamadı.", exception);
        }

        return exception;
    }

    private static bool IsMissingSqlObject(Exception exception, string operation)
    {
        if (exception is SqlException sqlException)
        {
            return sqlException.Number == 208
                || IsInvalidObjectName(sqlException.Message, operation);
        }

        return IsInvalidObjectName(exception.Message, operation);
    }

    private static bool IsInvalidObjectName(string message, string operation)
    {
        return message.Contains(operation, StringComparison.OrdinalIgnoreCase)
            && message.Contains("Invalid object name", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeOperation(string operation, string sql)
    {
        if (!string.IsNullOrWhiteSpace(operation))
        {
            return operation.Trim();
        }

        var match = NetsisFunctionNameRegex().Match(sql);
        if (match.Success)
        {
            return match.Groups["name"].Value;
        }

        return sql.Replace("\n", " ", StringComparison.Ordinal)
            .Replace("\r", " ", StringComparison.Ordinal)
            .Trim();
    }

    [GeneratedRegex(@"dbo\.(?<name>RII_[A-Z0-9_]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex NetsisFunctionNameRegex();
}
