using Microsoft.AspNetCore.Mvc;
using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    public class RegionController(PageControllerDependencies dependencies) : BasePageController<Region, RegionPageViewModel>(dependencies)
    {
        // GET: RegionController
        protected override RegionPageViewModel Build(Region page) =>  new RegionPageViewModel { Content = page };
    }
}
