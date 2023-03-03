namespace Monq.Core.VictoriaMetricsClient.Models;

public class MatrixDataResult
{
    public Dictionary<string, string> Metric { get; set; }
    public IEnumerable<double[]> Values { get; set; }
}
