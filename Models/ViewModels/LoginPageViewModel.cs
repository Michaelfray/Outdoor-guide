using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models.ViewModels;

public class LoginPageViewModel : PageViewModel<Login>
{
    /// <summary>
    /// Den lokala sidan besökaren ska tillbaka till efter inloggningen, från <c>?returnUrl=</c>. Utan den stannar
    /// besökaren på inloggningssidan, som skickar vidare till Mitt konto.
    /// </summary>
    public string? ReturnUrl { get; init; }

    /// <summary>Sidan Skapa konto, för länken under formuläret.</summary>
    public string? RegisterPageUrl => StartPage?.FirstChild<Register>()?.Url();
}