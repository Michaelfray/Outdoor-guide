using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Texterna som toppmenyn läser, på svenska och engelska.</summary>
    public class AddMenuDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Menu";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Menu.Label", "Huvudmeny", "Main menu"),
            ("Menu.Toggle", "Visa eller dölj menyn", "Show or hide the menu"),
        ];
    }
}