using OutdoorGuide.Models;
using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    /// <summary>Sidan Avsluta prenumeration: läser token från <c>?token=</c> i länken i mejlet (OUT-118).</summary>
    public class UnsubscribePageController(PageControllerDependencies dependencies)
        : BasePageController<UnsubscribePage, UnsubscribePageViewModel>(dependencies)
    {
        protected override UnsubscribePageViewModel Build(UnsubscribePage page)
        {
            var token = Request.Query["token"].ToString().Trim();

            return new UnsubscribePageViewModel
            {
                Content = page,
                Token = token.Length <= NewsletterSubscription.UnsubscribeTokenLength ? token : string.Empty,
                Unsubscribed = Request.Query[NewsletterStatus.QueryKey] == NewsletterStatus.Unsubscribed,
            };
        }
    }
}