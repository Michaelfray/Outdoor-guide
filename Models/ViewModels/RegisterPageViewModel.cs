using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models.ViewModels;

public class RegisterPageViewModel : PageViewModel<Register>
{
    /// <summary>Dit besökaren skickas när kontot är skapat.</summary>
    public string? AccountPageUrl => StartPage?.FirstChild<Account>()?.Url();

    /// <summary>För länken "Har du redan ett konto? Logga in".</summary>
    public string? LoginPageUrl => StartPage?.FirstChild<Login>()?.Url();

    /// <summary>Minsta antal tecken i lösenordet, från Umbracos inställningar.</summary>
    public int PasswordRequiredLength { get; init; }
}