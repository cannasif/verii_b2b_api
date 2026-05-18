using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Wms.Application.Common;
using Wms.Modules.NetsisIntegrations.Application.Dtos;
using Wms.Modules.NetsisIntegrations.Infrastructure.Options;

namespace Wms.Modules.NetsisIntegrations.Application.Services;

public sealed class NetsisItemSlipService : INetsisItemSlipService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly INetsisRestClient _restClient;
    private readonly IOptions<NetsisOptions> _options;
    private readonly IRequestTraceAccessor _requestTraceAccessor;
    private readonly IIntegrationLogWriter _integrationLogWriter;
    private readonly ILogger<NetsisItemSlipService> _logger;

    public NetsisItemSlipService(
        INetsisRestClient restClient,
        IOptions<NetsisOptions> options,
        IRequestTraceAccessor requestTraceAccessor,
        IIntegrationLogWriter integrationLogWriter,
        ILogger<NetsisItemSlipService> logger)
    {
        _restClient = restClient;
        _options = options;
        _requestTraceAccessor = requestTraceAccessor;
        _integrationLogWriter = integrationLogWriter;
        _logger = logger;
    }

    public async Task<NetsisItemSlipCreateResponseDto> CreateAsync(
        NetsisItemSlipCreateRequestDto request,
        string operation,
        int? configuredDocumentType,
        CancellationToken cancellationToken = default)
    {
        ValidateAndShapeRequest(request, configuredDocumentType, operation);

        var startedAt = DateTime.UtcNow;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var options = _options.Value;
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, ResolveItemSlipsPath(options))
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };

        try
        {
            using var response = await _restClient.SendAsync(httpRequest, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = ParseResponse(responseBody);
            result.RawResponse = responseBody;

            if (!response.IsSuccessStatusCode || !IsSuccessful(result))
            {
                var failure = ResolveFailureMessage(response, result, responseBody);
                await WriteIntegrationLogAsync(request, operation, "Failed", failure, result, startedAt, stopwatch.ElapsedMilliseconds, cancellationToken);
                throw new InvalidOperationException(failure);
            }

            await WriteIntegrationLogAsync(request, operation, "Succeeded", null, result, startedAt, stopwatch.ElapsedMilliseconds, cancellationToken);
            return result;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(
                ex,
                "Netsis item slip creation failed TraceId={TraceId} Operation={Operation} CustomerCode={CustomerCode}",
                _requestTraceAccessor.TraceId,
                operation,
                request.FatUst.CariKod);

            await WriteIntegrationLogAsync(request, operation, "Failed", ex.Message, null, startedAt, stopwatch.ElapsedMilliseconds, cancellationToken);
            throw;
        }
    }

    private static void ValidateAndShapeRequest(NetsisItemSlipCreateRequestDto request, int? configuredDocumentType, string operation)
    {
        request.FatUst.Tip = configuredDocumentType ?? request.FatUst.Tip
            ?? throw new InvalidOperationException($"Netsis {operation} document type is not configured.");
        request.FatUst.Tipi ??= NetsisItemSlipInvoiceType.DomesticClosed;

        if (string.IsNullOrWhiteSpace(request.FatUst.CariKod))
        {
            throw new InvalidOperationException($"Netsis {operation} requires FatUst.CariKod.");
        }

        if (request.Kalems.Count == 0)
        {
            throw new InvalidOperationException($"Netsis {operation} requires at least one line.");
        }

        if (request.Kalems.Any(line => string.IsNullOrWhiteSpace(line.StokKodu) || line.Miktar <= 0))
        {
            throw new InvalidOperationException($"Netsis {operation} lines require StokKodu and positive Miktar.");
        }
    }

    private static string ResolveItemSlipsPath(NetsisOptions options)
        => string.IsNullOrWhiteSpace(options.Rest.ItemSlipsPath) ? "/api/v2/ItemSlips" : options.Rest.ItemSlipsPath;

    private static NetsisItemSlipCreateResponseDto ParseResponse(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return new NetsisItemSlipCreateResponseDto { IsSuccessful = false };
        }

        try
        {
            var response = JsonSerializer.Deserialize<NetsisItemSlipCreateResponseDto>(responseBody, JsonOptions)
                ?? new NetsisItemSlipCreateResponseDto { IsSuccessful = false };
            HydrateReferenceFieldsFromRawResponse(response, responseBody);
            return response;
        }
        catch
        {
            return new NetsisItemSlipCreateResponseDto
            {
                IsSuccessful = false,
                ErrorDesc = responseBody,
                RawResponse = responseBody
            };
        }
    }

    private static void HydrateReferenceFieldsFromRawResponse(NetsisItemSlipCreateResponseDto response, string responseBody)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            CollectReferenceCandidates(document.RootElement, values);

            response.Data ??= new NetsisItemSlipResponseDataDto();
            response.Data.FisNo ??= FirstValue(values, "FisNo", "FISNO", "Fis_No", "FIS_NO", "FATIRS_NO", "FatirsNo");
            response.Data.BelgeNo ??= FirstValue(values, "BelgeNo", "BELGE_NO", "Belge_No", "BelgeNumarasi", "BelgeNumarası");
            response.Data.KayitNo ??= FirstValue(values, "KayitNo", "KAYIT_NO", "Kayit_No", "KayıtNo", "KayitNumarasi");
            response.Data.ReferenceNumber ??= FirstValue(values, "ReferenceNumber", "REFERENCE_NUMBER", "ReferansNo", "ReferansKodu", "RefNo");
        }
        catch
        {
            // Netsis can return non-standard payloads; the raw response is kept in integration metadata.
        }
    }

    private static void CollectReferenceCandidates(JsonElement element, IDictionary<string, string?> values)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Value.ValueKind is JsonValueKind.String or JsonValueKind.Number)
                {
                    values.TryAdd(property.Name, property.Value.ToString());
                }

                CollectReferenceCandidates(property.Value, values);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                CollectReferenceCandidates(item, values);
            }
        }
    }

    private static string? FirstValue(IReadOnlyDictionary<string, string?> values, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static bool IsSuccessful(NetsisItemSlipCreateResponseDto result)
    {
        return (result.IsSuccessful || result.IsSuccessStatusCode == true)
            && string.IsNullOrWhiteSpace(result.ErrorDesc)
            && string.IsNullOrWhiteSpace(result.ErrorDescription);
    }

    private static string ResolveFailureMessage(HttpResponseMessage response, NetsisItemSlipCreateResponseDto result, string responseBody)
    {
        return !string.IsNullOrWhiteSpace(result.ErrorDesc)
            ? $"Netsis ItemSlips request failed ({(int)response.StatusCode}). {result.ErrorDesc}"
            : !string.IsNullOrWhiteSpace(result.ErrorDescription)
                ? $"Netsis ItemSlips request failed ({(int)response.StatusCode}). {result.ErrorDescription}"
                : !string.IsNullOrWhiteSpace(result.ErrorCode)
                    ? $"Netsis ItemSlips request failed ({(int)response.StatusCode}). ErrorCode: {result.ErrorCode}"
                    : $"Netsis ItemSlips request failed ({(int)response.StatusCode}). Body: {Truncate(responseBody, 1000)}";
    }

    private async Task WriteIntegrationLogAsync(
        NetsisItemSlipCreateRequestDto request,
        string operation,
        string status,
        string? error,
        NetsisItemSlipCreateResponseDto? response,
        DateTime startedAt,
        long elapsedMilliseconds,
        CancellationToken cancellationToken)
    {
        await _integrationLogWriter.TryWriteAsync(new IntegrationLogWriteRequest
        {
            TraceId = _requestTraceAccessor.TraceId,
            IntegrationType = "ERP",
            TargetSystem = "Netsis",
            Operation = operation,
            Status = status,
            Source = "NetsisItemSlipService.CreateAsync",
            RequestMetadata = JsonSerializer.Serialize(new
            {
                request.FatUst.CariKod,
                request.FatUst.Tip,
                request.FatUst.Tipi,
                request.FatUst.SubeKodu,
                request.FatUst.DepoKodu,
                lineCount = request.Kalems.Count,
                stockCodes = request.Kalems.Select(x => x.StokKodu).Where(x => !string.IsNullOrWhiteSpace(x)).Take(50).ToArray(),
            }, JsonOptions),
            ResponseMetadata = response == null ? null : JsonSerializer.Serialize(new
            {
                response.Data?.FisNo,
                response.Data?.BelgeNo,
                response.Data?.KayitNo,
                response.Data?.ReferenceNumber,
                rawResponse = Truncate(response.RawResponse, 4000),
            }, JsonOptions),
            ErrorMessage = error,
            ErrorType = error == null ? null : "NetsisItemSlipError",
            StartedAt = startedAt,
            FinishedAt = DateTime.UtcNow,
            DurationMs = (int)Math.Min(elapsedMilliseconds, int.MaxValue),
        }, cancellationToken);
    }

    private static string? Truncate(string? value, int maxLength)
        => string.IsNullOrWhiteSpace(value) || value.Length <= maxLength ? value : value[..maxLength];
}
