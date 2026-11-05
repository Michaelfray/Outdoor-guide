using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Texterna på startsidan enligt designen, på svenska och engelska.</summary>
    public class AddStartDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Start";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Start.Eyebrow", "Leder i Stockholms län", "Trails in Stockholm County"),
        ];
    }
}
