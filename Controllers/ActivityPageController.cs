using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    public class ActivityPageController(PageControllerDependencies dependencies) : BasePageController<ActivityPage, ActivityPageViewModel>(dependencies)
    {
        // Bara sidan än så länge. Det som vyn ska lista räknas ut här i OUT-33.
        protected override ActivityPageViewModel Build(ActivityPage page) => new ActivityPageViewModel { Content = page };
    }
}
