namespace OutdoorGuide.Models;

/// <summary>En sparad prognos och när den hämtades, så att ledsidan kan visa "hämtad 08:00".</summary>
public record CachedWeatherForecast(WeatherForecast Forecast, DateTime FetchedUtc);
