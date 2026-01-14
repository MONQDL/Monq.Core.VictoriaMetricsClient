using Microsoft.Extensions.Options;
using Monq.Core.VictoriaMetricsClient.Exceptions;
using Monq.Core.VictoriaMetricsClient.Extensions;
using Monq.Core.VictoriaMetricsClient.Models;
using Monq.Core.VictoriaMetricsClient.SerializerContexts;
using System.Net.Http.Json;
using System.Text.Json;

namespace Monq.Core.VictoriaMetricsClient;

public sealed class VictoriaClientRead : IVictoriaClientRead
{
    readonly HttpClient _httpClient;
    readonly VictoriaOptions _victoriaOptions;

    public VictoriaClientRead(
        HttpClient httpClient,
        IOptions<VictoriaOptions> victoriaOptions)
    {
        _httpClient = httpClient;
        _victoriaOptions =
            victoriaOptions?.Value
            ?? throw new StorageConfigurationException("There is no configuration found for the VictoriaMetrics.");
    }

    /// <inheritdoc />
    public async ValueTask<BaseQueryDataResponse> Query(
        string query,
        string step,
        IEnumerable<long> streamIds,
        long userspaceId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(query))
            throw new StorageException($"The {nameof(query)} is null or empty.");

        if (!streamIds.Any())
            throw new StorageException("There is no streamIds set.");
        if (userspaceId <= 0)
            throw new StorageException("The userspaceId parameter is not set.");

        var contentParams = new Dictionary<string, string>
        {
            { "query", query },
            { "extra_filters[]", $"{{{_victoriaOptions.GetStreamIdLabelName()}=~\"{string.Join("|", streamIds)}\"}}" },
            { "extra_label", $"{_victoriaOptions.GetUserspaceIdLabelName()}={userspaceId}" },
            { "step", step }
        };
        return await GetQueryData("query", contentParams, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask<BaseQueryDataResponse> QueryRange(
        string query,
        DateTimeOffset start,
        DateTimeOffset end,
        TimeInterval step,
        IEnumerable<long> streamIds,
        long userspaceId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(query))
            throw new StorageException($"The {nameof(query)} is null or empty.");

        if (!streamIds.Any())
            throw new StorageException("There is no streamIds set.");
        if (userspaceId <= 0)
            throw new StorageException("The userspaceId parameter is not set.");

        var contentParams = new Dictionary<string, string>
        {
            { "query", query },
            { "extra_filters[]", $"{{{_victoriaOptions.GetStreamIdLabelName()}=~\"{string.Join("|", streamIds)}\"}}" },
            { "extra_label", $"{_victoriaOptions.GetUserspaceIdLabelName()}={userspaceId}" },
            { "start", $"{start.ToUnixTimeSeconds()}" },
            { "end", $"{end.ToUnixTimeSeconds()}" },
            { "step", step.ToPromQlInterval() }
        };
        return await GetQueryData("query_range", contentParams, cancellationToken);
    }

    async Task<BaseQueryDataResponse> GetQueryData(
        string requestUri,
        Dictionary<string, string> contentParams,
        CancellationToken cancellationToken)
    {
        var content = new FormUrlEncodedContent(contentParams);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(requestUri, content, cancellationToken);
        }
        catch (Exception e)
        {
            throw new StorageException($"Storage throws exception on request. Details: {e.Message}", e);
        }

        if (!response.IsSuccessStatusCode)
            throw new StorageException($"Storage. Victoria responded with status code: {(int)response.StatusCode}. " +
                "Can't read message due to exception. " +
                $"Details: {await response.Content.ReadAsStringAsync(cancellationToken)}");

        var responseMessage = await response.Content
            .ReadFromJsonAsync(BaseResponseModelSerializerContext.Default.BaseResponseModel, cancellationToken)
            ?? throw new StorageException("Storage responded with empty message.");

        if (responseMessage.Status == PrometheusResponseStatuses.error)
            throw new StorageException($"Storage responded with status Error. Details: {responseMessage.Error}");
        BaseQueryDataResponse? result;
        try
        {
            result = responseMessage.Data?
                .Deserialize(BaseQueryDataResponseSerializerContext.Default.BaseQueryDataResponse);
        }
        catch (JsonException e)
        {
            throw new StorageException($"""Storage "data" field can't be deserialized. Message: '{e.Message}'""", e);
        }

        if (result is null)
            throw new StorageException("""Storage responded with empty "data" message.""");
        return result;
    }
}
