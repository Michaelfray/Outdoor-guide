using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Texterna på undersidorna, till exempel Dagsturer (OUT-60), på svenska och engelska.</summary>
    public class AddTrailCategoryDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "TrailCategory";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("TrailCategory.Trails", "Leder", "Trails"),
            ("TrailCategory.NoTrails", "Det finns inga leder här än.", "There are no trails here yet."),
        ];
    }
}