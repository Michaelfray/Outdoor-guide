using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Texter som används på många ställen, på svenska och engelska.</summary>
    public class AddCommonDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Common";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Common.Yes", "Ja", "Yes"),
            ("Common.No", "Nej", "No"),
            ("Common.ReadMore", "Läs mer", "Read more"),
            ("Common.Back", "Tillbaka", "Back"),
        ];
    }
}
