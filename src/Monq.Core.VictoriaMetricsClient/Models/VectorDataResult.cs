namespace Monq.Core.VictoriaMetricsClient.Models;

/// <summary>
/// Vector data result.
/// </summary>
public class VectorDataResult
{
    /// <summary>
    /// Metric.
    /// </summary>
    public Dictionary<string, string> Metric { get; set; } = [];

    /// <summary>
    /// Value.
    /// </summary>
    public float[] Value { get; set; } = [0, 0];
}
