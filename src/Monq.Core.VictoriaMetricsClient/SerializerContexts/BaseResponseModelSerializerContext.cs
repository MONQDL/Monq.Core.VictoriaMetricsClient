using Monq.Core.VictoriaMetricsClient.Models;
using System.Text.Json.Serialization;

namespace Monq.Core.VictoriaMetricsClient.SerializerContexts;

[JsonSerializable(typeof(BaseResponseModel))]
[JsonSourceGenerationOptions(UseStringEnumConverter = true,
    PropertyNamingPolicy =  JsonKnownNamingPolicy.CamelCase,
    NumberHandling = JsonNumberHandling.AllowReadingFromString)]
internal partial class BaseResponseModelSerializerContext : JsonSerializerContext
{
}
