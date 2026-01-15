using Monq.Core.VictoriaMetricsClient.Models;
using System.Text.Json.Serialization;

namespace Monq.Core.VictoriaMetricsClient.SerializerContexts;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    NumberHandling = JsonNumberHandling.AllowReadingFromString)]
[JsonSerializable(typeof(MatrixDataResult))]
[JsonSerializable(typeof(MatrixDataResult[]))]
sealed partial class MatrixDataResultSerializerContext : JsonSerializerContext
{
}
