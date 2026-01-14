using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monq.Core.VictoriaMetricsClient.Exceptions;
using Monq.Core.VictoriaMetricsClient.Extensions;
using Monq.Core.VictoriaMetricsClient.Models;
using Monq.Core.VictoriaMetricsClient.SerializerContexts;
using System.Net.Http.Json;

namespace Monq.Core.VictoriaMetricsClient;

public sealed class VictoriaProxyClient : IVictoriaProxyClient
{
    readonly HttpClient _httpClient;
    readonly ILogger<VictoriaProxyClient> _logger;
    readonly VictoriaOptions _victoriaOptions;

    public VictoriaProxyClient(
        HttpClient httpClient,
        ILogger<VictoriaProxyClient> logger,
        IOptions<VictoriaOptions> victoriaOptions)
    {
        _httpClient = httpClient;
        _logger = logger;
        _victoriaOptions = victoriaOptions?.Value
            ?? throw new StorageConfigurationException("There is not configuration found for the VictoriaMetrics.");
    }

    /// <inheritdoc />
    public ValueTask<BaseResponseModel> Labels(
        IQueryCollection requestQuery,
        CancellationToken cancellationToken = default)
        => AllGrantedRequest("labels", requestQuery, cancellationToken);

    /// <inheritdoc />
    public ValueTask<BaseResponseModel> Labels(
        IQueryCollection requestQuery,
        long userspaceId,
        IEnumerable<long> streamIds,
        CancellationToken cancellationToken = default)
        => DefaultRequest("labels", requestQuery, userspaceId, streamIds, cancellationToken);

    /// <inheritdoc />
    public ValueTask<BaseResponseModel> LabelValues(
        string label,
        IQueryCollection requestQuery,
        long userspaceId,
        IEnumerable<long> streamIds,
        bool allowSkipExtraContent = true,
        CancellationToken cancellationToken = default)
    {
        if (allowSkipExtraContent && label == "__name__")
            return AllGrantedRequest($"label/{label}/values", requestQuery, cancellationToken);
        else
            return DefaultRequest($"label/{label}/values", requestQuery, userspaceId, streamIds, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask<BaseResponseModel> Series(
        IQueryCollection requestQuery,
        long userspaceId,
        IEnumerable<long> streamIds,
        CancellationToken cancellationToken = default)
        => DefaultRequest("series", requestQuery, userspaceId, streamIds, cancellationToken);

    /// <inheritdoc />
    public ValueTask<BaseResponseModel> Query(
        IQueryCollection requestQuery,
        long userspaceId,
        IEnumerable<long> streamIds,
        CancellationToken cancellationToken = default)
        => DefaultRequest("query", requestQuery, userspaceId, streamIds, cancellationToken);

    /// <inheritdoc />
    public ValueTask<BaseResponseModel> QueryRange(
        IQueryCollection requestQuery,
        long userspaceId,
        IEnumerable<long> streamIds,
        CancellationToken cancellationToken = default)
        => DefaultRequest("query_range", requestQuery, userspaceId, streamIds, cancellationToken);

    /// <inheritdoc />
    public ValueTask<BaseResponseModel> BuildInfo(
        IQueryCollection requestQuery,
        CancellationToken cancellationToken = default)
        => AllGrantedRequest("status/buildinfo", requestQuery, cancellationToken);

    /// <inheritdoc />
    public ValueTask<BaseResponseModel> QueryExemplars(
        IQueryCollection requestQuery,
        long userspaceId,
        IEnumerable<long> streamIds,
        CancellationToken cancellationToken = default)
        => DefaultRequest("query_exemplars", requestQuery, userspaceId, streamIds, cancellationToken);

    /// <inheritdoc />
    public ValueTask<BaseResponseModel> Metadata(
        IQueryCollection requestQuery,
        CancellationToken cancellationToken = default)
        => AllGrantedRequest("metadata", requestQuery, cancellationToken);

    /// <inheritdoc />
    public ValueTask<BaseResponseModel> Rules(
        IQueryCollection requestQuery,
        CancellationToken cancellationToken = default)
        => AllGrantedRequest("rules", requestQuery, cancellationToken);

    /// <inheritdoc />
    async ValueTask<BaseResponseModel> PostRequest(
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
            var message = $"Storage throws exception on request. Details: {e.Message}";
            _logger.LogError(e, message);
            return new BaseResponseModel
            {
                Error = message,
                Status = PrometheusResponseStatuses.error
            };
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrEmpty(responseContent))
        {
            return new BaseResponseModel
            {
                Error = "Storage responded with empty message.",
                Status = PrometheusResponseStatuses.error
            };
        }

        try
        {
            var responseMessage = await response.Content
                .ReadFromJsonAsync(BaseResponseModelSerializerContext.Default.BaseResponseModel, cancellationToken);
            if (responseMessage is null)
                return new BaseResponseModel
                {
                    Error = "Storage responded with empty message.",
                    Status = PrometheusResponseStatuses.error
                };

            return responseMessage;
        }
        catch (System.Text.Json.JsonException)
        {
            return new BaseResponseModel
            {
                Error = "Storage responded with invalid JSON.",
                Status = PrometheusResponseStatuses.error
            };
        }
    }

    async ValueTask<BaseResponseModel> DefaultRequest(
        string requestUri,
        IQueryCollection requestQuery,
        long userspaceId,
        IEnumerable<long> streamIds,
        CancellationToken cancellationToken)
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

        return await PostRequest(requestUri, contentParams, cancellationToken);
    }

    async ValueTask<BaseResponseModel> AllGrantedRequest(
        string requestUri,
        IQueryCollection requestQuery,
        CancellationToken cancellationToken)
    {
        var contentParams = new Dictionary<string, string>();

        foreach (var queryParam in requestQuery)
            contentParams.Add(queryParam.Key, queryParam.Value.ToString());

        return await PostRequest(requestUri, contentParams, cancellationToken);
    }
}
