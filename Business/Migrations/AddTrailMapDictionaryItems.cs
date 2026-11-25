using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Orden för kartan på ledsidan och rutan som visas tills besökaren har godkänt alla cookies (OUT-108).</summary>
    public class AddTrailMapDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Trail";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Trail.MapTitle", "Karta över {0}", "Map of {0}"),
            ("Trail.MapConsentText", "Kartan kommer från Google Maps, som sätter cookies. Du måste godkänna alla cookies för att kunna visa den.", "The map comes from Google Maps, which sets cookies. You need to accept all cookies to show it."),
            ("Trail.MapConsentButton", "Ändra cookie-inställningar", "Change cookie settings"),
            ("Trail.MapLoadText", "Kartan kommer från Google Maps. Tryck på knappen för att visa den.", "The map comes from Google Maps. Press the button to load it."),
            ("Trail.MapLoadButton", "Visa kartan", "Show map"),
            ("Trail.MapOpenLink", "Öppna i Google Maps", "Open in Google Maps"),
        ];
    }
}