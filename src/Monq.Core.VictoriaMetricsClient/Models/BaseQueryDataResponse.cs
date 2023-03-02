using System.Text.Json.Nodes;

namespace Monq.Core.VictoriaMetricsClient.Models;

public class BaseQueryDataResponse
{
    public QueryResultTypes ResultType { get; set; }
    public JsonArray Result { get; set; }
}
