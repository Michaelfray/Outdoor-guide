using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>
    /// Texterna för medlemssidorna och menyns länkar (OUT-36), på svenska och engelska. Nycklarna får suffix efter
    /// roll (beslut 26/10): …Link för länkar, …Button för knappar och inget suffix för fältetiketter.
    /// </summary>
    public class AddMemberDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Member";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            // Länkar
            ("Member.LoginLink", "Logga in", "Log in"),
            ("Member.AccountLink", "Mitt konto", "My account"),
            ("Member.NoAccountLink", "Inget konto? Skapa ett", "No account? Create one"),
            ("Member.HaveAccountLink", "Har du redan ett konto? Logga in", "Already have an account? Log in"),

            // Knappar
            ("Member.LoginButton", "Logga in", "Log in"),
            ("Member.RegisterButton", "Skapa konto", "Create account"),
            ("Member.LogoutButton", "Logga ut", "Log out"),

            // Fältetiketter
            ("Member.Username", "Användarnamn", "Username"),
            ("Member.Password", "Lösenord", "Password"),
            ("Member.ConfirmPassword", "Bekräfta lösenord", "Confirm password"),
            ("Member.Name", "Namn", "Name"),
            ("Member.Email", "E-post", "Email"),

            // Meddelanden
            ("Member.LoginFailed", "Fel användarnamn eller lösenord, eller så är kontot spärrat.", "Wrong username or password, or the account is locked."),
            ("Member.LoggedInAs", "Inloggad som", "Logged in as"),
        ];
    }
}