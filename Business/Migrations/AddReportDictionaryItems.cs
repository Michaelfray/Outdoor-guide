using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    public class AddReportDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Report";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Report.Title", "Rapportera ledens skick", "Report trail condition"),
            ("Report.Intro", "Har du gått leden nyligen? Berätta hur den ser ut.", "Walked this trail recently? Tell us what it is like."),
            ("Report.Condition", "Skick", "Condition"),
            ("Report.Condition.Choose", "Välj skick", "Choose a condition"),
            ("Report.Condition.Good", "Bra", "Good"),
            ("Report.Condition.Obstacle", "Hinder på leden", "Obstacle on the trail"),
            ("Report.Condition.Closed", "Avstängd", "Closed"),
            ("Report.Comment", "Kommentar", "Comment"),
            ("Report.Name", "Namn", "Name"),
            ("Report.Optional", "frivilligt", "optional"),
            ("Report.Send", "Skicka rapport", "Send report"),
            ("Report.Sent", "Tack! Din rapport har skickats.", "Thank you! Your report has been sent."),
            ("Report.Invalid", "Rapporten skickades inte. Välj skick och skriv en kommentar.", "The report was not sent. Choose a condition and write a comment."),
            ("Report.Expired", "Sidan var öppen för länge, så rapporten skickades inte. Försök igen.", "The page was open for too long, so the report was not sent. Please try again."),
        ];
    }
}