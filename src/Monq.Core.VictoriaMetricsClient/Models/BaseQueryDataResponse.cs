using System.Text.Json.Nodes;

namespace Monq.Core.VictoriaMetricsClient.Models;

/// <summary>
/// Data response.
/// </summary>
public sealed class BaseQueryDataResponse
{
    /// <summary>
    /// Result type.
    /// </summary>
    public required QueryResultTypes ResultType { get; init; }

    /// <summary>
    /// Result.
    /// </summary>
    public required JsonArray Result { get; init; }
}
