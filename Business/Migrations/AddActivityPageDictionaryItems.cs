using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Texterna på aktivitetssidorna (OUT-59), på svenska och engelska.</summary>
    public class AddActivityPageDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Activity";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Activity.Categories", "Typer av turer", "Types of trips"),
            ("Activity.Trails", "Alla leder", "All trails"),
            ("Activity.NoTrails", "Det finns inga leder för den här aktiviteten än.", "There are no trails for this activity yet."),
        ];
    }
}