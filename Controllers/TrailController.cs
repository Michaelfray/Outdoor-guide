using Microsoft.AspNetCore.Mvc;
using OutdoorGuide.Business.Services.Interfaces;
using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    /// <summary>Ledsidan, med väderprognos från Open-Meteo vid ledens koordinater (OUT-40).</summary>
    public class TrailController(PageControllerDependencies dependencies, IWeatherService weatherService)
        : BasePageController<Trail, TrailPageViewModel>(dependencies)
    {
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

            // Koordinaterna är decimal i Umbraco, men WeatherService tar double.
            // GetForecastAsync ger null i stället för att kasta när API:t inte svarar, så sidan kraschar aldrig
            var forecast = hasCoordinates
                ? await weatherService.GetForecastAsync((double)page.Latitude, (double)page.Longitude)
                : null;

            return PageResult(page, new TrailPageViewModel
            {
                Content = page,
                Forecast = forecast,
                HasCoordinates = hasCoordinates,
            });
        }
    }
}