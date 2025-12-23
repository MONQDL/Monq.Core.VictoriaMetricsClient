using Monq.Core.VictoriaMetricsClient.Models;

namespace Monq.Core.VictoriaMetricsClient.Extensions;

public static class TimeIntervalExtensions
{
    /// <summary>
    /// Convert the <paramref name="timeInterval"/> object to seconds.
    /// </summary>
    /// <param name="timeInterval">Time interval.</param>
    /// <returns></returns>
    public static int ToSeconds(this TimeInterval timeInterval)
    {
        int modifier;
        switch (timeInterval.Units)
        {
            case TimeIntervalUnits.Seconds: modifier = 1; break;
            case TimeIntervalUnits.Minutes: modifier = 60; break;
            case TimeIntervalUnits.Hours: modifier = 60 * 60; break;
            case TimeIntervalUnits.Days: modifier = 60 * 60 * 24; break;
            default: modifier = 1; break;
        }
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
        string promQlUnits;
        switch (timeInterval.Units)
        {
            case TimeIntervalUnits.Seconds: promQlUnits = "s"; break;
            case TimeIntervalUnits.Minutes: promQlUnits = "m"; break;
            case TimeIntervalUnits.Hours: promQlUnits = "h"; break;
            case TimeIntervalUnits.Days: promQlUnits = "d"; break;
            default: promQlUnits = "m"; break;
        }

        return $"{timeInterval.Value}{promQlUnits}";
    }
}
