namespace Monq.Core.VictoriaMetricsClient.Models;

/// <summary>
/// Matrix data result.
/// </summary>
public class MatrixDataResult
{
    /// <summary>
    /// Metric.
    /// </summary>
    public Dictionary<string, string> Metric { get; set; } = [];

    /// <summary>
    /// Values.
    /// </summary>
    public IEnumerable<double[]> Values { get; set; } = [];
}
