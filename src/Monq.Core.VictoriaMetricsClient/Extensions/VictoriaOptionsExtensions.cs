namespace Monq.Core.VictoriaMetricsClient;

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
    {
        return options.SystemLabelPrefix + VictoriaConstants.MetricsRequestLabels.UserspaceIdLabelName;
    }

    /// <summary>
    /// Get add stream id label.
    /// </summary>
    /// <param name="options">The VictoriaMetrics client connection options</param>
    /// <returns></returns>
    public static string GetStreamIdLabelName(this VictoriaOptions options)
    {
        return options.SystemLabelPrefix + VictoriaConstants.MetricsRequestLabels.StreamIdLabelName;
    }
}