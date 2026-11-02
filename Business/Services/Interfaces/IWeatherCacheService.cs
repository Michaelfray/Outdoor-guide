using OutdoorGuide.Models;

namespace OutdoorGuide.Business.Services.Interfaces;

public interface IWeatherCacheService
{
	/// <summary>Sparar prognosen för leden. Finns det redan en rad för leden skrivs den över.</summary>
	Task SaveAsync(Guid trailKey, WeatherForecast forecast);

	/// <summary>Den sparade prognosen för leden, eller <c>null</c> om ingen har hämtats än.</summary>
	Task<CachedWeatherForecast?> GetAsync(Guid trailKey);
}
