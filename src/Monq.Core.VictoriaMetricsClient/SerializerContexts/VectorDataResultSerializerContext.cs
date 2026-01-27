using Monq.Core.VictoriaMetricsClient.Models;
using System.Text.Json.Serialization;

namespace Monq.Core.VictoriaMetricsClient.SerializerContexts;

/// <summary>
/// <see cref="Models.VectorDataResult"/> serializer context.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    NumberHandling = JsonNumberHandling.AllowReadingFromString)]
[JsonSerializable(typeof(VectorDataResult))]
[JsonSerializable(typeof(VectorDataResult[]))]
public sealed partial class VectorDataResultSerializerContext : JsonSerializerContext
{
}
