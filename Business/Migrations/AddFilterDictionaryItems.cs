using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Texterna i filtret för listan över leder, på svenska och engelska.</summary>
    public class AddFilterDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Filter";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Filter.Title", "Filtrera", "Filter"),
            ("Filter.AllMunicipalities", "Alla kommuner", "All municipalities"),
            ("Filter.AllDifficulties", "Alla svårighetsgrader", "All difficulty levels"),
            ("Filter.OnlyChildFriendly", "Bara barnvänliga", "Child friendly only"),
        ];
    }
}
