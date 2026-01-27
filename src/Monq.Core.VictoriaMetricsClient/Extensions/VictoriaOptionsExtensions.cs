namespace Monq.Core.VictoriaMetricsClient.Extensions;

/// <summary>
/// VictoriaMetrics client connection options extension methods.
/// </summary>
public static class VictoriaOptionsExtensions
{
    /// <summary>
    /// Get userspace id label.
    /// </summary>
    /// <param name="options">VictoriaMetrics client connection options.</param>
    /// <returns></returns>
    public static string GetUserspaceIdLabelName(this VictoriaOptions options)
        => options.SystemLabelPrefix + VictoriaConstants.MetricsRequestLabels.UserspaceIdLabelName;

    /// <summary>
    /// Get add stream id label.
    /// </summary>
    /// <param name="options">VictoriaMetrics client connection options.</param>
    /// <returns></returns>
    public static string GetStreamIdLabelName(this VictoriaOptions options)
        => options.SystemLabelPrefix + VictoriaConstants.MetricsRequestLabels.StreamIdLabelName;
}
