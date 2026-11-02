namespace OutdoorGuide.Models;

/// <summary>Den senaste prognosen från Open-Meteo för en led. Sparas i Umbracos databas, en rad per led.</summary>
public class WeatherCacheEntry
{
	public int Id { get; set; }

	/// <summary>Ledsidan prognosen gäller.</summary>
	public Guid TrailKey { get; set; }

	/// <summary>Svaret som JSON, i samma format som Open-Meteo skickar. Ingen maxlängd, SQLite sparar det som TEXT.</summary>
	public string ForecastJson { get; set; } = string.Empty;

	public DateTime FetchedUtc { get; set; }
}
