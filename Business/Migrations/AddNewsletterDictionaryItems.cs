using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    public class AddNewsletterDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Newsletter";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Newsletter.Title", "Nyhetsbrev", "Newsletter"),
            ("Newsletter.Intro", "Få tips om nya leder och säsongens bästa turer.", "Get tips on new trails and the best walks of the season."),
            ("Newsletter.Email", "E-postadress", "Email address"),
            ("Newsletter.Send", "Anmäl mig", "Sign me up"),
            ("Newsletter.Sent", "Tack! Du är anmäld till nyhetsbrevet.", "Thank you! You are signed up for the newsletter."),
            ("Newsletter.Invalid", "Anmälan gick inte igenom. Skriv en giltig e-postadress.", "The sign-up did not go through. Enter a valid email address."),
            ("Newsletter.Expired", "Sidan var öppen för länge, så anmälan gick inte igenom. Försök igen.", "The page was open for too long, so the sign-up did not go through. Please try again."),
        ];
    }
}