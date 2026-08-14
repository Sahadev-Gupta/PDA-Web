using WebApplication1.Controllers;

namespace WebApplication1.Tests.Controllers;

public class WeatherForecastControllerTests
{
    [Fact]
    public void Get_ReturnsFiveItems()
    {
        var controller = new WeatherForecastController();

        var result = controller.Get().ToList();

        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void TemperatureF_ComputesFromTemperatureC()
    {
        var model = new WeatherForecast { TemperatureC = 0 };

        Assert.Equal(32, model.TemperatureF);
    }
}
