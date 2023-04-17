using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Monq.Core.VictoriaMetricsClient.Models;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Monq.Core.VictoriaMetricsClient;

public class VictoriaProxyClient : IVictoriaProxyClient
{
    static readonly JsonSerializerOptions _defaultJsonOptions =
        new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
    public static JsonSerializerOptions DefaultJsonOptions => _defaultJsonOptions;

    readonly HttpClient _httpClient;
    readonly ILogger<VictoriaProxyClient> _log;
    readonly VictoriaOptions _victoriaOptions;

    static VictoriaProxyClient()
    {
        _defaultJsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public VictoriaProxyClient(
        HttpClient httpClient,
        ILogger<VictoriaProxyClient> log,
        IOptions<VictoriaOptions> victoriaOptions)
    {
        _log = log;
        _httpClient = httpClient;
        _victoriaOptions =
            victoriaOptions?.Value 
            ?? throw new StorageConfigurationException("There is not configuration found for the VictoriaMetrics.");
    }

    /// <inheritdoc />
    public ValueTask<BaseResponseModel> Labels(IQueryCollection requestQuery) => 
        AllGrantedRequest("labels", requestQuery);

    /// <inheritdoc />
    public ValueTask<BaseResponseModel> LabelValues(string label,
        IQueryCollection requestQuery,
        long userspaceId,
        IEnumerable<long> streamIds)
    {
        if (label == "__name__")
            return AllGrantedRequest($"label/{label}/values", requestQuery);
        else
            return DefaultRequest($"label/{label}/values", requestQuery, userspaceId, streamIds);
    }

    /// <inheritdoc />
    public ValueTask<BaseResponseModel> Series(IQueryCollection requestQuery,
        long userspaceId,
        IEnumerable<long> streamIds) => DefaultRequest("series", requestQuery, userspaceId, streamIds);

    /// <inheritdoc />
    public ValueTask<BaseResponseModel> Query(IQueryCollection requestQuery,
        long userspaceId,
        IEnumerable<long> streamIds) => DefaultRequest("query", requestQuery, userspaceId, streamIds);

    /// <inheritdoc />
    public ValueTask<BaseResponseModel> QueryRange(IQueryCollection requestQuery,
        long userspaceId,
        IEnumerable<long> streamIds) => DefaultRequest("query_range", requestQuery, userspaceId, streamIds);

    /// <inheritdoc />
    public ValueTask<BaseResponseModel> BuildInfo(IQueryCollection requestQuery) =>
        AllGrantedRequest("status/buildinfo", requestQuery);

    /// <inheritdoc />
    public ValueTask<BaseResponseModel> QueryExemplars(IQueryCollection requestQuery,
        long userspaceId,
        IEnumerable<long> streamIds) =>
            DefaultRequest("query_exemplars", requestQuery, userspaceId, streamIds);

    /// <inheritdoc />
    public ValueTask<BaseResponseModel> Metadata(IQueryCollection requestQuery) => 
        AllGrantedRequest("metadata", requestQuery);

    /// <inheritdoc />
    public ValueTask<BaseResponseModel> Rules(IQueryCollection requestQuery) =>
        AllGrantedRequest("rules", requestQuery);

    /// <inheritdoc />
    async ValueTask<BaseResponseModel> PostRequest(string requestUri, Dictionary<string, string> contentParams)
    {
        var content = new FormUrlEncodedContent(contentParams);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(requestUri, content);
        }
        catch (Exception e)
        {
            var message = $"Storage throws exeption on request. Details: {e.Message}";
            _log.LogError(message, e);
            return new BaseResponseModel
            {
                Error = message,
                Status = PrometheusResponseStatuses.error
            };
        }

        var responseMessage = await response.Content.ReadFromJsonAsync<BaseResponseModel>(_defaultJsonOptions);
        if (responseMessage is null)
            return new BaseResponseModel
            {
                Error = "Storage responded with empty message.",
                Status = PrometheusResponseStatuses.error
            };

        return responseMessage;
    }

    async ValueTask<BaseResponseModel> DefaultRequest(string requestUri, IQueryCollection requestQuery,
        long userspaceId,
        IEnumerable<long> streamIds)
    {
        if (!streamIds.Any())
            throw new StorageException("There is no streamIds set.");
        if (userspaceId <= 0)
            throw new StorageException("The userspaceId parameter is not set.");

        var contentParams = new Dictionary<string, string>()
        {
            { "extra_filters[]", $"{{{_victoriaOptions.GetStreamIdLabelName()}=~\"{string.Join("|", streamIds)}\"}}" },
            { "extra_label", $"{_victoriaOptions.GetUserspaceIdLabelName()}={userspaceId}" },
        };

        foreach (var queryParam in requestQuery)
        {
            // Защита от пользовательского внедрения своих параметров.
            if (queryParam.Key is "extra_label" or "extra_filters[]")
                continue;
            contentParams.Add(queryParam.Key, queryParam.Value.ToString());
        }

        return await PostRequest(requestUri, contentParams);
    }

    async ValueTask<BaseResponseModel> AllGrantedRequest(string requestUri, IQueryCollection requestQuery)
    {
        var contentParams = new Dictionary<string, string>();

        foreach (var queryParam in requestQuery)
            contentParams.Add(queryParam.Key, queryParam.Value.ToString());

        return await PostRequest(requestUri, contentParams);
    }
}
