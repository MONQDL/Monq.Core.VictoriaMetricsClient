using Monq.Core.VictoriaMetricsClient.Models;

namespace Monq.Core.VictoriaMetricsClient;

public interface IVictoriaClientRead

{
    /// <summary>
    /// Run a request to read metrics from the repository.
    /// </summary>
    /// <returns></returns>
    ValueTask<BaseQueryDataResponse> Query(string query,
        string step,
        IEnumerable<long> streamIds,
        long userspaceId);

    /// <summary>
    /// Run a query to read the list of metrics by range from the storage.
    /// </summary>
    /// <returns></returns>
    ValueTask<BaseQueryDataResponse> QueryRange(string query,
        DateTimeOffset start,
        DateTimeOffset end,
        TimeInterval step,
        IEnumerable<long> streamIds,
        long userspaceId);
}
