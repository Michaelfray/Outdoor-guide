using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Texterna som visar om en träff på söksidan är en led eller en kommun, på svenska och engelska.</summary>
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
        ];
    }
}
