using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Texterna för registreringen (OUT-38): tips om lösenordet och felmeddelanden, på svenska och engelska.</summary>
    public class AddMemberRegisterDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Member";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            // {0} ersätts med det minsta antalet tecken från Umbracos inställningar
            ("Member.PasswordHint", "Minst {0} tecken.", "At least {0} characters."),

            // Meddelanden
            ("Member.EmailInvalid", "Skriv en giltig e-postadress.", "Enter a valid email address."),
            ("Member.PasswordMismatch", "Lösenorden är inte likadana.", "The passwords don't match."),
            ("Member.RegisterFailed", "Kontot kunde inte skapas. Kanske finns e-postadressen redan, eller så är lösenordet för kort.", "The account could not be created. The email address may already be in use, or the password may be too short."),
        ];
    }
}