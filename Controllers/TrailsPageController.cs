using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    public class TrailsPageController(PageControllerDependencies dependencies) : BasePageController<TrailsPage, TrailsPageViewModel>(dependencies)
    {
        protected override TrailsPageViewModel Build(TrailsPage page) => new TrailsPageViewModel
        {
            Content = page,
            ActivityPages = page.Children<ActivityPage>()?.Where(activity => activity.IsPublished()).ToList() ?? [],
        };
    }
}