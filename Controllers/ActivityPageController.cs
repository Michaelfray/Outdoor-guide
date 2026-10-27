using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    public class ActivityPageController(PageControllerDependencies dependencies) : BasePageController<ActivityPage, ActivityPageViewModel>(dependencies)
    {
        protected override ActivityPageViewModel Build(ActivityPage page)
        {
            var categories = page.Children<TrailCategory>()?.Where(category => category.IsPublished()).ToList() ?? [];

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
    }
}