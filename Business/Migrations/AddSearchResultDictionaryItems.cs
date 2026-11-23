using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Rubriken över träffarna och texten när inget hittas på söksidan (OUT-110), på svenska och engelska.</summary>
    public class AddSearchResultDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Search";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Search.ResultsFor", "{0} träffar för ”{1}”", "{0} results for “{1}”"),
            ("Search.ResultsForOne", "1 träff för ”{0}”", "1 result for “{0}”"),
            ("Search.NoResultsFor", "Inga träffar för ”{0}”", "No results for “{0}”"),
            ("Search.NoResultsHint", "Kolla stavningen, eller sök på en kommun, till exempel Nacka.", "Check the spelling, or search for a municipality, for example Nacka."),
        ];
    }
}