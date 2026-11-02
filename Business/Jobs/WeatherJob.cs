using Hangfire.Console;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using OutdoorGuide.Business.Jobs.Interfaces;
using OutdoorGuide.Business.Services.Interfaces;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Extensions;

namespace OutdoorGuide.Business.Jobs;

/// <summary>Byggd som MoviesJob i Carls repo, men asynkron. Körs varje timme av ScheduledJobsComposer (OUT-64).</summary>
public class WeatherJob(
	IUmbracoContextFactory umbracoContextFactory,
	IDocumentNavigationQueryService documentNavigationQueryService,
	IWeatherService weatherService,
	IWeatherCacheService weatherCacheService,
	ILogger<WeatherJob> logger) : IWeatherJob
{
	public async Task UpdateForecastsAsync(PerformContext context)
	{
		var trails = GetTrailsWithCoordinates();
		context.WriteLine($"Leder med koordinater: {trails.Count}");

		var succeeded = 0;
		var failed = 0;

		foreach (var trail in trails)
		{
			// GetForecastAsync ger null när API:t inte svarar. Då sparas inget, så att den gamla prognosen ligger kvar
			var forecast = await weatherService.GetForecastAsync(trail.Latitude, trail.Longitude);

			if (forecast is null)
			{
				failed++;
				context.WriteLine($"Ingen prognos för {trail.Name} ({trail.Key})");
				continue;
			}

			// Ett fel på en led ska inte stoppa de andra. SQLite kan till exempel ge timeout om någon sparar i backoffice samtidigt.
			// Felet kastas inte vidare, för då kör Hangfire om hela jobbet. Leden får en ny prognos vid nästa körning
			try
			{
				await weatherCacheService.SaveAsync(trail.Key, forecast);
				succeeded++;
			}
			catch (Exception ex)
			{
				failed++;
				logger.LogError(ex, "Kunde inte spara prognosen för led {TrailKey}.", trail.Key);
				context.WriteLine($"Kunde inte spara prognosen för {trail.Key}: {ex.Message}");
			}
		}

		context.WriteLine($"Klart. Sparade: {succeeded}, misslyckade: {failed}");
	}

	private List<TrailCoordinates> GetTrailsWithCoordinates()
	{
		// Hangfire kör utanför en webbförfrågan, så IUmbracoContextAccessor har ingen context.
		// EnsureUmbracoContext skapar en, som i Carls FindService
		using var contextReference = umbracoContextFactory.EnsureUmbracoContext();

		if (contextReference.UmbracoContext.Content is not { } content)
		{
			return [];
		}

		var startPage = documentNavigationQueryService.TryGetRootKeys(out var rootKeys)
			? rootKeys.Select(key => content.GetById(key)).OfType<Start>().FirstOrDefault()
			: null;

		if (startPage == null)
		{
			return [];
		}

		// Start och Trail varierar per språk, och utan webbförfrågan finns inget aktuellt språk.
		// Då hittar DescendantsOrSelf inga leder, så varje språk anges. Koordinaterna är samma på alla språk.
		// Koordinaterna läses medan contexten finns. Saknas de är båda 0, som i TrailController.
		// Namnet varierar också per språk, så det läses med språket leden hittades på. Utan språk blir det null
		return startPage.Cultures.Keys
			.SelectMany(culture => startPage.DescendantsOrSelf<Trail>(culture).Select(trail => (Trail: trail, Culture: culture)))
			.DistinctBy(x => x.Trail.Key)
			.Where(x => x.Trail.Latitude != 0 || x.Trail.Longitude != 0)
			.Select(x => new TrailCoordinates(x.Trail.Key, x.Trail.Name(x.Culture), (double)x.Trail.Latitude, (double)x.Trail.Longitude))
			.ToList();
	}

	private record TrailCoordinates(Guid Key, string? Name, double Latitude, double Longitude);
}
