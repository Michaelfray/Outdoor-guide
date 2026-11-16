using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Texterna på Platser, regionsidan och kommunsidan (OUT-101), på svenska och engelska.</summary>
    public class AddPlacesDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Places";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Places.TrailCountOne", "1 led", "1 trail"),
            ("Places.TrailCount", "{0} leder", "{0} trails"),
            ("Places.TrailsInOne", "1 led i {0}", "1 trail in {0}"),
            ("Places.TrailsIn", "{0} leder i {1}", "{0} trails in {1}"),
            ("Places.Municipalities", "Kommuner", "Municipalities"),
            ("Places.NoMunicipalities", "Det finns inga kommuner här än.", "There are no municipalities here yet."),
        ];
    }
}
