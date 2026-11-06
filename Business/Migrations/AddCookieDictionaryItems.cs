using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Texterna i cookie-rutan (OUT-77), på svenska och engelska.</summary>
    public class AddCookieDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Cookie";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Cookie.Label", "Cookies", "Cookies"),
            ("Cookie.Text", "Vi använder nödvändiga cookies. Med ditt samtycke även cookies för statistik.", "We use necessary cookies. With your consent, we also use cookies for statistics."),
            ("Cookie.NecessaryButton", "Bara nödvändiga", "Only necessary"),
            ("Cookie.AcceptAllButton", "Godkänn alla", "Accept all"),
        ];
    }
}
