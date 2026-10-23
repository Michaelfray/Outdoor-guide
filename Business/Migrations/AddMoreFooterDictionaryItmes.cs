using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Fler texter i sidfoten, på svenska och engelska.</summary>
    public class AddMoreFooterDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Footer";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Footer.Tagline", "Leder för vandring, paddling, cykel och skidor i Stockholms län.", "Trails for hiking, paddling, cycling and skiing in Stockholm County."),
            ("Footer.Links", "Länkar", "Links"),
            ("Footer.WeatherSource", "Väder från", "Weather from"),
        ];
    }
}