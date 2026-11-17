using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Länken i cookie-rutan och knappen i sidfoten som visar cookie-rutan igen (OUT-47, OUT-94).</summary>
    public class AddCookieSettingsDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Cookie";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Cookie.ReadMore", "Läs mer om cookies", "Read more about cookies"),
            ("Cookie.Settings", "Cookie-inställningar", "Cookie settings"),
        ];
    }
}
