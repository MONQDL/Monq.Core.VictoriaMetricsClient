using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Monq.Core.VictoriaMetricsClient.Models;

/// <summary>
/// Prometheus base response model.
/// </summary>
public sealed class BaseResponseModel
{
    /// <summary>
    /// Prometheus response status.
    /// </summary>
    public required PrometheusResponseStatuses Status { get; init; }

    /// <summary>
    /// Response data.
    /// </summary>
    public JsonNode? Data { get; init; }

    /// <summary>
    /// Only set if status is "error". The data field may still hold additional data.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorType { get; set; }

    /// <summary>
    /// Error message.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; set; }

    /// <summary>
    /// Only if there were warnings while executing the request.
    /// There will still be data in the data field.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<string>? Warnings { get; set; }
}
