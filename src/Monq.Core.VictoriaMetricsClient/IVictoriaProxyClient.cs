using Microsoft.AspNetCore.Http;
using Monq.Core.VictoriaMetricsClient.Models;

namespace Monq.Core.VictoriaMetricsClient;

/// <summary>
/// Interface for the client proxy requests to VictoriaMetrics with the addition of extra_labels.
/// </summary>
public interface IVictoriaProxyClient
{
    /// <summary>
    /// Run a query to read the list of labels with forwarding query parameters.
    /// </summary>
    /// <param name="requestQuery">Query parameters set by the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    ValueTask<BaseResponseModel> Labels(
        IQueryCollection requestQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Run a query to read the list of labels with forwarding query parameters.
    /// </summary>
    /// <param name="requestQuery">Query parameters set by the user.</param>
    /// <param name="userspaceId">The id of the userspace within which the request will be executed.</param>
    /// <param name="streamIds">List of stream Ids available to the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    ValueTask<BaseResponseModel> Labels(
        IQueryCollection requestQuery,
        long userspaceId,
        IEnumerable<long> streamIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Run a query to read the label values with forwarding query parameters.
    /// </summary>
    /// <param name="label">Label.</param>
    /// <param name="requestQuery">Query parameters set by the user.</param>
    /// <param name="userspaceId">The id of the userspace within which the request will be executed.</param>
    /// <param name="streamIds">List of stream Ids available to the user.</param>
    /// <param name="allowSkipExtraContent">Allow skipping extra content with streamIds and userspaceId for label `__name__`.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    ValueTask<BaseResponseModel> LabelValues(
        string label,
        IQueryCollection requestQuery,
        long userspaceId,
        IEnumerable<long> streamIds,
        bool allowSkipExtraContent = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find series by label matchers.
    /// </summary>
    /// <param name="requestQuery">Query parameters set by the user.</param>
    /// <param name="userspaceId">The id of the userspace within which the request will be executed.</param>
    /// <param name="streamIds">List of stream Ids available to the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    ValueTask<BaseResponseModel> Series(
        IQueryCollection requestQuery,
        long userspaceId,
        IEnumerable<long> streamIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Perform PromQL/MetricsQL instant query.
    /// </summary>
    /// <param name="requestQuery">Query parameters set by the user.</param>
    /// <param name="userspaceId">The id of the userspace within which the request will be executed.</param>
    /// <param name="streamIds">List of stream Ids available to the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    ValueTask<BaseResponseModel> Query(
        IQueryCollection requestQuery,
        long userspaceId,
        IEnumerable<long> streamIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Perform PromQL/MetricsQL range query.
    /// </summary>
    /// <param name="requestQuery">Query parameters set by the user.</param>
    /// <param name="userspaceId">The id of the userspace within which the request will be executed.</param>
    /// <param name="streamIds">List of stream Ids available to the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    ValueTask<BaseResponseModel> QueryRange(
        IQueryCollection requestQuery,
        long userspaceId,
        IEnumerable<long> streamIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Perform PromQL/MetricsQL build_info query.
    /// </summary>
    /// <param name="requestQuery">Query parameters set by the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    ValueTask<BaseResponseModel> BuildInfo(
        IQueryCollection requestQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Perform PromQL/MetricsQL query exemplars.
    /// </summary>
    /// <param name="requestQuery">Query parameters set by the user.</param>
    /// <param name="userspaceId">The id of the userspace within which the request will be executed.</param>
    /// <param name="streamIds">List of stream Ids available to the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    ValueTask<BaseResponseModel> QueryExemplars(
        IQueryCollection requestQuery,
        long userspaceId,
        IEnumerable<long> streamIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Perform PromQL/MetricsQL metadata query.
    /// </summary>
    /// <param name="requestQuery">Query parameters set by the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    ValueTask<BaseResponseModel> Metadata(
        IQueryCollection requestQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Perform PromQL/MetricsQL rules query.
    /// </summary>
    /// <param name="requestQuery">Query parameters set by the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    ValueTask<BaseResponseModel> Rules(
        IQueryCollection requestQuery,
        CancellationToken cancellationToken = default);
}
