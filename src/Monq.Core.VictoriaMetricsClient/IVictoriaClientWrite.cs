using Monq.Core.VictoriaMetricsClient.Models;
using PrometheusGrpc;

namespace Monq.Core.VictoriaMetricsClient;

public interface IVictoriaClientWrite
{
    ValueTask Write(WriteRequest request);
}
