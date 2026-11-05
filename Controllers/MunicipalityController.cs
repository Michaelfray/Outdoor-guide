using OutdoorGuide.Models;
using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    public class MunicipalityController(PageControllerDependencies dependencies) : BasePageController<Municipality, MunicipalityPageViewModel>(dependencies)
    {
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
    }
}