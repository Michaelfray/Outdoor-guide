using Microsoft.AspNetCore.Mvc;
using OutdoorGuide.Business.Services.Interfaces;
using OutdoorGuide.Models;
using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    /// <summary>
    /// Startsidan: lederna med filter (OUT-75), betygen på korten och sökrutan i toppen (OUT-90),
    /// väderkortet för Stockholm och säsongstips-kortet (OUT-103).
    /// </summary>
    public class StartController(
        PageControllerDependencies dependencies,
        ITrailRatingService trailRatingService,
        IWeatherService weatherService,
        IWeatherCacheService weatherCacheService,
        TimeProvider timeProvider)
        : BasePageController<Start, StartPageViewModel>(dependencies)
    {
        // Stockholms centrum (Sergels torg), för väderkortet i toppen
        private const double StockholmLatitude = 59.3326;
        private const double StockholmLongitude = 18.0649;

        /// <summary>
        /// Vädercachen sparar en prognos per Guid. Startsidan har ingen egen led, så Stockholm får en fast nyckel.
        /// Den kan aldrig krocka med en led, eftersom Umbracos nycklar är slumpade.
        /// </summary>
        private static readonly Guid StockholmWeatherKey = new("5f0c6a5e-3c1d-4d1e-9a2f-000000000001");

        /// <summary>Vädret på startsidan hämtas om när det är äldre än så här. Jobbet (OUT-64) uppdaterar bara lederna.</summary>
        private static readonly TimeSpan WeatherMaxAge = TimeSpan.FromHours(1);

        /// <summary>Sidan visas bara genom <see cref="StartAsync"/>, så att betygen kan hämtas asynkront.</summary>
        public override IActionResult Index() => NotFound();

        protected override StartPageViewModel Build(Start page)
        {
            var filter = TrailFilter.FromQuery(Request.Query);

            var trails = page.Descendants<Trail>()?.Where(trail => trail.IsPublished()).ToList() ?? [];

            return new StartPageViewModel
            {
                Content = page,
                Filter = filter,
                Trails = filter.Apply(trails).ToList(),
                HasTrails = trails.Count > 0,
                TotalTrails = trails.Count,
                // Söksidan ligger under startsidan (OUT-74), så den följer språket
                SearchUrl = page.FirstChild<SearchPage>()?.Url(),
                SeasonalTips = FindSeasonalTips(page),
            };
        }

        /// <summary>Heter som mallen Start (MVC tar bort "Async"), så Umbraco skickar sidan hit, som i TrailController.</summary>
        public async Task<IActionResult> StartAsync()
        {
            if (CurrentPage is not Start page)
            {
                return NotFound();
            }

            var model = Build(page);

            try
            {
                model.Ratings = await trailRatingService.GetSummariesAsync(model.Trails.Select(trail => trail.Key).ToList());
            }
            catch (Exception ex)
            {
                // Ett databasfel får inte krascha startsidan. Korten visas då utan betyg
                Dependencies.Logger.LogWarning(ex, "Kunde inte läsa betygen för startsidan");
            }

            model.StockholmWeather = await GetStockholmWeatherAsync();

            return PageResult(page, model);
        }

        /// <summary>
        /// Prognosen för Stockholm från vädercachen. Saknas den, eller är den äldre än en timme, hämtas en ny från Open-Meteo
        /// och sparas. Går det inte att hämta används den gamla, och finns ingen alls visas inget väderkort.
        /// Samma mönster som i TrailController.
        /// </summary>
        private async Task<WeatherForecast?> GetStockholmWeatherAsync()
        {
            CachedWeatherForecast? cached = null;

            try
            {
                cached = await weatherCacheService.GetAsync(StockholmWeatherKey);
            }
            catch (Exception ex)
            {
                // Ett databasfel får inte krascha startsidan. Prognosen hämtas i stället direkt nedan
                Dependencies.Logger.LogWarning(ex, "Kunde inte läsa vädercachen för startsidan");
            }

            var now = timeProvider.GetUtcNow().UtcDateTime;

            if (cached is not null && now - cached.FetchedUtc < WeatherMaxAge)
            {
                return cached.Forecast;
            }

            // GetForecastAsync ger null i stället för att kasta när API:t inte svarar, så sidan kraschar aldrig
            var fetched = await weatherService.GetForecastAsync(StockholmLatitude, StockholmLongitude);

            if (fetched is null)
            {
                return cached?.Forecast;
            }

            try
            {
                await weatherCacheService.SaveAsync(StockholmWeatherKey, fetched);
            }
            catch (Exception ex)
            {
                // Prognosen finns ändå att visa, så ett databasfel (till exempel SQLite-timeout) får inte krascha sidan
                Dependencies.Logger.LogWarning(ex, "Kunde inte spara väderprognosen för startsidan");
            }

            return fetched;
        }

        /// <summary>
        /// Sidan Säsongstips under startsidan, som skapas i AddMenuContent. Den hittas på sitt engelska namn, så att den
        /// hittas på båda språken. Döps den om på engelska visas inget säsongstips-kort.
        /// </summary>
        private static TextPage? FindSeasonalTips(Start page) =>
            page.Children<TextPage>()?.FirstOrDefault(child => child.IsPublished()
                && child.Cultures.Values.Any(culture => string.Equals(culture.Name, "Seasonal tips", StringComparison.OrdinalIgnoreCase)));
    }
}
