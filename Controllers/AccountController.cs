using Microsoft.AspNetCore.Mvc;
using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    /// <summary>
    /// Mitt konto. Bara rubrik, vem som är inloggad och utloggningen än så länge (OUT-37).
    /// Att sidan bara visas för inloggade kommer i OUT-39.
    /// </summary>
    public class AccountController(PageControllerDependencies dependencies)
        : BasePageController<Account, AccountPageViewModel>(dependencies)
    {
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public override IActionResult Index() => base.Index();

        protected override AccountPageViewModel Build(Account page) => new AccountPageViewModel { Content = page };
    }
}