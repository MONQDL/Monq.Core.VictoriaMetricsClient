namespace Monq.Core.VictoriaMetricsClient.Models;

public class VectorDataResult
{
    public Dictionary<string, string> Metric { get; set; } = new Dictionary<string, string>();
    public float[] Value { get; set; } = { 0, 0 };
}
