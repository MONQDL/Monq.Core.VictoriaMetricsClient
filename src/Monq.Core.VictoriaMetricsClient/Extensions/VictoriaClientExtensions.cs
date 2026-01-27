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
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    public static async ValueTask<MatrixDataResult[]> QueryMatrixRange(
        this IVictoriaClientRead victoriaClient,
        string query,
        DateTimeOffset start,
        DateTimeOffset end,
        TimeInterval step,
        IEnumerable<long> streamIds,
        long userspaceId,
        CancellationToken cancellationToken = default)
    {
        var queryDataResponse = await victoriaClient.QueryRange(
            query, start, end, step, streamIds, userspaceId, cancellationToken);

        if (queryDataResponse.ResultType != QueryResultTypes.Matrix)
            throw new StorageException("""Query does not return "matrix" result.""");

        var dataResult = queryDataResponse.Result.Deserialize(
            MatrixDataResultSerializerContext.Default.MatrixDataResultArray)
            ?? [];

        return dataResult;
    }

    /// <summary>
    /// Run a query to read metrics from the storage and get a result as vector.
    /// </summary>
    /// <param name="victoriaClient"><see cref="IVictoriaClientRead"/>.</param>
    /// <param name="query">Query.</param>
    /// <param name="step">Time interval.</param>
    /// <param name="streamIds">Stream Ids.</param>
    /// <param name="userspaceId">Userspace Id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    public static async ValueTask<VectorDataResult[]> QueryVector(
        this IVictoriaClientRead victoriaClient,
        string query,
        string step,
        IEnumerable<long> streamIds,
        long userspaceId,
        CancellationToken cancellationToken = default)
    {
        var queryDataResponse = await victoriaClient.Query(
            query, step, streamIds, userspaceId, cancellationToken);

        if (queryDataResponse.ResultType != QueryResultTypes.Vector)
            throw new StorageException("""Query does not return "vector" result.""");

        var dataResult = queryDataResponse.Result.Deserialize(
            VectorDataResultSerializerContext.Default.VectorDataResultArray)
            ?? [];

        return dataResult;
    }
}
