using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Ental av Trail.Hours, så att det står "1 timme" och inte "1 timmar", på svenska och engelska (OUT-92).</summary>
    public class AddTrailHourDictionaryItem(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Trail";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Trail.Hour", "timme", "hour"),
        ];
    }
}
