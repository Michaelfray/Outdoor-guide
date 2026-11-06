using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Sökknappen och texterna som visar om en träff är en led eller en kommun, på svenska och engelska.</summary>
    public class AddSearchHitDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Search";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Search.Trail", "Led", "Trail"),
            ("Search.Municipality", "Kommun", "Municipality"),
            ("Search.SearchButton", "Sök", "Search"),
        ];
    }
}
