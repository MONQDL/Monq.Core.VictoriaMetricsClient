namespace Monq.Core.VictoriaMetricsClient.Models;

/// <summary>
/// Query result type.
/// </summary>
public enum QueryResultTypes
{
    /// <summary>
    /// Matrix.
    /// </summary>
    matrix,

    /// <summary>
    /// Vector.
    /// </summary>
    vector,

    /// <summary>
    /// Scalar.
    /// </summary>
    scalar,

    /// <summary>
    /// String.
    /// </summary>
    @string
}
