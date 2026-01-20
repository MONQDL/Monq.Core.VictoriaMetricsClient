namespace Monq.Core.VictoriaMetricsClient;

/// <summary>
/// VectoriaMetrics client constants.
/// </summary>
public static class VictoriaConstants
{
    /// <summary>
    /// Labels.
    /// </summary>
    public static class MetricsRequestLabels
    {
        /// <summary>
        /// Userspace Id label.
        /// </summary>
        public const string UserspaceIdLabelName = "userspace_id";

        /// <summary>
        /// Stream Id label.
        /// </summary>
        public const string StreamIdLabelName = "stream_id";
    }
}
