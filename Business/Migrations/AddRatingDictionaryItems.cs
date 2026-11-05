using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Texterna för stjärnbetygen på ledsidan (OUT-79), på svenska och engelska.</summary>
    public class AddRatingDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Rating";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Rating.Title", "Betyg", "Rating"),
            ("Rating.Votes", "röster", "votes"),
            ("Rating.YourScore", "Ditt betyg", "Your rating"),
            ("Rating.LoginLink", "Logga in för att betygsätta", "Log in to rate"),
            ("Rating.SaveButton", "Spara betyg", "Save rating"),
            ("Rating.Saved", "Tack för ditt betyg!", "Thanks for your rating!"),
            ("Rating.Invalid", "Välj mellan 1 och 5 stjärnor.", "Choose between 1 and 5 stars."),
            ("Rating.Expired", "Sidan hade varit öppen för länge. Försök igen.", "The page had been open too long. Please try again."),
            ("Rating.LoginRequired", "Du måste vara inloggad för att betygsätta.", "You need to be logged in to rate."),
            ("Rating.Retry", "Något gick fel. Försök igen.", "Something went wrong. Please try again."),
        ];
    }
}
