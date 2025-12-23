using System.Text.Json.Nodes;

namespace Monq.Core.VictoriaMetricsClient.Models;

public sealed class BaseQueryDataResponse
{
    public required QueryResultTypes ResultType { get; init; }
    public required JsonArray Result { get; init; }
}
