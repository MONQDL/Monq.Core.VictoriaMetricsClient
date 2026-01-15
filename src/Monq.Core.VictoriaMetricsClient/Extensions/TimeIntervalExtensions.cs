using Monq.Core.VictoriaMetricsClient.Models;

namespace Monq.Core.VictoriaMetricsClient.Extensions;

/// <summary>
/// Time interval extension methods.
/// </summary>
public static class TimeIntervalExtensions
{
    /// <summary>
    /// Convert the <paramref name="timeInterval"/> object to seconds.
    /// </summary>
    /// <param name="timeInterval">Time interval.</param>
    /// <returns></returns>
    public static int ToSeconds(this TimeInterval timeInterval)
    {
        var modifier = timeInterval.Units switch
        {
            TimeIntervalUnits.Seconds => 1,
            TimeIntervalUnits.Minutes => 60,
            TimeIntervalUnits.Hours => 60 * 60,
            TimeIntervalUnits.Days => 60 * 60 * 24,
            _ => 1,
        };
        return timeInterval.Value * modifier;
    }

    /// <summary>
    /// Convert the <paramref name="timeInterval"/> object to a string value 
    /// compatible with the PromQL time interval query.
    /// </summary>
    /// <param name="timeInterval">Time interval.</param>
    /// <returns></returns>
    public static string ToPromQlInterval(this TimeInterval timeInterval)
    {
        var promQlUnits = timeInterval.Units switch
        {
            TimeIntervalUnits.Seconds => "s",
            TimeIntervalUnits.Minutes => "m",
            TimeIntervalUnits.Hours => "h",
            TimeIntervalUnits.Days => "d",
            _ => "m",
        };
        return $"{timeInterval.Value}{promQlUnits}";
    }
}
