using Microsoft.AspNetCore.Mvc;
using OutdoorGuide.Business.Services.Interfaces;
using OutdoorGuide.Models;
using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    /// <summary>Startsidan: lederna med filter (OUT-75), betygen på korten och sökrutan i toppen (OUT-90).</summary>
    public class StartController(PageControllerDependencies dependencies, ITrailRatingService trailRatingService)
        : BasePageController<Start, StartPageViewModel>(dependencies)
    {
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

            return PageResult(page, model);
        }
    }
}
