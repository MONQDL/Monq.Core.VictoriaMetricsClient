using Google.Protobuf;
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
    public async ValueTask Write(WriteRequest request)
    {
        var compressedMessage = IronSnappy.Snappy.Encode(request.ToByteArray());

        var byteArrayContent = new ByteArrayContent(compressedMessage);
        byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-protobuf");

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync("write", byteArrayContent);
        }
        catch (Exception e)
        {
            throw new StorageException($"Storage. Can't store message due to exception. Details: {e.Message}", e);
        }

        if (!response.IsSuccessStatusCode)
            throw new StorageException($"Storage. Victoria responded with status code: {(int)response.StatusCode}. " +
                $"Can't store message due to exception. " +
                $"Details: {await response.Content.ReadAsStringAsync()}");
    }
}
