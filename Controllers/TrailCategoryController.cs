using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    public class TrailCategoryController(PageControllerDependencies dependencies) : BasePageController<TrailCategory, TrailCategoryViewModel>(dependencies)
    {
        // Bara sidan än så länge. Det som vyn ska lista räknas ut här i OUT-33.
        protected override TrailCategoryViewModel Build(TrailCategory page) => new TrailCategoryViewModel { Content = page };
    }
}
