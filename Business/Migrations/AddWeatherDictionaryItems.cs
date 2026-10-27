using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Texterna för vädret på ledsidan (OUT-40), på svenska och engelska.</summary>
    public class AddWeatherDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Weather";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Weather.Title", "Väder", "Weather"),
            ("Weather.Now", "Just nu", "Right now"),
            ("Weather.Wind", "vind", "wind"),
            ("Weather.Forecast", "Kommande dagar", "Coming days"),
            ("Weather.Unavailable", "Vädret kan inte visas just nu.", "The weather can't be shown right now."),

            // En text per grupp av WMO-koder, se WeatherCodes
            ("Weather.Code.Clear", "Klart", "Clear"),
            ("Weather.Code.MainlyClear", "Mestadels klart", "Mainly clear"),
            ("Weather.Code.PartlyCloudy", "Halvklart", "Partly cloudy"),
            ("Weather.Code.Overcast", "Mulet", "Overcast"),
            ("Weather.Code.Fog", "Dimma", "Fog"),
            ("Weather.Code.Drizzle", "Duggregn", "Drizzle"),
            ("Weather.Code.Rain", "Regn", "Rain"),
            ("Weather.Code.Snow", "Snö", "Snow"),
            ("Weather.Code.Thunderstorm", "Åska", "Thunderstorm"),
            ("Weather.Code.Unknown", "Okänt väder", "Unknown weather"),
        ];
    }
}