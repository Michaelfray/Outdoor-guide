using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    public class TrailCategoryController(PageControllerDependencies dependencies) : BasePageController<TrailCategory, TrailCategoryViewModel>(dependencies)
    {
        protected override TrailCategoryViewModel Build(TrailCategory page) => new TrailCategoryViewModel
        {
            Content = page,

            // Fältet "Trails" ger IPublishedContent. OfType<Trail> ger ModelsBuilders typ, så att ledkortet kan läsa
            // längd och svårighetsgrad, och hoppar över allt som inte är en led
            Trails = page.Trails?.OfType<Trail>().Where(trail => trail.IsPublished()).ToList() ?? [],
        };
    }
}