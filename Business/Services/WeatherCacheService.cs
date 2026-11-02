using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using OutdoorGuide.Business.Data;
using OutdoorGuide.Business.Services.Interfaces;
using OutdoorGuide.Models;

namespace OutdoorGuide.Business.Services;

public class WeatherCacheService(IEFCoreScopeProvider<WeatherCacheDbContext> scopeProvider, TimeProvider timeProvider)
	: IWeatherCacheService
{
	public async Task SaveAsync(Guid trailKey, WeatherForecast forecast)
	{
		// WeatherForecast har [JsonPropertyName], så JSON:en får samma format som Open-Meteo skickar
		var json = JsonSerializer.Serialize(forecast);
		var now = timeProvider.GetUtcNow().UtcDateTime;

		// Umbracos scope, eftersom tabellen delar anslutning med Umbraco (shareUmbracoConnection i composern)
		using var scope = scopeProvider.CreateScope();

		await scope.ExecuteWithContextAsync<Task>(async db =>
		{
			// TrailKey är unik, så en befintlig rad uppdateras i stället för att en ny läggs till
			var entry = await db.Entries.FirstOrDefaultAsync(x => x.TrailKey == trailKey);

			if (entry is null)
			{
				db.Entries.Add(new WeatherCacheEntry
				{
					TrailKey = trailKey,
					ForecastJson = json,
					FetchedUtc = now,
				});
			}
			else
			{
				entry.ForecastJson = json;
				entry.FetchedUtc = now;
			}

			await db.SaveChangesAsync();
		});

		scope.Complete();
	}

	public async Task<CachedWeatherForecast?> GetAsync(Guid trailKey)
	{
		using var scope = scopeProvider.CreateScope();

		var entry = await scope.ExecuteWithContextAsync(async db =>
			await db.Entries.AsNoTracking().FirstOrDefaultAsync(x => x.TrailKey == trailKey));

		scope.Complete();

		if (entry is null)
		{
			return null;
		}

		var forecast = JsonSerializer.Deserialize<WeatherForecast>(entry.ForecastJson);

		return forecast is null ? null : new CachedWeatherForecast(forecast, entry.FetchedUtc);
	}
}
