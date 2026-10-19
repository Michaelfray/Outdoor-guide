using System.Globalization;
using System.Net.Http.Json;
using OutdoorGuide.Business.Services.Interfaces;
using OutdoorGuide.Models;

namespace OutdoorGuide.Business.Services;

/// <summary>Hämtar väder från Open-Meteo. API:t kräver ingen nyckel.</summary>
public class WeatherService(HttpClient httpClient, ILogger<WeatherService> logger) : IWeatherService
{
    public const string ForecastUrl = "https://api.open-meteo.com/v1/forecast";

    public async Task<WeatherForecast?> GetForecastAsync(double latitude, double longitude)
    {
        // InvariantCulture ger 59.27; en svensk dator skulle annars skriva 59,27 och API:t svarar med fel
        var lat = latitude.ToString(CultureInfo.InvariantCulture);
        var lon = longitude.ToString(CultureInfo.InvariantCulture);

        var url = $"{ForecastUrl}?latitude={lat}&longitude={lon}"
                + "&current=temperature_2m,weather_code,wind_speed_10m"
                + "&daily=weather_code,temperature_2m_max,temperature_2m_min"
                + "&timezone=Europe%2FStockholm";

        try
        {
            // Kastar HttpRequestException om svaret inte är 2xx, och TaskCanceledException vid timeout
            return await httpClient.GetFromJsonAsync<WeatherForecast>(url);
        }
        catch (Exception e)
        {
            // Sidan ska fungera även när API:t är nere: null betyder "visa reservtext"
            logger.LogError(e, "Could not get the forecast for {Latitude}, {Longitude}", lat, lon);
        }

        return null;
    }
}
