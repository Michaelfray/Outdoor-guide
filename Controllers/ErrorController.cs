using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
	public class ErrorController(PageControllerDependencies dependencies) : BasePageController<Error, ErrorPageViewModel>(dependencies)
	{
		protected override ErrorPageViewModel Build(Error page) => new ErrorPageViewModel { Content = page };
	}
}