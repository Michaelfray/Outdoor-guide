using Microsoft.AspNetCore.Mvc;
using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    public class StartController(PageControllerDependencies dependencies) : BasePageController<Start, StartPageViewModel>(dependencies)
    {
        // GET: StartController
        protected override StartPageViewModel Build(Start page) => new StartPageViewModel{ Content = page };

    }
}
