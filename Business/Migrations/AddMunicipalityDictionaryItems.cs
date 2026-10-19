using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Texterna på kommunsidan, på svenska och engelska.</summary>
    public class AddMunicipalityDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Municipality";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Municipality.Trails", "Leder i kommunen", "Trails in the municipality"),
            ("Municipality.Region", "Region", "Region"),
            ("Municipality.NoTrails", "Det finns inga leder här än.", "There are no trails here yet."),
        ];
    }
}
