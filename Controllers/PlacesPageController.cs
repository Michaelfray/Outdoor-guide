using OutdoorGuide.Models;
using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    public class PlacesPageController(PageControllerDependencies dependencies) : BasePageController<PlacesPage, PlacesPageViewModel>(dependencies)
    {
        protected override PlacesPageViewModel Build(PlacesPage page)
        {
            // Regionerna ligger direkt under Start, inte under Platser (beslut 21/10)
            var regions = page.AncestorOrSelf<Start>()?.Children<Region>()?.Where(region => region.IsPublished()).ToList() ?? [];

            return new PlacesPageViewModel
            {
                Content = page,
                Regions = regions,
                MunicipalitiesByRegion = regions.ToDictionary(region => region.Key, MunicipalityCard.For),
            };
        }
    }
}
