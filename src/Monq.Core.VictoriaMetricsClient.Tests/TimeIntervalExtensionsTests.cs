using Monq.Core.VictoriaMetricsClient.Extensions;
using Monq.Core.VictoriaMetricsClient.Models;

namespace Monq.Core.VictoriaMetricsClient.Tests;

public class TimeIntervalExtensionsTests
{
    [Fact(DisplayName = "[ToSeconds] Преобразование интервала в секундах должно возвращать значение в секундах")]
    public void ToSeconds_WithSecondsInterval_ShouldReturnCorrectValue()
    {
        // Arrange
        var timeInterval = new TimeInterval(30, TimeIntervalUnits.Seconds);

        // Act
        var result = timeInterval.ToSeconds();

        // Assert
        Assert.Equal(30, result);
    }

    [Fact(DisplayName = "[ToSeconds] Преобразование интервала в минутах должно возвращать значение в секундах")]
    public void ToSeconds_WithMinutesInterval_ShouldReturnCorrectValue()
    {
        // Arrange
        var timeInterval = new TimeInterval(5, TimeIntervalUnits.Minutes);

        // Act
        var result = timeInterval.ToSeconds();

        // Assert
        Assert.Equal(300, result); // 5 * 60 = 300
    }

    [Fact(DisplayName = "[ToSeconds] Преобразование интервала в часах должно возвращать значение в секундах")]
    public void ToSeconds_WithHoursInterval_ShouldReturnCorrectValue()
    {
        // Arrange
        var timeInterval = new TimeInterval(2, TimeIntervalUnits.Hours);

        // Act
        var result = timeInterval.ToSeconds();

        // Assert
        Assert.Equal(7200, result); // 2 * 60 * 60 = 7200
    }

    [Fact(DisplayName = "[ToSeconds] Преобразование интервала в днях должно возвращать значение в секундах")]
    public void ToSeconds_WithDaysInterval_ShouldReturnCorrectValue()
    {
        // Arrange
        var timeInterval = new TimeInterval(1, TimeIntervalUnits.Days);

        // Act
        var result = timeInterval.ToSeconds();

        // Assert
        Assert.Equal(86400, result); // 1 * 60 * 60 * 24 = 86400
    }

    [Fact(DisplayName = "[ToSeconds] Преобразование интервала с неизвестным типом единиц должно возвращать значение в секундах")]
    public void ToSeconds_WithUnknownUnitsInterval_ShouldReturnCorrectValue()
    {
        // Arrange
        var timeInterval = new TimeInterval(10, (TimeIntervalUnits)(-1)); // Invalid units

        // Act
        var result = timeInterval.ToSeconds();

        // Assert
        Assert.Equal(10, result); // Should default to seconds (multiplier 1)
    }

    [Fact(DisplayName = "[ToPromQlInterval] Преобразование интервала в секундах должно возвращать строку в формате PromQL")]
    public void ToPromQlInterval_WithSecondsInterval_ShouldReturnCorrectString()
    {
        // Arrange
        var timeInterval = new TimeInterval(30, TimeIntervalUnits.Seconds);

        // Act
        var result = timeInterval.ToPromQlInterval();

        // Assert
        Assert.Equal("30s", result);
    }

    [Fact(DisplayName = "[ToPromQlInterval] Преобразование интервала в минутах должно возвращать строку в формате PromQL")]
    public void ToPromQlInterval_WithMinutesInterval_ShouldReturnCorrectString()
    {
        // Arrange
        var timeInterval = new TimeInterval(5, TimeIntervalUnits.Minutes);

        // Act
        var result = timeInterval.ToPromQlInterval();

        // Assert
        Assert.Equal("5m", result);
    }

    [Fact(DisplayName = "[ToPromQlInterval] Преобразование интервала в часах должно возвращать строку в формате PromQL")]
    public void ToPromQlInterval_WithHoursInterval_ShouldReturnCorrectString()
    {
        // Arrange
        var timeInterval = new TimeInterval(2, TimeIntervalUnits.Hours);

        // Act
        var result = timeInterval.ToPromQlInterval();

        // Assert
        Assert.Equal("2h", result);
    }

    [Fact(DisplayName = "[ToPromQlInterval] Преобразование интервала в днях должно возвращать строку в формате PromQL")]
    public void ToPromQlInterval_WithDaysInterval_ShouldReturnCorrectString()
    {
        // Arrange
        var timeInterval = new TimeInterval(1, TimeIntervalUnits.Days);

        // Act
        var result = timeInterval.ToPromQlInterval();

        // Assert
        Assert.Equal("1d", result);
    }

    [Fact(DisplayName = "[ToPromQlInterval] Преобразование интервала с неизвестным типом единиц должно возвращать строку в формате PromQL")]
    public void ToPromQlInterval_WithUnknownUnitsInterval_ShouldReturnCorrectString()
    {
        // Arrange
        var timeInterval = new TimeInterval(10, (TimeIntervalUnits)(-1)); // Invalid units

        // Act
        var result = timeInterval.ToPromQlInterval();

        // Assert
        Assert.Equal("10m", result); // Should default to minutes
    }

    [Fact(DisplayName = "[ToSeconds] Преобразование интервала с отрицательным значением должно возвращать отрицательное число секунд")]
    public void ToSeconds_WithNegativeValue_ShouldReturnNegativeSeconds()
    {
        // Arrange
        var timeInterval = new TimeInterval(-5, TimeIntervalUnits.Minutes);

        // Act
        var result = timeInterval.ToSeconds();

        // Assert
        Assert.Equal(-300, result); // -5 * 60 = -300
    }

    [Fact(DisplayName = "[ToPromQlInterval] Преобразование интервала с отрицательным значением должно возвращать строку с отрицательным числом")]
    public void ToPromQlInterval_WithNegativeValue_ShouldReturnNegativeString()
    {
        // Arrange
        var timeInterval = new TimeInterval(-10, TimeIntervalUnits.Seconds);

        // Act
        var result = timeInterval.ToPromQlInterval();

        // Assert
        Assert.Equal("-10s", result);
    }

    [Fact(DisplayName = "[ToSeconds] Преобразование интервала с нулевым значением должно возвращать ноль")]
    public void ToSeconds_WithZeroValue_ShouldReturnZero()
    {
        // Arrange
        var timeInterval = new TimeInterval(0, TimeIntervalUnits.Hours);

        // Act
        var result = timeInterval.ToSeconds();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact(DisplayName = "[ToPromQlInterval] Преобразование интервала с нулевым значением должно возвращать строку с нулем")]
    public void ToPromQlInterval_WithZeroValue_ShouldReturnZeroString()
    {
        // Arrange
        var timeInterval = new TimeInterval(0, TimeIntervalUnits.Days);

        // Act
        var result = timeInterval.ToPromQlInterval();

        // Assert
        Assert.Equal("0d", result);
    }

    [Fact(DisplayName = "[ToSeconds] Преобразование интервала с большим значением должно возвращать корректное число секунд")]
    public void ToSeconds_WithLargeValue_ShouldReturnCorrectValue()
    {
        // Arrange
        var timeInterval = new TimeInterval(1000, TimeIntervalUnits.Hours);

        // Act
        var result = timeInterval.ToSeconds();

        // Assert
        Assert.Equal(3600000, result); // 1000 * 60 * 60 = 3600000
    }

    [Fact(DisplayName = "[ToPromQlInterval] Преобразование интервала с большим значением должно возвращать корректную строку")]
    public void ToPromQlInterval_WithLargeValue_ShouldReturnCorrectString()
    {
        // Arrange
        var timeInterval = new TimeInterval(9999, TimeIntervalUnits.Minutes);

        // Act
        var result = timeInterval.ToPromQlInterval();

        // Assert
        Assert.Equal("9999m", result);
    }
}