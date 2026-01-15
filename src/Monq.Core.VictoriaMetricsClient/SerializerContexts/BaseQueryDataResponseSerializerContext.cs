using Monq.Core.VictoriaMetricsClient.Models;
using System.Text.Json.Serialization;

namespace Monq.Core.VictoriaMetricsClient.SerializerContexts;

/// <summary>
/// <see cref="Models.BaseQueryDataResponse"/> serializer context.
/// </summary>
[JsonSourceGenerationOptions(
    UseStringEnumConverter = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    NumberHandling = JsonNumberHandling.AllowReadingFromString)]
[JsonSerializable(typeof(BaseQueryDataResponse))]
public sealed partial class BaseQueryDataResponseSerializerContext : JsonSerializerContext
{
}
