using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    /// <summary>
    /// Sidan Skapa konto (OUT-38). Formuläret postar till Umbracos egen UmbRegisterController, som skapar medlemmen,
    /// loggar in den och visar sidan igen vid fel.
    /// </summary>
    public class RegisterController(
        PageControllerDependencies dependencies,
        IMemberManager memberManager,
        IOptions<MemberPasswordConfigurationSettings> passwordSettings)
        : BasePageController<Register, RegisterPageViewModel>(dependencies)
    {
        /// <summary>Den som redan är inloggad skickas till Mitt konto, som i LoginController.</summary>
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public override IActionResult Index()
        {
            if (memberManager.IsLoggedIn()
                && CurrentPage is Register page
                && page.AncestorOrSelf<Start>()?.FirstChild<Account>()?.Url() is string accountUrl)
            {
                return Redirect(accountUrl);
            }

            return base.Index();
        }

        protected override RegisterPageViewModel Build(Register page) => new RegisterPageViewModel
        {
            Content = page,
            PasswordRequiredLength = passwordSettings.Value.RequiredLength,
        };
    }
}