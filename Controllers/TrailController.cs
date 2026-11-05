using Microsoft.AspNetCore.Mvc;
using OutdoorGuide.Business.Services.Interfaces;
using OutdoorGuide.Models;
using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    /// <summary>Ledsidan, med väderprognos från vädercachen (OUT-65), eller från Open-Meteo när cachen saknar leden (OUT-40).</summary>
    public class TrailController(
        PageControllerDependencies dependencies,
        IWeatherService weatherService,
        IWeatherCacheService weatherCacheService,
        ITrailRatingService trailRatingService,
        IMemberManager memberManager,
        TimeProvider timeProvider)
        : BasePageController<Trail, TrailPageViewModel>(dependencies)
    {
        /// <summary>Äldre än så här visas prognosen ändå, men med en text om att den kan vara inaktuell.</summary>
        private static readonly TimeSpan StaleAfter = TimeSpan.FromHours(3);

        /// <summary>Sidan visas bara genom <see cref="TrailAsync"/>, så att prognosen kan hämtas asynkront.</summary>
        public override IActionResult Index() => NotFound();

        protected override TrailPageViewModel Build(Trail page) => new TrailPageViewModel { Content = page };

        /// <summary>Heter som mallen Trail (MVC tar bort "Async"), så Umbraco skickar sidan hit, som i AccountController.</summary>
        public async Task<IActionResult> TrailAsync()
        {
            if (CurrentPage is not Trail page)
            {
                return NotFound();
            }

            // Saknas koordinater är båda 0, och då finns inget väder att hämta
            var hasCoordinates = page.Latitude != 0 || page.Longitude != 0;

            CachedWeatherForecast? cached = null;

            if (hasCoordinates)
            {
                try
                {
                    cached = await weatherCacheService.GetAsync(page.Key);
                }
                catch (Exception ex)
                {
                    // Ett databasfel får inte krascha sidan. Prognosen hämtas i stället direkt från Open-Meteo nedan
                    Dependencies.Logger.LogWarning(ex, "Kunde inte läsa vädercachen för leden {TrailKey}", page.Key);
                }
            }

            if (hasCoordinates && cached is null)
            {
                // En ny led som jobbet inte har hunnit med. Hämta direkt som i OUT-40.
                // Koordinaterna är decimal i Umbraco, men WeatherService tar double.
                // GetForecastAsync ger null i stället för att kasta när API:t inte svarar, så sidan kraschar aldrig
                var fetched = await weatherService.GetForecastAsync((double)page.Latitude, (double)page.Longitude);

                if (fetched is not null)
                {
                    cached = new(fetched, timeProvider.GetUtcNow().UtcDateTime);

                    try
                    {
                        await weatherCacheService.SaveAsync(page.Key, fetched);
                    }
                    catch (Exception ex)
                    {
                        // Prognosen finns ändå att visa, så ett databasfel (till exempel SQLite-timeout) får inte krascha sidan
                        Dependencies.Logger.LogWarning(ex, "Kunde inte spara väderprognosen för leden {TrailKey}", page.Key);
                    }
                }
            }

            TrailRatingSummary rating = new(null, 0, null);
            var isMember = false;

            try
            {
                var member = await memberManager.GetCurrentMemberAsync();
                isMember = member is not null;
                rating = await trailRatingService.GetSummaryAsync(page.Key, member?.Key);
            }
            catch (Exception ex)
            {
                // Ett databasfel får inte krascha sidan. Leden visas då utan betyg
                Dependencies.Logger.LogWarning(ex, "Kunde inte läsa betygen för leden {TrailKey}", page.Key);
            }

            return PageResult(page, new TrailPageViewModel
            {
                Content = page,
                Forecast = cached?.Forecast,
                FetchedUtc = cached?.FetchedUtc,
                IsStale = cached is not null && timeProvider.GetUtcNow().UtcDateTime - cached.FetchedUtc > StaleAfter,
                HasCoordinates = hasCoordinates,
                Rating = rating,
                IsMember = isMember,
            });
        }
    }
}
