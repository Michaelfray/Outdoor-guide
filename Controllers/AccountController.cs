using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    /// <summary>
    /// Mitt konto (OUT-39), bara för inloggade. Utloggningen postar till Umbracos egen UmbLoginStatusController.
    /// </summary>
    public class AccountController(PageControllerDependencies dependencies, IMemberManager memberManager)
        : BasePageController<Account, AccountPageViewModel>(dependencies)
    {
        /// <summary>Sidan visas bara genom <see cref="AccountAsync"/>, som först kollar att någon är inloggad.</summary>
        public override IActionResult Index() => NotFound();

        protected override AccountPageViewModel Build(Account page) => new AccountPageViewModel { Content = page };

        /// <summary>
        /// Heter som mallen Account (MVC tar bort "Async" från namnet), så Umbraco skickar sidan hit i stället för till
        /// Index, och medlemmen kan hämtas asynkront. Den som inte är inloggad skickas till inloggningen och kommer
        /// tillbaka hit efteråt.
        /// </summary>
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> AccountAsync()
        {
            if (CurrentPage is not Account page)
            {
                return NotFound();
            }

            var member = await memberManager.GetCurrentMemberAsync();

            if (member == null)
            {
                var loginUrl = page.AncestorOrSelf<Start>()?.FirstChild<Login>()?.Url();

                return loginUrl == null
                    ? NotFound()
                    : Redirect(QueryHelpers.AddQueryString(loginUrl, "returnUrl", page.Url()));
            }

            return PageResult(page, new AccountPageViewModel
            {
                Content = page,
                MemberName = member.Name ?? member.UserName ?? string.Empty,
            });
        }
    }
}