using Google.Protobuf;
using Monq.Core.VictoriaMetricsClient.Exceptions;
using PrometheusGrpc;
using System.Net.Http.Headers;

namespace Monq.Core.VictoriaMetricsClient;

public class VictoriaClientWrite : IVictoriaClientWrite
{
    readonly HttpClient _httpClient;

    public VictoriaClientWrite(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async ValueTask Write(
        WriteRequest request,
        CancellationToken cancellationToken = default)
    {
        var compressedMessage = IronSnappy.Snappy.Encode(request.ToByteArray());

        var byteArrayContent = new ByteArrayContent(compressedMessage);
        byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-protobuf");

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync("write", byteArrayContent, cancellationToken);
        }
        catch (Exception e)
        {
            throw new StorageException($"Storage threw exception on request. Details: {e.Message}", e);
        }

        if (!response.IsSuccessStatusCode)
            throw new StorageException($"""
                Storage responded with status code: {(int)response.StatusCode}.
                Cannot store message due to an error.
                Details: {await response.Content.ReadAsStringAsync(cancellationToken)}
                """);
    }
}
