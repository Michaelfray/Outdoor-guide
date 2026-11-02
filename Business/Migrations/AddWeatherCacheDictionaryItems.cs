using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Texterna för när prognosen hämtades och om den kan vara inaktuell (OUT-65), på svenska och engelska.</summary>
    public class AddWeatherCacheDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Weather";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Weather.Updated", "Uppdaterad", "Updated"),
            ("Weather.Stale", "Prognosen kan vara inaktuell.", "The forecast may be out of date."),
        ];
    }
}
