using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>
    /// Rätt rubrik och meta description på startsidan (OUT-107), i stället för "Min titel" och "My Title" som
    /// <see cref="AddBaseAndStart"/> skrev in när sidan skapades. Fyller också i ingressen om den är tom.
    /// </summary>
    /// <remarks>
    /// Ett innehållssteg (undantaget under regel 3 i CLAUDE.md): ett fält byts bara om det är tomt eller fortfarande har
    /// exakt samma text som AddBaseAndStart skrev. Har någon ändrat texten i backoffice lämnas den som den är.
    /// </remarks>
    public class AddStartPageTitle(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IContentService contentService)
        : UnscopedAsyncMigrationBase(context)
    {
        private const string English = "en-US";
        private const string Swedish = "sv-SE";

        private const string TitleAlias = "title";
        private const string MetaDescriptionAlias = "metaDescription";

        // Fältet, språket, den nya texten och texten från AddBaseAndStart som får bytas ut
        private static readonly (string Alias, string Culture, string Value, string? Placeholder)[] Fields =
        [
            (TitleAlias, English, "Find your next trip into nature", "My Title"),
            (TitleAlias, Swedish, "Hitta din nästa tur i naturen", "Min titel"),
            (MetaDescriptionAlias, English, "Trails for hiking, paddling, cycling and skiing in Stockholm County, with weather and ratings.", "My meta description"),
            (MetaDescriptionAlias, Swedish, "Leder för vandring, paddling, cykel och skidor i Stockholms län, med väder och betyg.", "Min meta beskrivning"),
            (AddRegionAndMunicipality.IntroAlias, English, "Hiking, paddling, cycling and skiing close to the city. Pick a trail, check the weather and head out.", null),
            (AddRegionAndMunicipality.IntroAlias, Swedish, "Vandring, paddling, cykel och skidor nära stan. Välj en led, kolla vädret och ge dig ut.", null),
        ];

        protected override Task MigrateAsync()
        {
            using (var scope = scopeProvider.CreateCoreScope())
            {
                var start = contentService.GetRootContent().FirstOrDefault(x => x.ContentType.Alias == AddBaseAndStart.StartTypeAlias);

                if (start == null)
                {
                    Logger.LogWarning("No start page found, so the start page title was not changed.");
                }
                else
                {
                    UpdateStart(start);
                }

                scope.Complete();
            }

            // Unscoped migrations måste själva säga att de är klara, då sparas planens nya läge
            Context.Complete();
            return Task.CompletedTask;
        }

        private void UpdateStart(IContent start)
        {
            var changed = false;

            foreach (var (alias, culture, value, placeholder) in Fields)
            {
                // Startsidan kan sakna ett fält, till exempel om dokumenttypen har ändrats i backoffice
                if (!start.HasProperty(alias))
                {
                    continue;
                }

                var current = start.GetValue<string>(alias, culture);

                if (string.IsNullOrWhiteSpace(current) || current == placeholder)
                {
                    start.SetValue(alias, value, culture);
                    changed = true;
                }
            }

            if (!changed)
            {
                return;
            }

            var saved = contentService.Save(start);

            if (!saved.Success)
            {
                throw new InvalidOperationException($"Could not save the start page: {saved.Result}");
            }

            var published = contentService.Publish(start, ["*"]);

            if (!published.Success)
            {
                throw new InvalidOperationException($"Could not publish the start page: {published.Result}");
            }
        }
    }
}
