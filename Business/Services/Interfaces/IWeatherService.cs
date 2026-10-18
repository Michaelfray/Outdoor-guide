using OutdoorGuide.Models;

namespace OutdoorGuide.Business.Services.Interfaces;

public interface IWeatherService
{
    Task<WeatherForecast?> GetForecastAsync(double latitude, double longitude);
}
