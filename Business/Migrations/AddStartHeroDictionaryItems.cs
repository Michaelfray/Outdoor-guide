using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Fotografens namn under bilden i toppen på startsidan (OUT-107), på svenska och engelska.</summary>
    public class AddStartHeroDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Start";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Start.PhotoCredit", "Foto", "Photo"),
        ];
    }
}
