namespace Monq.Core.VictoriaMetricsClient.Models;

/// <summary>
/// Query result type.
/// </summary>
public enum QueryResultTypes
{
    /// <summary>
    /// Matrix.
    /// </summary>
    Matrix,

    /// <summary>
    /// Vector.
    /// </summary>
    Vector,

    /// <summary>
    /// Scalar.
    /// </summary>
    Scalar,

    /// <summary>
    /// String.
    /// </summary>
    String
}
