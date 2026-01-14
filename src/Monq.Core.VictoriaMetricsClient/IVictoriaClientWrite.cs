using PrometheusGrpc;

namespace Monq.Core.VictoriaMetricsClient;

public interface IVictoriaClientWrite
{
    ValueTask Write(
        WriteRequest request,
        CancellationToken cancellationToken = default);
}
