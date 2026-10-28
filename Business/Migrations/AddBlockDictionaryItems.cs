using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>
    /// The headings for the blocks on the trail page (OUT-48), in Swedish and English.
    /// The views show these headings, so the editor only writes the text in each block.
    /// </summary>
    public class AddBlockDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Trail";

        // Key, Swedish, English
        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Trail.Warning", "Varning", "Warning"),
            ("Trail.Gallery", "Bilder från leden", "Photos from the trail"),
            ("Trail.GoodToKnow", "Bra att veta", "Good to know"),
        ];
    }
}