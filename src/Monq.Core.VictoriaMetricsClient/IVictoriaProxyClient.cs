using Microsoft.AspNetCore.Http;
using Monq.Core.VictoriaMetricsClient.Models;

namespace Monq.Core.VictoriaMetricsClient;

/// <summary>
/// The client proxies requests to Vicroria Metrics with the addition of extra_labels.
/// </summary>
public interface IVictoriaProxyClient
{
    /// <summary>
    /// Execute a request to read the list of labels with forwarding query parameters.
    /// </summary>
    /// <param name="requestQuery">Query parameters set by the user.</param>
    /// <returns></returns>
    ValueTask<BaseResponseModel> Labels(IQueryCollection requestQuery);

    /// <summary>
    /// Execute a request to read the label values with forwarding query parameters.
    /// </summary>
    /// <param name="label"></param>
    /// <param name="requestQuery">Query parameters set by the user.</param>
    /// <param name="userspaceId">The id of the user space within which the request will be executed.</param>
    /// <param name="streamIds">List of thread IDs available to the user.</param>
    /// <param name="allowSkipExtraContent">Allow skip extra content with stremIds and userspaceId for label `__name__`.</param>
    /// <returns></returns>
    ValueTask<BaseResponseModel> LabelValues(string label,
        IQueryCollection requestQuery,
        long userspaceId,
        IEnumerable<long> streamIds,
        bool allowSkipExtraContent = true);

    /// <summary>
    /// Finding series by label matchers.
    /// </summary>
    /// <param name="requestQuery">Query parameters set by the user.</param>
    /// <param name="userspaceId">The id of the user space within which the request will be executed.</param>
    /// <param name="streamIds">List of thread IDs available to the user.</param>
    /// <returns></returns>
    ValueTask<BaseResponseModel> Series(IQueryCollection requestQuery,
        long userspaceId,
        IEnumerable<long> streamIds);

    /// <summary>
    /// Performs PromQL/MetricsQL instant query.
    /// </summary>
    /// <param name="requestQuery">Query parameters set by the user.</param>
    /// <param name="userspaceId">The id of the user space within which the request will be executed.</param>
    /// <param name="streamIds">List of thread IDs available to the user.</param>
    /// <returns></returns>
    ValueTask<BaseResponseModel> Query(IQueryCollection requestQuery,
        long userspaceId,
        IEnumerable<long> streamIds);

    /// <summary>
    /// Performs PromQL/MetricsQL range query.
    /// </summary>
    /// <param name="requestQuery">Query parameters set by the user.</param>
    /// <param name="userspaceId">The id of the user space within which the request will be executed.</param>
    /// <param name="streamIds">List of thread IDs available to the user.</param>
    /// <returns></returns>
    ValueTask<BaseResponseModel> QueryRange(IQueryCollection requestQuery,
        long userspaceId,
        IEnumerable<long> streamIds);

    /// <summary>
    /// Performs PromQL/MetricsQL build_info query.
    /// </summary>
    /// <param name="requestQuery"></param>
    /// <returns></returns>
    ValueTask<BaseResponseModel> BuildInfo(IQueryCollection requestQuery);

    /// <summary>
    /// Performs PromQL/MetricsQL query exemplars.
    /// </summary>
    /// <param name="requestQuery">Query parameters set by the user.</param>
    /// <param name="userspaceId">The id of the user space within which the request will be executed.</param>
    /// <param name="streamIds">List of thread IDs available to the user.</param>
    /// <returns></returns>
    ValueTask<BaseResponseModel> QueryExemplars(IQueryCollection requestQuery,
        long userspaceId,
        IEnumerable<long> streamIds);

    /// <summary>
    /// Performs PromQL/MetricsQL metadata query.
    /// </summary>
    /// <param name="requestQuery">Query parameters set by the user.</param>
    /// <returns></returns>
    ValueTask<BaseResponseModel> Metadata(IQueryCollection requestQuery);

    /// <summary>
    /// Performs PromQL/MetricsQL rules query.
    /// </summary>
    /// <param name="requestQuery">Query parameters set by the user.</param>
    /// <returns></returns>
    ValueTask<BaseResponseModel> Rules(IQueryCollection requestQuery);
}
