using Microsoft.AspNetCore.Mvc;
using OutdoorGuide.Business.Services.Interfaces;
using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    /// <summary>En undersida, till exempel Dagsturer: lederna som redaktören har valt, med betyg (OUT-105).</summary>
    public class TrailCategoryController(PageControllerDependencies dependencies, ITrailRatingService trailRatingService)
        : BasePageController<TrailCategory, TrailCategoryViewModel>(dependencies)
    {
        /// <summary>Sidan visas bara genom <see cref="TrailCategoryAsync"/>, så att betygen kan hämtas asynkront.</summary>
        public override IActionResult Index() => NotFound();

        protected override TrailCategoryViewModel Build(TrailCategory page) => new TrailCategoryViewModel
        {
            Content = page,

            // Fältet "Trails" ger IPublishedContent. OfType<Trail> ger ModelsBuilders typ, så att ledkortet kan läsa
            // längd och svårighetsgrad, och hoppar över allt som inte är en led
            Trails = page.Trails?.OfType<Trail>().Where(trail => trail.IsPublished()).ToList() ?? [],
        };

        /// <summary>Heter som mallen TrailCategory (MVC tar bort "Async"), så Umbraco skickar sidan hit, som i MunicipalityController.</summary>
        public async Task<IActionResult> TrailCategoryAsync()
        {
            if (CurrentPage is not TrailCategory page)
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
                Dependencies.Logger.LogWarning(ex, "Kunde inte läsa betygen för undersidan {PageKey}", page.Key);
            }

            return PageResult(page, model);
        }
    }
}
