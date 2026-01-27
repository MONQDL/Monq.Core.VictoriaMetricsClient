using PrometheusGrpc;

namespace Monq.Core.VictoriaMetricsClient;

/// <summary>
/// VicrotiaClient write operations interface.
/// </summary>
public interface IVictoriaClientWrite
{
    /// <summary>
    /// Write metrics to the storage.
    /// </summary>
    /// <param name="request">Write request in Prometheus format.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    ValueTask Write(
        WriteRequest request,
        CancellationToken cancellationToken = default);
}
