namespace Monq.Core.VictoriaMetricsClient.Extensions;

/// <summary>
/// The VictoriaMetrics client connection options extensions.
/// </summary>
public static class VictoriaOptionsExtensions
{

    /// <summary>
    /// Get userspace id label.
    /// </summary>
    /// <param name="options">The VictoriaMetrics client connection options</param>
    /// <returns></returns>
    public static string GetUserspaceIdLabelName(this VictoriaOptions options)
        => options.SystemLabelPrefix + VictoriaConstants.MetricsRequestLabels.UserspaceIdLabelName;

    /// <summary>
    /// Get add stream id label.
    /// </summary>
    /// <param name="options">The VictoriaMetrics client connection options</param>
    /// <returns></returns>
    public static string GetStreamIdLabelName(this VictoriaOptions options)
        => options.SystemLabelPrefix + VictoriaConstants.MetricsRequestLabels.StreamIdLabelName;
}
