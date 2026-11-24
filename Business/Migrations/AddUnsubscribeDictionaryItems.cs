using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Texterna i mejlets sidfot och på sidan Avsluta prenumeration (OUT-118), på svenska och engelska.</summary>
    public class AddUnsubscribeDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Newsletter";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Newsletter.UnsubscribeIntro", "Du får det här mejlet eftersom du har anmält dig till nyhetsbrevet från Friluftsguiden.", "You are receiving this email because you signed up for the Friluftsguiden newsletter."),
            ("Newsletter.UnsubscribeLink", "Avsluta prenumerationen", "Unsubscribe"),
            ("Newsletter.UnsubscribeQuestion", "Vill du sluta få nyhetsbrevet? Tryck på knappen, så tar vi bort din e-postadress.", "Do you want to stop getting the newsletter? Press the button and we will delete your email address."),
            ("Newsletter.UnsubscribeButton", "Avsluta prenumerationen", "Unsubscribe"),
            ("Newsletter.Unsubscribed", "Klart! Din e-postadress är borttagen, och du får inga fler nyhetsbrev.", "Done! Your email address has been deleted, and you won't get any more newsletters."),
            ("Newsletter.UnsubscribeTokenMissing", "Länken är inte komplett. Använd länken längst ner i nyhetsbrevet.", "The link is not complete. Use the link at the bottom of the newsletter."),
        ];
    }
}