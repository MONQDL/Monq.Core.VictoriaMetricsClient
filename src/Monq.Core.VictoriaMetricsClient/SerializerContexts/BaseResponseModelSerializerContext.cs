using Monq.Core.VictoriaMetricsClient.Models;
using System.Text.Json.Serialization;

namespace Monq.Core.VictoriaMetricsClient.SerializerContexts;

/// <summary>
/// <see cref="Models.BaseResponseModel"/> serializer context.
/// </summary>
[JsonSourceGenerationOptions(
    UseStringEnumConverter = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    NumberHandling = JsonNumberHandling.AllowReadingFromString)]
[JsonSerializable(typeof(BaseResponseModel))]
public sealed partial class BaseResponseModelSerializerContext : JsonSerializerContext
{
}
