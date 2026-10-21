using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    public class TextPageController(PageControllerDependencies dependencies) : BasePageController<TextPage, TextPageViewModel>(dependencies)
    {
        // Bara sidan än så länge. Det som vyn ska lista räknas ut här i OUT-33.
        protected override TextPageViewModel Build(TextPage page) => new TextPageViewModel { Content = page };
    }
}
