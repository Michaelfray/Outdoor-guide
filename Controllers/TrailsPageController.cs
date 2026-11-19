using OutdoorGuide.Models;
using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    /// <summary>Leder: ett kort för varje aktivitet, med antal leder och en bild från en av dem (OUT-105).</summary>
    public class TrailsPageController(PageControllerDependencies dependencies) : BasePageController<TrailsPage, TrailsPageViewModel>(dependencies)
    {
        protected override TrailsPageViewModel Build(TrailsPage page)
        {
            // Lederna ligger under regioner och kommuner, så sök i hela trädet under Start
            var trails = page.AncestorOrSelf<Start>()?.Descendants<Trail>()
                .Where(trail => trail.IsPublished())
                .ToList() ?? [];

            return new TrailsPageViewModel
            {
                Content = page,
                Activities = page.Children<ActivityPage>()?
                    .Where(activity => activity.IsPublished())
                    .Select(activity => CategoryCard.For(activity, trails))
                    .ToList() ?? [],
            };
        }
    }
}
