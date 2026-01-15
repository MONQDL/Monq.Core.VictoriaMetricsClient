using Monq.Core.VictoriaMetricsClient.Exceptions;
using Monq.Core.VictoriaMetricsClient.Models;
using Monq.Core.VictoriaMetricsClient.SerializerContexts;
using System.Text.Json;

namespace Monq.Core.VictoriaMetricsClient.Extensions;

/// <summary>
/// VictoriaMetrics client extension methods.
/// </summary>
public static class VictoriaClientExtensions
{
    /// <summary>
    /// Run a query to read metrics from the storage by date range and get a result as matrix.
    /// </summary>
    /// <param name="victoriaClient"><see cref="IVictoriaClientRead"/>.</param>
    /// <param name="query">Query.</param>
    /// <param name="start">The start of the date range.</param>
    /// <param name="end">The end of the date range.</param>
    /// <param name="step">Time interval.</param>
    /// <param name="streamIds">Stream Ids.</param>
    /// <param name="userspaceId">Userspace Id.</param>
    /// <returns></returns>
    public static async Task<MatrixDataResult[]> QueryMatrixRange(
        this IVictoriaClientRead victoriaClient,
        string query,
        DateTimeOffset start,
        DateTimeOffset end,
        TimeInterval step,
        IEnumerable<long> streamIds,
        long userspaceId)
    {
        var queryDataResponse = await victoriaClient.QueryRange(query, start, end, step, streamIds, userspaceId);

        if (queryDataResponse.ResultType != QueryResultTypes.matrix)
            throw new StorageException("""Query does not return "matrix" result.""");

        var dataResult = queryDataResponse.Result.Deserialize(
            MatrixDataResultSerializerContext.Default.MatrixDataResultArray)
            ?? [];

        return dataResult;
    }
}
