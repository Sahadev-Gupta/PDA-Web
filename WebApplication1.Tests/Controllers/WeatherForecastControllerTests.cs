using NUnit.Framework;
using WebApplication1.Controllers;

namespace WebApplication1.Tests.Controllers;

public class WeatherForecastControllerTests
{
    [Test]
    public void Get_WhenCalled_ReturnsFiveForecastItems()
    {
        var controller = new WeatherForecastController();

        var result = controller.Get().ToArray();

        Assert.That(result, Has.Length.EqualTo(5));
    }

    [Test]
    public void Get_WhenCalled_ReturnsValidForecastValues()
    {
        var controller = new WeatherForecastController();

        var result = controller.Get().ToArray();

        Assert.That(result.All(item => item.TemperatureC >= -20 && item.TemperatureC < 55), Is.True);
        Assert.That(result.All(item => item.TemperatureF == 32 + (int)(item.TemperatureC / 0.5556)), Is.True);
    }
}
