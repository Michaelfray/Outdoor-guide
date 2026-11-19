using Microsoft.AspNetCore.Mvc;
using OutdoorGuide.Business.Services.Interfaces;
using OutdoorGuide.Models;
using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    /// <summary>Aktivitetssidan, till exempel Vandring: korten för undersidorna och alla leder med betyg (OUT-105).</summary>
    public class ActivityPageController(PageControllerDependencies dependencies, ITrailRatingService trailRatingService)
        : BasePageController<ActivityPage, ActivityPageViewModel>(dependencies)
    {
        /// <summary>Sidan visas bara genom <see cref="ActivityPageAsync"/>, så att betygen kan hämtas asynkront.</summary>
        public override IActionResult Index() => NotFound();

        protected override ActivityPageViewModel Build(ActivityPage page)
        {
            var categories = page.Children<TrailCategory>()?
                .Where(category => category.IsPublished())
                .Select(category => CategoryCard.For(category, page.Activity))
                .ToList() ?? [];

            // Lederna ligger under regioner och kommuner, så sök i hela trädet under Start.
            // Saknar sidan aktivitet visas inga leder, hellre än alla
            List<Trail> trails = string.IsNullOrEmpty(page.Activity)
                ? []
                : page.AncestorOrSelf<Start>()?.Descendants<Trail>()
                    .Where(trail => trail.IsPublished() && trail.Activity == page.Activity)
                    .OrderBy(trail => trail.Name)
                    .ToList() ?? [];

            return new ActivityPageViewModel { Content = page, Categories = categories, Trails = trails };
        }

        /// <summary>Heter som mallen ActivityPage (MVC tar bort "Async"), så Umbraco skickar sidan hit, som i MunicipalityController.</summary>
        public async Task<IActionResult> ActivityPageAsync()
        {
            if (CurrentPage is not ActivityPage page)
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
                // Ett databasfel får inte krascha sidan. Korten visas då utan betyg
                Dependencies.Logger.LogWarning(ex, "Kunde inte läsa betygen för aktivitetssidan {PageKey}", page.Key);
            }

            return PageResult(page, model);
        }
    }
}
