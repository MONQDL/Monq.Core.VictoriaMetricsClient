using Microsoft.Extensions.Options;
using Monq.Core.VictoriaMetricsClient.Exceptions;
using Monq.Core.VictoriaMetricsClient.Models;
using Monq.Core.VictoriaMetricsClient.SerializerContexts;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Monq.Core.VictoriaMetricsClient.Tests;

public class VictoriaClientReadTests
{
    readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    readonly HttpClient _httpClient;
    readonly VictoriaOptions _victoriaOptions;
    readonly Mock<IOptions<VictoriaOptions>> _optionsMock;

    public VictoriaClientReadTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://local.com")
        };
        _victoriaOptions = new VictoriaOptions { SystemLabelPrefix = "monq_" };
        _optionsMock = new Mock<IOptions<VictoriaOptions>>();
        _optionsMock.Setup(x => x.Value).Returns(_victoriaOptions);
    }

    [Fact(DisplayName = "[Query] Вызов метода с валидными параметрами должен возвращать успешный результат")]
    public async Task Query_WithValidParameters_ShouldReturnSuccessResult()
    {
        // Arrange
        var query = "up";
        var step = "15s";
        var streamIds = new List<long> { 1, 2, 3 };
        var userspaceId = 100L;

        var expectedResponse = CreateSuccessResponse();
        var jsonContent = JsonSerializer.Serialize(expectedResponse, BaseResponseModelSerializerContext.Default.BaseResponseModel);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            });

        var client = new VictoriaClientRead(_httpClient, _optionsMock.Object);

        // Act
        var result = await client.Query(query, step, streamIds, userspaceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(QueryResultTypes.matrix, result.ResultType);
        Assert.NotNull(result.Result);
    }

    [Fact(DisplayName = "[Query] Проверка валидации пустого параметра query")]
    public async Task Query_WithEmptyQuery_ShouldThrowStorageException()
    {
        // Arrange
        var query = "";
        var step = "15s";
        var streamIds = new List<long> { 1, 2, 3 };
        var userspaceId = 100L;

        var client = new VictoriaClientRead(_httpClient, _optionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<StorageException>(async () =>
            await client.Query(query, step, streamIds, userspaceId));

        Assert.Contains("query is null or empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "[Query] Проверка валидации null параметра query")]
    public async Task Query_WithNullQuery_ShouldThrowStorageException()
    {
        // Arrange
        string? query = null;
        var step = "15s";
        var streamIds = new List<long> { 1, 2, 3 };
        var userspaceId = 100L;

        var client = new VictoriaClientRead(_httpClient, _optionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<StorageException>(async () =>
            await client.Query(query, step, streamIds, userspaceId));

        Assert.Contains("query is null or empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "[Query] Проверка валидации пустого списка streamIds")]
    public async Task Query_WithEmptyStreamIds_ShouldThrowStorageException()
    {
        // Arrange
        var query = "up";
        var step = "15s";
        var streamIds = new List<long>(); // Empty list
        var userspaceId = 100L;

        var client = new VictoriaClientRead(_httpClient, _optionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<StorageException>(async () =>
            await client.Query(query, step, streamIds, userspaceId));

        Assert.Contains("streamIds is empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "[Query] Проверка валидации отрицательного userspaceId")]
    public async Task Query_WithNegativeUserspaceId_ShouldThrowStorageException()
    {
        // Arrange
        var query = "up";
        var step = "15s";
        var streamIds = new List<long> { 1, 2, 3 };
        var userspaceId = -1L;

        var client = new VictoriaClientRead(_httpClient, _optionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<StorageException>(async () =>
            await client.Query(query, step, streamIds, userspaceId));

        Assert.Contains("userspaceId must be greater than zero", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "[Query] Проверка валидации нулевого userspaceId")]
    public async Task Query_WithZeroUserspaceId_ShouldThrowStorageException()
    {
        // Arrange
        var query = "up";
        var step = "15s";
        var streamIds = new List<long> { 1, 2, 3 };
        var userspaceId = 0L;

        var client = new VictoriaClientRead(_httpClient, _optionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<StorageException>(async () =>
            await client.Query(query, step, streamIds, userspaceId));

        Assert.Contains("userspaceId must be greater than zero", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "[QueryRange] Вызов метода с валидными параметрами должен возвращать успешный результат")]
    public async Task QueryRange_WithValidParameters_ShouldReturnSuccessResult()
    {
        // Arrange
        var query = "up";
        var start = DateTimeOffset.UtcNow.AddDays(-1);
        var end = DateTimeOffset.UtcNow;
        var step = new TimeInterval(15, TimeIntervalUnits.Seconds);
        var streamIds = new List<long> { 1, 2, 3 };
        var userspaceId = 100L;

        var expectedResponse = CreateSuccessResponse();
        var jsonContent = JsonSerializer.Serialize(expectedResponse, BaseResponseModelSerializerContext.Default.BaseResponseModel);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            });

        var client = new VictoriaClientRead(_httpClient, _optionsMock.Object);

        // Act
        var result = await client.QueryRange(query, start, end, step, streamIds, userspaceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(QueryResultTypes.matrix, result.ResultType);
        Assert.NotNull(result.Result);
    }

    [Fact(DisplayName = "[QueryRange] Проверка валидации пустого параметра query")]
    public async Task QueryRange_WithEmptyQuery_ShouldThrowStorageException()
    {
        // Arrange
        var query = "";
        var start = DateTimeOffset.UtcNow.AddDays(-1);
        var end = DateTimeOffset.UtcNow;
        var step = new TimeInterval(15, TimeIntervalUnits.Seconds);
        var streamIds = new List<long> { 1, 2, 3 };
        var userspaceId = 100L;

        var client = new VictoriaClientRead(_httpClient, _optionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<StorageException>(async () =>
            await client.QueryRange(query, start, end, step, streamIds, userspaceId));

        Assert.Contains("query is null or empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "[QueryRange] Проверка валидации null параметра query")]
    public async Task QueryRange_WithNullQuery_ShouldThrowStorageException()
    {
        // Arrange
        string? query = null;
        var start = DateTimeOffset.UtcNow.AddDays(-1);
        var end = DateTimeOffset.UtcNow;
        var step = new TimeInterval(15, TimeIntervalUnits.Seconds);
        var streamIds = new List<long> { 1, 2, 3 };
        var userspaceId = 100L;

        var client = new VictoriaClientRead(_httpClient, _optionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<StorageException>(async () =>
            await client.QueryRange(query, start, end, step, streamIds, userspaceId));

        Assert.Contains("query is null or empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "[QueryRange] Проверка валидации пустого списка streamIds")]
    public async Task QueryRange_WithEmptyStreamIds_ShouldThrowStorageException()
    {
        // Arrange
        var query = "up";
        var start = DateTimeOffset.UtcNow.AddDays(-1);
        var end = DateTimeOffset.UtcNow;
        var step = new TimeInterval(15, TimeIntervalUnits.Seconds);
        var streamIds = new List<long>(); // Empty list
        var userspaceId = 100L;

        var client = new VictoriaClientRead(_httpClient, _optionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<StorageException>(async () =>
            await client.QueryRange(query, start, end, step, streamIds, userspaceId));

        Assert.Contains("streamIds is empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "[QueryRange] Проверка валидации отрицательного userspaceId")]
    public async Task QueryRange_WithNegativeUserspaceId_ShouldThrowStorageException()
    {
        // Arrange
        var query = "up";
        var start = DateTimeOffset.UtcNow.AddDays(-1);
        var end = DateTimeOffset.UtcNow;
        var step = new TimeInterval(15, TimeIntervalUnits.Seconds);
        var streamIds = new List<long> { 1, 2, 3 };
        var userspaceId = -1L;

        var client = new VictoriaClientRead(_httpClient, _optionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<StorageException>(async () =>
            await client.QueryRange(query, start, end, step, streamIds, userspaceId));

        Assert.Contains("userspaceId must be greater than zero", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "[QueryRange] Проверка валидации нулевого userspaceId")]
    public async Task QueryRange_WithZeroUserspaceId_ShouldThrowStorageException()
    {
        // Arrange
        var query = "up";
        var start = DateTimeOffset.UtcNow.AddDays(-1);
        var end = DateTimeOffset.UtcNow;
        var step = new TimeInterval(15, TimeIntervalUnits.Seconds);
        var streamIds = new List<long> { 1, 2, 3 };
        var userspaceId = 0L;

        var client = new VictoriaClientRead(_httpClient, _optionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<StorageException>(async () =>
            await client.QueryRange(query, start, end, step, streamIds, userspaceId));

        Assert.Contains("userspaceId must be greater than zero", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "[Query] Успешный ответ от VictoriaMetrics должен обрабатываться корректно")]
    public async Task Query_WithSuccessVictoriaMetricsResponse_ShouldReturnCorrectResult()
    {
        // Arrange
        var query = "up";
        var step = "15s";
        var streamIds = new List<long> { 1, 2, 3 };
        var userspaceId = 100L;

        var expectedResponse = new BaseResponseModel
        {
            Status = PrometheusResponseStatuses.success,
            Data = JsonNode.Parse("""
            {
              "resultType": "matrix",
              "result": [
                {
                  "metric": { "__name__": "up", "job": "prometheus" },
                  "values": [[1565133785.061, "1"], [1565133845.061, "1"]]
                }
              ]
            }
            """)
        };

        var jsonContent = JsonSerializer.Serialize(expectedResponse, BaseResponseModelSerializerContext.Default.BaseResponseModel);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            });

        var client = new VictoriaClientRead(_httpClient, _optionsMock.Object);

        // Act
        var result = await client.Query(query, step, streamIds, userspaceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(QueryResultTypes.matrix, result.ResultType);
        Assert.NotNull(result.Result);
        Assert.Single(result.Result);

        // Verify that the result contains the expected values from the JSON response
        var firstResult = result.Result[0];
        var metric = firstResult["metric"];
        var values = firstResult["values"];

        Assert.NotNull(metric);
        Assert.NotNull(values);
        Assert.Equal("up", metric["__name__"].ToString());
        Assert.Equal("prometheus", metric["job"].ToString());
        Assert.Equal(2, values.AsArray().Count);
    }

    [Fact(DisplayName = "[Query] Проверка содержимого результата ответа VictoriaMetrics")]
    public async Task Query_WithSuccessVictoriaMetricsResponse_ShouldReturnCorrectResultWithExpectedValues()
    {
        // Arrange
        var query = "up";
        var step = "15s";
        var streamIds = new List<long> { 1, 2, 3 };
        var userspaceId = 100L;

        var expectedResponse = new BaseResponseModel
        {
            Status = PrometheusResponseStatuses.success,
            Data = JsonNode.Parse("""
            {
              "resultType": "matrix",
              "result": [
                {
                  "metric": { "__name__": "up", "job": "prometheus" },
                  "values": [[1565133785.061, "1"], [1565133845.061, "1"]]
                }
              ]
            }
            """)
        };

        var jsonContent = JsonSerializer.Serialize(expectedResponse, BaseResponseModelSerializerContext.Default.BaseResponseModel);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            });

        var client = new VictoriaClientRead(_httpClient, _optionsMock.Object);

        // Act
        var result = await client.Query(query, step, streamIds, userspaceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(QueryResultTypes.matrix, result.ResultType);
        Assert.NotNull(result.Result);
        Assert.Single(result.Result);

        // Verify that the result contains the expected values from the JSON response
        var firstResult = result.Result[0];
        var metric = firstResult["metric"];
        var values = firstResult["values"];

        Assert.NotNull(metric);
        Assert.NotNull(values);
        Assert.Equal("up", metric["__name__"].ToString());
        Assert.Equal("prometheus", metric["job"].ToString());
        Assert.Equal(2, values.AsArray().Count);

        // Check the first value
        var firstValue = values[0];
        Assert.Equal("1565133785.061", firstValue[0].ToString());
        Assert.Equal("1", firstValue[1].ToString());

        // Check the second value
        var secondValue = values[1];
        Assert.Equal("1565133845.061", secondValue[0].ToString());
        Assert.Equal("1", secondValue[1].ToString());
    }

    [Fact(DisplayName = "[Query] Ошибка HTTP клиента должна приводить к StorageException")]
    public async Task Query_WhenHttpClientThrowsException_ShouldThrowStorageException()
    {
        // Arrange
        var query = "up";
        var step = "15s";
        var streamIds = new List<long> { 1, 2, 3 };
        var userspaceId = 100L;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        var client = new VictoriaClientRead(_httpClient, _optionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<StorageException>(async () =>
            await client.Query(query, step, streamIds, userspaceId));

        Assert.Contains("Storage threw exception on request", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.IsType<HttpRequestException>(exception.InnerException);
    }

    [Fact(DisplayName = "[Query] Ошибка ответа от VictoriaMetrics должна приводить к StorageException")]
    public async Task Query_WhenVictoriaMetricsReturnsError_ShouldThrowStorageException()
    {
        // Arrange
        var query = "up";
        var step = "15s";
        var streamIds = new List<long> { 1, 2, 3 };
        var userspaceId = 100L;

        var errorResponse = new BaseResponseModel
        {
            Status = PrometheusResponseStatuses.error,
            Error = "Invalid query"
        };

        var jsonContent = JsonSerializer.Serialize(errorResponse, BaseResponseModelSerializerContext.Default.BaseResponseModel);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            });

        var client = new VictoriaClientRead(_httpClient, _optionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<StorageException>(async () =>
            await client.Query(query, step, streamIds, userspaceId));

        Assert.Contains("Storage responded with status Error", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Invalid query", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "[Query] Пустой ответ от VictoriaMetrics должен приводить к StorageException")]
    public async Task Query_WhenVictoriaMetricsReturnsEmptyData_ShouldThrowStorageException()
    {
        // Arrange
        var query = "up";
        var step = "15s";
        var streamIds = new List<long> { 1, 2, 3 };
        var userspaceId = 100L;

        var emptyResponse = new BaseResponseModel
        {
            Status = PrometheusResponseStatuses.success,
            Data = null // No data
        };

        var jsonContent = JsonSerializer.Serialize(emptyResponse, BaseResponseModelSerializerContext.Default.BaseResponseModel);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            });

        var client = new VictoriaClientRead(_httpClient, _optionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<StorageException>(async () =>
            await client.Query(query, step, streamIds, userspaceId));

        Assert.Contains("""Storage responded with empty "data" message.""", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "[Query] Пустое поле data в ответе VictoriaMetrics должно приводить к StorageException")]
    public async Task Query_WhenVictoriaMetricsReturnsEmptyDataField_ShouldThrowStorageException()
    {
        // Arrange
        var query = "up";
        var step = "15s";
        var streamIds = new List<long> { 1, 2, 3 };
        var userspaceId = 100L;

        var emptyDataResponse = new BaseResponseModel
        {
            Status = PrometheusResponseStatuses.success,
            Data = JsonNode.Parse("{}") // Empty data object
        };

        var jsonContent = JsonSerializer.Serialize(emptyDataResponse, BaseResponseModelSerializerContext.Default.BaseResponseModel);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            });

        var client = new VictoriaClientRead(_httpClient, _optionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<StorageException>(async () =>
            await client.Query(query, step, streamIds, userspaceId));

        Assert.Contains($"""Storage "data" field cannot be deserialized. Message:""", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "[Query] Неуспешный HTTP статус должен приводить к StorageException")]
    public async Task Query_WhenHttpStatusCodeIsNotSuccess_ShouldThrowStorageException()
    {
        // Arrange
        var query = "up";
        var step = "15s";
        var streamIds = new List<long> { 1, 2, 3 };
        var userspaceId = 100L;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Internal Server Error")
            });

        var client = new VictoriaClientRead(_httpClient, _optionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<StorageException>(async () =>
            await client.Query(query, step, streamIds, userspaceId));

        Assert.Contains("Storage responded with status code: 500", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Internal Server Error", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "[QueryRange] Успешный ответ от VictoriaMetrics должен обрабатываться корректно")]
    public async Task QueryRange_WithSuccessVictoriaMetricsResponse_ShouldReturnCorrectResult()
    {
        // Arrange
        var query = "up";
        var start = DateTimeOffset.UtcNow.AddDays(-1);
        var end = DateTimeOffset.UtcNow;
        var step = new TimeInterval(15, TimeIntervalUnits.Seconds);
        var streamIds = new List<long> { 1, 2, 3 };
        var userspaceId = 100L;

        var expectedResponse = new BaseResponseModel
        {
            Status = PrometheusResponseStatuses.success,
            Data = JsonNode.Parse("""
            {
              "resultType": "matrix",
              "result": [
                {
                  "metric": { "__name__": "up", "job": "prometheus" },
                  "values": [[1565133785.061, "1"], [1565133845.061, "1"]]
                }
              ]
            }
            """)
        };

        var jsonContent = JsonSerializer.Serialize(expectedResponse, BaseResponseModelSerializerContext.Default.BaseResponseModel);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            });

        var client = new VictoriaClientRead(_httpClient, _optionsMock.Object);

        // Act
        var result = await client.QueryRange(query, start, end, step, streamIds, userspaceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(QueryResultTypes.matrix, result.ResultType);
        Assert.NotNull(result.Result);
        Assert.Single(result.Result);
    }

    [Fact(DisplayName = "[QueryRange] Ошибка HTTP клиента должна приводить к StorageException")]
    public async Task QueryRange_WhenHttpClientThrowsException_ShouldThrowStorageException()
    {
        // Arrange
        var query = "up";
        var start = DateTimeOffset.UtcNow.AddDays(-1);
        var end = DateTimeOffset.UtcNow;
        var step = new TimeInterval(15, TimeIntervalUnits.Seconds);
        var streamIds = new List<long> { 1, 2, 3 };
        var userspaceId = 100L;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        var client = new VictoriaClientRead(_httpClient, _optionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<StorageException>(async () =>
            await client.QueryRange(query, start, end, step, streamIds, userspaceId));

        Assert.Contains("Storage threw exception on request", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.IsType<HttpRequestException>(exception.InnerException);
    }

    [Fact(DisplayName = "[QueryRange] Ошибка ответа от VictoriaMetrics должна приводить к StorageException")]
    public async Task QueryRange_WhenVictoriaMetricsReturnsError_ShouldThrowStorageException()
    {
        // Arrange
        var query = "up";
        var start = DateTimeOffset.UtcNow.AddDays(-1);
        var end = DateTimeOffset.UtcNow;
        var step = new TimeInterval(15, TimeIntervalUnits.Seconds);
        var streamIds = new List<long> { 1, 2, 3 };
        var userspaceId = 100L;

        var errorResponse = new BaseResponseModel
        {
            Status = PrometheusResponseStatuses.error,
            Error = "Invalid query"
        };

        var jsonContent = JsonSerializer.Serialize(errorResponse, BaseResponseModelSerializerContext.Default.BaseResponseModel);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            });

        var client = new VictoriaClientRead(_httpClient, _optionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<StorageException>(async () =>
            await client.QueryRange(query, start, end, step, streamIds, userspaceId));

        Assert.Contains("Storage responded with status Error", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Invalid query", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    static BaseResponseModel CreateSuccessResponse()
        => new()
        {
            Status = PrometheusResponseStatuses.success,
            Data = JsonNode.Parse("""
                {
                  "resultType": "matrix",
                  "result": [
                    {
                      "metric": { "__name__": "up", "job": "prometheus" },
                      "values": [[1565133785.061, "1"], [1565133845.061, "1"]]
                    }
                  ]
                }
                """)
        };
}
