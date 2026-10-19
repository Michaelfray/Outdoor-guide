using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Texterna i sidfoten, på svenska och engelska.</summary>
    public class AddFooterDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Footer";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Footer.Contact", "Kontakt", "Contact"),
        ];
    }
}
