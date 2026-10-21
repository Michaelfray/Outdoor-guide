using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    public class TrailsPageController(PageControllerDependencies dependencies) : BasePageController<TrailsPage, TrailsPageViewModel>(dependencies)
    {
        // Bara sidan än så länge. Det som vyn ska lista räknas ut här i OUT-33.
        protected override TrailsPageViewModel Build(TrailsPage page) => new TrailsPageViewModel { Content = page };
    }
}
