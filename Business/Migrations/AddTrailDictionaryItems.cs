using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Texterna på ledsidan, på svenska och engelska.</summary>
    public class AddTrailDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Trail";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Trail.Length", "Längd", "Length"),
            ("Trail.Time", "Tid", "Time"),
            ("Trail.Hours", "timmar", "hours"),
            ("Trail.Difficulty", "Svårighetsgrad", "Difficulty"),
            ("Trail.Difficulty.Easy", "Lätt", "Easy"),
            ("Trail.Difficulty.Medium", "Medel", "Moderate"),
            ("Trail.Difficulty.Hard", "Svår", "Hard"),
            ("Trail.ChildFriendly", "Barnvänlig", "Child friendly"),
            ("Trail.Loop", "Rundslinga", "Loop trail"),
            ("Trail.StartPoint", "Startpunkt", "Starting point"),
            ("Trail.ShowOnMap", "Visa på karta", "Show on map"),
            ("Trail.Municipality", "Kommun", "Municipality"),
        ];
    }
}
