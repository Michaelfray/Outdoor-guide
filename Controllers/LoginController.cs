using Microsoft.AspNetCore.Mvc;
using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    /// <summary>
    /// Inloggningssidan (OUT-37). Formuläret postar till Umbracos egen UmbLoginController, som visar sidan igen vid fel.
    /// </summary>
    public class LoginController(PageControllerDependencies dependencies, IMemberManager memberManager)
        : BasePageController<Login, LoginPageViewModel>(dependencies)
    {
        /// <summary>Den som redan är inloggad, till exempel direkt efter inloggningen här, skickas vidare till Mitt konto.</summary>
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public override IActionResult Index()
        {
            if (memberManager.IsLoggedIn()
                && CurrentPage is Login page
                && page.AncestorOrSelf<Start>()?.FirstChild<Account>()?.Url() is string accountUrl)
            {
                return Redirect(accountUrl);
            }

            return base.Index();
        }

        protected override LoginPageViewModel Build(Login page)
        {
            var returnUrl = Request.Query["returnUrl"].ToString();

            // Bara adresser på den egna sajten, så att ingen kan skicka besökaren vidare till en annan sajt
            return new LoginPageViewModel { Content = page, ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null };
        }
    }
}