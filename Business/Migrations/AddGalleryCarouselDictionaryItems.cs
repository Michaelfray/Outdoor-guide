using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>Texterna för skärmläsare i bildkarusellen på ledsidan (OUT-109), på svenska och engelska.</summary>
    public class AddGalleryCarouselDictionaryItems(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : DictionaryItemsMigrationBase(context, scopeProvider, dictionaryItemService, languageService)
    {
        protected override string ParentKey => "Trail";

        protected override IEnumerable<(string Key, string Swedish, string English)> Items =>
        [
            ("Trail.GalleryPrevious", "Föregående bild", "Previous photo"),
            ("Trail.GalleryNext", "Nästa bild", "Next photo"),
            ("Trail.GallerySlide", "Bild {0} av {1}", "Photo {0} of {1}"),
        ];
    }
}
