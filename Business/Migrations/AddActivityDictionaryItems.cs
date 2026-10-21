using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>
    /// Etiketterna för aktiviteterna på Led, på svenska och engelska (OUT-31). Nycklarna matchar
    /// <see cref="OutdoorGuide.Models.ActivityValues.DictionaryKey"/>.
    /// </summary>
    public class AddActivityDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Trail";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Trail.Activity", "Aktivitet", "Activity"),
            ("Trail.Activity.Hiking", "Vandring", "Hiking"),
            ("Trail.Activity.Paddling", "Paddling", "Paddling"),
            ("Trail.Activity.Cycling", "Cykel", "Cycling"),
            ("Trail.Activity.Winter", "Vinter", "Winter"),
        ];
    }
}
