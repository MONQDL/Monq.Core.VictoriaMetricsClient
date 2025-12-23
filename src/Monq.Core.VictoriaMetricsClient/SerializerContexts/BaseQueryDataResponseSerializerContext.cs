using Monq.Core.VictoriaMetricsClient.Models;
using System.Text.Json.Serialization;

namespace Monq.Core.VictoriaMetricsClient.SerializerContexts;

[JsonSerializable(typeof(BaseQueryDataResponse))]
[JsonSourceGenerationOptions(UseStringEnumConverter = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    NumberHandling = JsonNumberHandling.AllowReadingFromString)]
internal partial class BaseQueryDataResponseSerializerContext : JsonSerializerContext
{
}
