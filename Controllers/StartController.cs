using OutdoorGuide.Models;
using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    public class StartController(PageControllerDependencies dependencies) : BasePageController<Start, StartPageViewModel>(dependencies)
    {
        protected override StartPageViewModel Build(Start page)
        {
            var filter = TrailFilter.FromQuery(Request.Query);

            var trails = page.Descendants<Trail>()?.Where(trail => trail.IsPublished()).ToList() ?? [];

            return new StartPageViewModel
            {
                Content = page,
                Filter = filter,
                Trails = filter.Apply(trails).ToList(),
                HasTrails = trails.Count > 0,
                TotalTrails = trails.Count,
            };
        }
    }
}