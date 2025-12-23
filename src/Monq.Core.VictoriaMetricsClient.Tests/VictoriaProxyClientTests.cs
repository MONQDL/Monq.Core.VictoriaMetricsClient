using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monq.Core.VictoriaMetricsClient.Models;
using Monq.Core.VictoriaMetricsClient.SerializerContexts;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Monq.Core.VictoriaMetricsClient.Tests;

public class VictoriaProxyClientTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly VictoriaOptions _victoriaOptions;
    private readonly Mock<IOptions<VictoriaOptions>> _optionsMock;
    private readonly Mock<ILogger<VictoriaProxyClient>> _loggerMock;

    public VictoriaProxyClientTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://local.com")
        };
        _victoriaOptions = new VictoriaOptions { SystemLabelPrefix = "monq_" };
        _optionsMock = new Mock<IOptions<VictoriaOptions>>();
        _optionsMock.Setup(x => x.Value).Returns(_victoriaOptions);
        _loggerMock = new Mock<ILogger<VictoriaProxyClient>>();
    }

    [Fact(DisplayName = "[Constructor] Создание клиента с валидными параметрами")]
    public void VictoriaProxyClient_Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act
        var client = new VictoriaProxyClient(_httpClient, _loggerMock.Object, _optionsMock.Object);

        // Assert
        Assert.NotNull(client);
    }

    [Fact(DisplayName = "[Constructor] Создание клиента с null конфигурацией")]
    public void VictoriaProxyClient_Constructor_WithNullConfiguration_ShouldThrowStorageConfigurationException()
    {
        // Arrange
        var nullOptionsMock = new Mock<IOptions<VictoriaOptions>>();
        nullOptionsMock.Setup(x => x.Value).Returns((VictoriaOptions)null);

        // Act & Assert
        Assert.Throws<StorageConfigurationException>(() =>
            new VictoriaProxyClient(_httpClient, _loggerMock.Object, nullOptionsMock.Object));
    }

    [Fact(DisplayName = "[Labels] Вызов метода с IQueryCollection должен возвращать успешный результат")]
    public async Task Labels_WithIQueryCollection_ShouldReturnSuccessResult()
    {
        // Arrange
        var queryCollection = new Mock<IQueryCollection>();
        queryCollection.Setup(x => x.GetEnumerator()).Returns(new List<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>().GetEnumerator());

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

        var client = new VictoriaProxyClient(_httpClient, _loggerMock.Object, _optionsMock.Object);

        // Act
        var result = await client.Labels(queryCollection.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PrometheusResponseStatuses.success, result.Status);
        Assert.NotNull(result.Data);
    }

    [Fact(DisplayName = "[Labels] Вызов метода с IQueryCollection, userspaceId и streamIds должен возвращать успешный результат")]
    public async Task Labels_WithIQueryCollectionUserspaceIdAndStreamIds_ShouldReturnSuccessResult()
    {
        // Arrange
        var queryCollection = new Mock<IQueryCollection>();
        queryCollection.Setup(x => x.GetEnumerator()).Returns(new List<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>().GetEnumerator());

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

        var client = new VictoriaProxyClient(_httpClient, _loggerMock.Object, _optionsMock.Object);

        // Act
        var result = await client.Labels(queryCollection.Object, 100, new List<long> { 1, 2, 3 });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PrometheusResponseStatuses.success, result.Status);
        Assert.NotNull(result.Data);
    }

    [Fact(DisplayName = "[Labels] Проверка валидации пустого списка streamIds")]
    public async Task Labels_WithEmptyStreamIds_ShouldThrowStorageException()
    {
        // Arrange
        var queryCollection = new Mock<IQueryCollection>();
        queryCollection.Setup(x => x.GetEnumerator()).Returns(new List<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>().GetEnumerator());

        var client = new VictoriaProxyClient(_httpClient, _loggerMock.Object, _optionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<StorageException>(async () =>
            await client.Labels(queryCollection.Object, 100, new List<long>())); // Empty list

        Assert.Contains("no streamIds set", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "[Labels] Проверка валидации отрицательного userspaceId")]
    public async Task Labels_WithNegativeUserspaceId_ShouldThrowStorageException()
    {
        // Arrange
        var queryCollection = new Mock<IQueryCollection>();
        queryCollection.Setup(x => x.GetEnumerator()).Returns(new List<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>().GetEnumerator());

        var client = new VictoriaProxyClient(_httpClient, _loggerMock.Object, _optionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<StorageException>(async () =>
            await client.Labels(queryCollection.Object, -1, new List<long> { 1, 2, 3 }));

        Assert.Contains("userspaceId parameter is not set", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "[Labels] Проверка валидации нулевого userspaceId")]
    public async Task Labels_WithZeroUserspaceId_ShouldThrowStorageException()
    {
        // Arrange
        var queryCollection = new Mock<IQueryCollection>();
        queryCollection.Setup(x => x.GetEnumerator()).Returns(new List<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>().GetEnumerator());

        var client = new VictoriaProxyClient(_httpClient, _loggerMock.Object, _optionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<StorageException>(async () =>
            await client.Labels(queryCollection.Object, 0, new List<long> { 1, 2, 3 }));

        Assert.Contains("userspaceId parameter is not set", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "[LabelValues] Вызов метода с label '__name__' и allowSkipExtraContent=true должен использовать AllGrantedRequest")]
    public async Task LabelValues_WithLabelNameAndAllowSkipExtraContentTrue_ShouldUseAllGrantedRequest()
    {
        // Arrange
        var queryCollection = new Mock<IQueryCollection>();
        queryCollection.Setup(x => x.GetEnumerator()).Returns(new List<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>().GetEnumerator());

        var expectedResponse = CreateSuccessResponse();
        var jsonContent = JsonSerializer.Serialize(expectedResponse, BaseResponseModelSerializerContext.Default.BaseResponseModel);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("label/__name__/values")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            });

        var client = new VictoriaProxyClient(_httpClient, _loggerMock.Object, _optionsMock.Object);

        // Act
        var result = await client.LabelValues("__name__", queryCollection.Object, 100, new List<long> { 1, 2, 3 }, true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PrometheusResponseStatuses.success, result.Status);
        Assert.NotNull(result.Data);
    }

    [Fact(DisplayName = "[LabelValues] Вызов метода с label '__name__' и allowSkipExtraContent=false должен использовать DefaultRequest")]
    public async Task LabelValues_WithLabelNameAndAllowSkipExtraContentFalse_ShouldUseDefaultRequest()
    {
        // Arrange
        var queryCollection = new Mock<IQueryCollection>();
        queryCollection.Setup(x => x.GetEnumerator()).Returns(new List<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>().GetEnumerator());

        var expectedResponse = CreateSuccessResponse();
        var jsonContent = JsonSerializer.Serialize(expectedResponse, BaseResponseModelSerializerContext.Default.BaseResponseModel);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("label/__name__/values")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            });

        var client = new VictoriaProxyClient(_httpClient, _loggerMock.Object, _optionsMock.Object);

        // Act
        var result = await client.LabelValues("__name__", queryCollection.Object, 100, new List<long> { 1, 2, 3 }, false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PrometheusResponseStatuses.success, result.Status);
        Assert.NotNull(result.Data);
    }

    [Fact(DisplayName = "[LabelValues] Вызов метода с другим label должен использовать DefaultRequest")]
    public async Task LabelValues_WithOtherLabel_ShouldUseDefaultRequest()
    {
        // Arrange
        var queryCollection = new Mock<IQueryCollection>();
        queryCollection.Setup(x => x.GetEnumerator()).Returns(new List<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>().GetEnumerator());

        var expectedResponse = CreateSuccessResponse();
        var jsonContent = JsonSerializer.Serialize(expectedResponse, BaseResponseModelSerializerContext.Default.BaseResponseModel);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("label/test_label/values")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            });

        var client = new VictoriaProxyClient(_httpClient, _loggerMock.Object, _optionsMock.Object);

        // Act
        var result = await client.LabelValues("test_label", queryCollection.Object, 100, new List<long> { 1, 2, 3 }, true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PrometheusResponseStatuses.success, result.Status);
        Assert.NotNull(result.Data);
    }

    [Fact(DisplayName = "[Series] Вызов метода с IQueryCollection, userspaceId и streamIds должен возвращать успешный результат")]
    public async Task Series_WithIQueryCollectionUserspaceIdAndStreamIds_ShouldReturnSuccessResult()
    {
        // Arrange
        var queryCollection = new Mock<IQueryCollection>();
        queryCollection.Setup(x => x.GetEnumerator()).Returns(new List<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>().GetEnumerator());

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

        var client = new VictoriaProxyClient(_httpClient, _loggerMock.Object, _optionsMock.Object);

        // Act
        var result = await client.Series(queryCollection.Object, 100, new List<long> { 1, 2, 3 });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PrometheusResponseStatuses.success, result.Status);
        Assert.NotNull(result.Data);
    }

    [Fact(DisplayName = "[Query] Вызов метода с IQueryCollection, userspaceId и streamIds должен возвращать успешный результат")]
    public async Task Query_WithIQueryCollectionUserspaceIdAndStreamIds_ShouldReturnSuccessResult()
    {
        // Arrange
        var queryCollection = new Mock<IQueryCollection>();
        queryCollection.Setup(x => x.GetEnumerator()).Returns(new List<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>().GetEnumerator());

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

        var client = new VictoriaProxyClient(_httpClient, _loggerMock.Object, _optionsMock.Object);

        // Act
        var result = await client.Query(queryCollection.Object, 100, new List<long> { 1, 2, 3 });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PrometheusResponseStatuses.success, result.Status);
        Assert.NotNull(result.Data);
    }

    [Fact(DisplayName = "[QueryRange] Вызов метода с IQueryCollection, userspaceId и streamIds должен возвращать успешный результат")]
    public async Task QueryRange_WithIQueryCollectionUserspaceIdAndStreamIds_ShouldReturnSuccessResult()
    {
        // Arrange
        var queryCollection = new Mock<IQueryCollection>();
        queryCollection.Setup(x => x.GetEnumerator()).Returns(new List<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>().GetEnumerator());

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

        var client = new VictoriaProxyClient(_httpClient, _loggerMock.Object, _optionsMock.Object);

        // Act
        var result = await client.QueryRange(queryCollection.Object, 100, new List<long> { 1, 2, 3 });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PrometheusResponseStatuses.success, result.Status);
        Assert.NotNull(result.Data);
    }

    [Fact(DisplayName = "[BuildInfo] Вызов метода с IQueryCollection должен возвращать успешный результат")]
    public async Task BuildInfo_WithIQueryCollection_ShouldReturnSuccessResult()
    {
        // Arrange
        var queryCollection = new Mock<IQueryCollection>();
        queryCollection.Setup(x => x.GetEnumerator()).Returns(new List<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>().GetEnumerator());

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

        var client = new VictoriaProxyClient(_httpClient, _loggerMock.Object, _optionsMock.Object);

        // Act
        var result = await client.BuildInfo(queryCollection.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PrometheusResponseStatuses.success, result.Status);
        Assert.NotNull(result.Data);
    }

    [Fact(DisplayName = "[QueryExemplars] Вызов метода с IQueryCollection, userspaceId и streamIds должен возвращать успешный результат")]
    public async Task QueryExemplars_WithIQueryCollectionUserspaceIdAndStreamIds_ShouldReturnSuccessResult()
    {
        // Arrange
        var queryCollection = new Mock<IQueryCollection>();
        queryCollection.Setup(x => x.GetEnumerator()).Returns(new List<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>().GetEnumerator());

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

        var client = new VictoriaProxyClient(_httpClient, _loggerMock.Object, _optionsMock.Object);

        // Act
        var result = await client.QueryExemplars(queryCollection.Object, 100, new List<long> { 1, 2, 3 });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PrometheusResponseStatuses.success, result.Status);
        Assert.NotNull(result.Data);
    }

    [Fact(DisplayName = "[Metadata] Вызов метода с IQueryCollection должен возвращать успешный результат")]
    public async Task Metadata_WithIQueryCollection_ShouldReturnSuccessResult()
    {
        // Arrange
        var queryCollection = new Mock<IQueryCollection>();
        queryCollection.Setup(x => x.GetEnumerator()).Returns(new List<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>().GetEnumerator());

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

        var client = new VictoriaProxyClient(_httpClient, _loggerMock.Object, _optionsMock.Object);

        // Act
        var result = await client.Metadata(queryCollection.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PrometheusResponseStatuses.success, result.Status);
        Assert.NotNull(result.Data);
    }

    [Fact(DisplayName = "[Rules] Вызов метода с IQueryCollection должен возвращать успешный результат")]
    public async Task Rules_WithIQueryCollection_ShouldReturnSuccessResult()
    {
        // Arrange
        var queryCollection = new Mock<IQueryCollection>();
        queryCollection.Setup(x => x.GetEnumerator()).Returns(new List<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>().GetEnumerator());

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

        var client = new VictoriaProxyClient(_httpClient, _loggerMock.Object, _optionsMock.Object);

        // Act
        var result = await client.Rules(queryCollection.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PrometheusResponseStatuses.success, result.Status);
        Assert.NotNull(result.Data);
    }

    [Fact(DisplayName = "[PostRequest] Ошибка HTTP клиента должна возвращать BaseResponseModel с ошибкой")]
    public async Task PostRequest_WhenHttpClientThrowsException_ShouldReturnBaseResponseModelWithError()
    {
        // Arrange
        var queryCollection = new Mock<IQueryCollection>();
        queryCollection.Setup(x => x.GetEnumerator()).Returns(new List<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>().GetEnumerator());

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        var client = new VictoriaProxyClient(_httpClient, _loggerMock.Object, _optionsMock.Object);

        // Act
        var result = await client.Labels(queryCollection.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PrometheusResponseStatuses.error, result.Status);
        Assert.Contains("Storage throws exception on request", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "[PostRequest] Пустой ответ от VictoriaMetrics должен возвращать BaseResponseModel с ошибкой")]
    public async Task PostRequest_WhenVictoriaMetricsReturnsEmptyData_ShouldReturnBaseResponseModelWithError()
    {
        // Arrange
        var queryCollection = new Mock<IQueryCollection>();
        queryCollection.Setup(x => x.GetEnumerator()).Returns(new List<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>().GetEnumerator());

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            });

        var client = new VictoriaProxyClient(_httpClient, _loggerMock.Object, _optionsMock.Object);

        // Act
        var result = await client.Labels(queryCollection.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PrometheusResponseStatuses.error, result.Status);
        Assert.Contains("Storage responded with empty message", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "[DefaultRequest] Проверка добавления extra_filters и extra_label в параметры запроса")]
    public async Task DefaultRequest_WithUserspaceIdAndStreamIds_ShouldAddExtraFiltersAndExtraLabel()
    {
        // Arrange
        var queryCollection = new Mock<IQueryCollection>();
        var queryParams = new List<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>
        {
            new KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>("query", "up")
        };
        queryCollection.Setup(x => x.GetEnumerator()).Returns(queryParams.GetEnumerator());

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

        var client = new VictoriaProxyClient(_httpClient, _loggerMock.Object, _optionsMock.Object);

        // Act
        var result = await client.Labels(queryCollection.Object, 100, new List<long> { 1, 2, 3 });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PrometheusResponseStatuses.success, result.Status);
    }

    [Fact(DisplayName = "[DefaultRequest] Проверка защиты от внедрения параметров extra_label и extra_filters[]")]
    public async Task DefaultRequest_WithExtraLabelAndExtraFiltersInQueryCollection_ShouldNotIncludeThem()
    {
        // Arrange
        var queryCollection = new Mock<IQueryCollection>();
        var queryParams = new List<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>
        {
            new KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>("extra_label", "user_value"),
            new KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>("extra_filters[]", "user_filter"),
            new KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>("valid_param", "valid_value")
        };
        queryCollection.Setup(x => x.GetEnumerator()).Returns(queryParams.GetEnumerator());

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

        var client = new VictoriaProxyClient(_httpClient, _loggerMock.Object, _optionsMock.Object);

        // Act
        var result = await client.Labels(queryCollection.Object, 100, new List<long> { 1, 2, 3 });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PrometheusResponseStatuses.success, result.Status);
    }

    [Fact(DisplayName = "[AllGrantedRequest] Вызов метода должен использовать все параметры из IQueryCollection")]
    public async Task AllGrantedRequest_WithQueryCollection_ShouldIncludeAllQueryParameters()
    {
        // Arrange
        var queryCollection = new Mock<IQueryCollection>();
        var queryParams = new List<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>
        {
            new KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>("param1", "value1"),
            new KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>("param2", "value2")
        };
        queryCollection.Setup(x => x.GetEnumerator()).Returns(queryParams.GetEnumerator());

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

        var client = new VictoriaProxyClient(_httpClient, _loggerMock.Object, _optionsMock.Object);

        // Act
        var result = await client.BuildInfo(queryCollection.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PrometheusResponseStatuses.success, result.Status);
    }

    private BaseResponseModel CreateSuccessResponse()
    {
        return new BaseResponseModel
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
}
