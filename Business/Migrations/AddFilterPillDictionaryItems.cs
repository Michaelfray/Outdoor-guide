using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Texterna i det nya filtret och antalet leder på startsidan, på svenska och engelska.</summary>
    public class AddFilterPillDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Filter";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Filter.All", "Alla", "All"),
            ("Filter.Showing", "Visar {0} av {1} leder", "Showing {0} of {1} trails"),
        ];
    }
}
