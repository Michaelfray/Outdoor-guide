using Microsoft.AspNetCore.Mvc;
using OutdoorGuide.Business.Services.Interfaces;
using OutdoorGuide.Models;
using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    /// <summary>Kommunsidan: lederna med filter och betyg på korten (OUT-101).</summary>
    public class MunicipalityController(PageControllerDependencies dependencies, ITrailRatingService trailRatingService)
        : BasePageController<Municipality, MunicipalityPageViewModel>(dependencies)
    {
        /// <summary>Sidan visas bara genom <see cref="MunicipalityAsync"/>, så att betygen kan hämtas asynkront.</summary>
        public override IActionResult Index() => NotFound();

        protected override MunicipalityPageViewModel Build(Municipality page)
        {
            var filter = TrailFilter.FromQuery(Request.Query);
            var trails = page.Children<Trail>()?.Where(trail => trail.IsPublished()).ToList() ?? [];

            return new MunicipalityPageViewModel
            {
                Content = page,
                Filter = filter,
                Trails = filter.Apply(trails).ToList(),
                HasTrails = trails.Count > 0,
            };
        }

        /// <summary>Heter som mallen Municipality (MVC tar bort "Async"), så Umbraco skickar sidan hit, som i StartController.</summary>
        public async Task<IActionResult> MunicipalityAsync()
        {
            if (CurrentPage is not Municipality page)
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
                Dependencies.Logger.LogWarning(ex, "Kunde inte läsa betygen för kommunen {MunicipalityKey}", page.Key);
            }

            return PageResult(page, model);
        }
    }
}
