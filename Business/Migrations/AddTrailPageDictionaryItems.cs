using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>
    /// The headings on the trail page that follow the design, in Swedish and English.
    /// "Coming soon" is shown on the parts that are not built yet (the map and similar trails).
    /// </summary>
    public class AddTrailPageDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Trail";

        // Key, Swedish, English
        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Trail.Breadcrumbs", "Brödsmulor", "Breadcrumbs"),
            ("Trail.About", "Om leden", "About the trail"),
            ("Trail.Facts", "Fakta", "Facts"),
            ("Trail.Map", "Karta", "Map"),
            ("Trail.Similar", "Liknande leder", "Similar trails"),
            ("Trail.ComingSoon", "Kommer snart", "Coming soon"),
        ];
    }
}
