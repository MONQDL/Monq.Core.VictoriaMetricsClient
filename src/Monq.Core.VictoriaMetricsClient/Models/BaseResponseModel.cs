using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Monq.Core.VictoriaMetricsClient.Models;

/// <summary>
/// Базовая модель ответа Prometheus.
/// </summary>
public class BaseResponseModel
{
    public PrometheusResponseStatuses Status { get; set; }
    public JsonNode Data { get; set; }

    /// <summary>
    /// Only set if status is "error". The data field may still hold additional data.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorType { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; set; }

    /// <summary>
    /// Only if there were warnings while executing the request.
    /// There will still be data in the data field.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<string>? Warnings { get; set; }
}
