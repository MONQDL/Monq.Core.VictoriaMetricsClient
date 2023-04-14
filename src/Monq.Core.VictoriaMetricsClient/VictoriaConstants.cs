namespace Monq.Core.VictoriaMetricsClient;

public static class VictoriaConstants
{
    public static class MetricsRequestLabels
    {
        public static string UserspaceIdLabelName { get; private set; } = "userspace_id";
        public static string StreamIdLabelName { get; private set; } = "stream_id";

        internal static void AddUserspaceIdLabelPrefix(string prefix)
        {
            UserspaceIdLabelName = prefix + UserspaceIdLabelName;
        }

        internal static void AddStreamIdLabelPrefix(string prefix)
        {
            StreamIdLabelName = prefix + StreamIdLabelName;
        }
    }
}
