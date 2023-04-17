using Monq.Core.VictoriaMetricsClient.Extensions;
using Monq.Core.VictoriaMetricsClient.Models;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Monq.Core.VictoriaMetricsClient;

public class VictoriaClientRead : IVictoriaClientRead
{
    static readonly JsonSerializerOptions _defaultJsonOptions =
        new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
    public static JsonSerializerOptions DefaultJsonOptions => _defaultJsonOptions;

    readonly HttpClient _httpClient;
    readonly VictoriaOptions _victoriaOptions;

    static VictoriaClientRead()
    {
        _defaultJsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public VictoriaClientRead(HttpClient httpClient, IOptions<VictoriaOptions> victoriaOptions)
    {
        _httpClient = httpClient;
        _victoriaOptions =
            victoriaOptions?.Value 
            ?? throw new StorageConfigurationException("There is no configuration found for the VictoriaMetrics.");
    }

    /// <inheritdoc />
    public async ValueTask<BaseQueryDataResponse> Query(string query,
        string step,
        IEnumerable<long> streamIds,
        long userspaceId)
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
        return await GetQueryData("query", contentParams);
    }

    /// <inheritdoc />
    public async ValueTask<BaseQueryDataResponse> QueryRange(string query,
        DateTimeOffset start,
        DateTimeOffset end,
        TimeInterval step,
        IEnumerable<long> streamIds,
        long userspaceId)
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
        return await GetQueryData("query_range", contentParams);
    }

    async Task<BaseQueryDataResponse> GetQueryData(string requestUri, Dictionary<string, string> contentParams)
    {
        var content = new FormUrlEncodedContent(contentParams);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(requestUri, content);
        }
        catch (Exception e)
        {
            throw new StorageException($"Storage throws exeption on request. Details: {e.Message}", e);
        }

        if (!response.IsSuccessStatusCode)
            throw new StorageException($"Storage. Victoria responded with status code: {response.StatusCode}. " +
                "Can't read message due to exception. " +
                $"Details: {await response.Content.ReadAsStringAsync()}");

        var responseMessage = await response.Content.ReadFromJsonAsync<BaseResponseModel>(_defaultJsonOptions);
        if (responseMessage is null)
            throw new StorageException("Storage responded with empty message.");

        if (responseMessage.Status == PrometheusResponseStatuses.error)
            throw new StorageException($"Storage responded with status Error. Details: {responseMessage.Error}");
        var result = responseMessage.Data.Deserialize<BaseQueryDataResponse>(_defaultJsonOptions);
        if (result is null)
            throw new StorageException("""Storage responded with empty "data" message.""");
        return result;
    }
}
