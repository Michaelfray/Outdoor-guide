using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Texterna i väderkortet och säsongstips-kortet på startsidan (OUT-103), på svenska och engelska.</summary>
    public class AddStartWeatherDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Start";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Start.WeatherTitle", "Väder i Stockholm", "Weather in Stockholm"),
            ("Start.Today", "Idag", "Today"),
            ("Start.Tomorrow", "Imorgon", "Tomorrow"),
            ("Start.SeasonalEyebrow", "Säsongstips", "Seasonal tips"),
            ("Start.Season.Spring", "Vår", "Spring"),
            ("Start.Season.Summer", "Sommar", "Summer"),
            ("Start.Season.Autumn", "Höst", "Autumn"),
            ("Start.Season.Winter", "Vinter", "Winter"),
        ];
    }
}
