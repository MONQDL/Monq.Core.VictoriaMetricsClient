namespace Monq.Core.VictoriaMetricsClient.Models;

/// <summary>
/// The representation of the time intervals in Prometheus. 
/// </summary>
public class TimeInterval
{
    /// <summary>
    /// The numerical value of the interval.
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// The interval unit is seconds, minutes, hours, days.
    /// </summary>
    public TimeIntervalUnits Units { get; set; }

    public TimeInterval(int value, TimeIntervalUnits units)
    {
        Value = value;
        Units = units;
    }

    TimeInterval() { } // For serializers.
}
