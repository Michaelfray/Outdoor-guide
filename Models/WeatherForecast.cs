using System.Text.Json.Serialization;

namespace OutdoorGuide.Models;

/// <summary>Svaret från Open-Meteo (/v1/forecast). Bara fälten vi ber om i URL:en finns med.</summary>
public class WeatherForecast
{
    [JsonPropertyName("current")]
    public CurrentWeather? Current { get; set; }

    [JsonPropertyName("daily")]
    public DailyWeather? Daily { get; set; }
}

/// <summary>Vädret just nu.</summary>
public class CurrentWeather
{
    [JsonPropertyName("time")]
    public string Time { get; set; } = string.Empty;

    /// <summary>Grader Celsius.</summary>
    [JsonPropertyName("temperature_2m")]
    public double Temperature { get; set; }

    /// <summary>WMO-kod, till exempel 0 = klart, 61 = lätt regn.</summary>
    [JsonPropertyName("weather_code")]
    public int WeatherCode { get; set; }

    /// <summary>km/h.</summary>
    [JsonPropertyName("wind_speed_10m")]
    public double WindSpeed { get; set; }
}

/// <summary>
/// Prognos per dag. Open-Meteo skickar en lista per fält, inte en lista med dagar:
/// Time[0], WeatherCode[0], TemperatureMax[0] och TemperatureMin[0] hör till samma dag.
/// </summary>
public class DailyWeather
{
    [JsonPropertyName("time")]
    public List<string> Time { get; set; } = [];

    [JsonPropertyName("weather_code")]
    public List<int> WeatherCode { get; set; } = [];

    [JsonPropertyName("temperature_2m_max")]
    public List<double> TemperatureMax { get; set; } = [];

    [JsonPropertyName("temperature_2m_min")]
    public List<double> TemperatureMin { get; set; } = [];
}
