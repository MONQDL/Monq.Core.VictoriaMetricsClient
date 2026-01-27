using Monq.Core.VictoriaMetricsClient.Models;

namespace Monq.Core.VictoriaMetricsClient;

/// <summary>
/// VictoriaClient read operations interface.
/// </summary>
public interface IVictoriaClientRead
{
    /// <summary>
    /// Run a query to read metrics from the storage.
    /// </summary>
    /// <returns></returns>
    ValueTask<BaseQueryDataResponse> Query(
        string query,
        string step,
        IEnumerable<long> streamIds,
        long userspaceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Run a query to read metrics from the storage by date range.
    /// </summary>
    /// <returns></returns>
    ValueTask<BaseQueryDataResponse> QueryRange(
        string query,
        DateTimeOffset start,
        DateTimeOffset end,
        TimeInterval step,
        IEnumerable<long> streamIds,
        long userspaceId,
        CancellationToken cancellationToken = default);
}
