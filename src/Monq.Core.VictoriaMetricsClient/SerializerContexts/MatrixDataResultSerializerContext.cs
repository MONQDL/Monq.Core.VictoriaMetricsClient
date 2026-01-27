using Monq.Core.VictoriaMetricsClient.Models;
using System.Text.Json.Serialization;

namespace Monq.Core.VictoriaMetricsClient.SerializerContexts;

/// <summary>
/// <see cref="Models.MatrixDataResult"/> serializer context.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    NumberHandling = JsonNumberHandling.AllowReadingFromString)]
[JsonSerializable(typeof(MatrixDataResult))]
[JsonSerializable(typeof(MatrixDataResult[]))]
public sealed partial class MatrixDataResultSerializerContext : JsonSerializerContext
{
}
