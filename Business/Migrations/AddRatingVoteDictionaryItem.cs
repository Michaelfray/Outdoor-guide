using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Ental av Rating.Votes, så att det står "1 röst" och inte "1 röster", på svenska och engelska (OUT-92).</summary>
    public class AddRatingVoteDictionaryItem(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Rating";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Rating.Vote", "röst", "vote"),
        ];
    }
}
