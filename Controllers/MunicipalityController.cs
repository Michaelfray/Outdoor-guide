using Microsoft.AspNetCore.Mvc;
using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    public class MunicipalityController(PageControllerDependencies dependencies) : BasePageController<Municipality, MunicipalityPageViewModel>(dependencies)
    {
        // GET: MunicipalityController
        protected override MunicipalityPageViewModel Build(Municipality page) => new MunicipalityPageViewModel { Content = page };
    }
}
