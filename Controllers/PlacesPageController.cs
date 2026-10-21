using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    public class PlacesPageController(PageControllerDependencies dependencies) : BasePageController<PlacesPage, PlacesPageViewModel>(dependencies)
    {
        // Bara sidan än så länge. Det som vyn ska lista räknas ut här i OUT-33.
        protected override PlacesPageViewModel Build(PlacesPage page) => new PlacesPageViewModel { Content = page };
    }
}
