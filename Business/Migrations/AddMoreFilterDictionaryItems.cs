using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    public class AddMoreFilterDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Filter";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Filter.AllActivities", "Alla aktiviteter", "All activities"),
            ("Filter.AllLengths", "Alla längder", "All lengths"),
            ("Filter.Length.Short", "Kort (under 5 km)", "Short (under 5 km)"),
            ("Filter.Length.Medium", "Mellan (5–15 km)", "Medium (5–15 km)"),
            ("Filter.Length.Long", "Lång (över 15 km)", "Long (over 15 km)"),
            ("Filter.Submit", "Visa leder", "Show trails"),
            ("Filter.Clear", "Rensa filtret", "Clear filter"),
            ("Filter.NoResults", "Inga leder matchar filtret.", "No trails match the filter."),
        ];
    }
}