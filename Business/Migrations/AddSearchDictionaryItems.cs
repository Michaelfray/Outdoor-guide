using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Texterna i sökfältet och sökresultatet, på svenska och engelska.</summary>
    public class AddSearchDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Search";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Search.Label", "Sök", "Search"),
            ("Search.Placeholder", "Sök led eller kommun", "Search trail or municipality"),
            ("Search.NoResults", "Inga träffar", "No results"),
        ];
    }
}
