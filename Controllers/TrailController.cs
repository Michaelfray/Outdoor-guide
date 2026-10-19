using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;


/*
    19/10/2026
    Det här blir sedan stället där vädret kopplas in (“prognos på ledsidan”). TrailPageViewModel får en Forecast, och controllern anropar WeatherService med ledens koordinater. Det är en egen uppgift, efter det här ärendet.
*/
namespace OutdoorGuide.Controllers
{
    public class TrailController(PageControllerDependencies dependencies) : BasePageController<Trail, TrailPageViewModel>(dependencies)
    {
        protected override TrailPageViewModel Build(Trail page) => new TrailPageViewModel { Content = page };
    }
}