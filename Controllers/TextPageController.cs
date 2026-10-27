using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    public class TextPageController(PageControllerDependencies dependencies) : BasePageController<TextPage, TextPageViewModel>(dependencies)
    {
         // Textsidan behöver inget mer än sidan själv: vyn visar ingress och brödtext (OUT-58)
        protected override TextPageViewModel Build(TextPage page) => new TextPageViewModel { Content = page };
    }
}
