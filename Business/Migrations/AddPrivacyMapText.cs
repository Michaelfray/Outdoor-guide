using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>
    /// Stycket om cookies med samtycke på Integritet och cookies handlar nu om kartan från Google Maps (OUT-108). Stycket
    /// byts bara om det fortfarande är exakt som <see cref="AddLegalPages"/> skrev det. Har någon ändrat det i backoffice
    /// lämnas det som det är (undantaget under regel 3 i CLAUDE.md).
    /// </summary>
    public class AddPrivacyMapText(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IContentService contentService)
        : UnscopedAsyncMigrationBase(context)
    {
        private const string English = "en-US";

        private static readonly (string Culture, string Old, string New)[] Paragraphs =
        [
            ("sv-SE",
                "<p>Med ditt samtycke kan vi också använda cookies för statistik. Just nu använder sajten inga sådana cookies.</p>",
                "<p>Med ditt samtycke visar vi en karta från Google Maps på varje ledsida. Google sätter då egna cookies, och hur Google använder dem står i <a href=\"https://policies.google.com/privacy?hl=sv\">Googles integritetspolicy</a>. Väljer du bara nödvändiga cookies visas ingen karta, men du kan öppna leden i Google Maps med länken under kartrutan. Sajten använder inga cookies för statistik.</p>"),
            (English,
                "<p>With your consent we may also use cookies for statistics. Right now the site does not use any such cookies.</p>",
                "<p>With your consent we show a map from Google Maps on every trail page. Google then sets its own cookies, and how Google uses them is described in <a href=\"https://policies.google.com/privacy?hl=en\">Google's privacy policy</a>. If you choose only necessary cookies, no map is shown, but you can open the trail in Google Maps with the link under the map box. The site does not use any cookies for statistics.</p>"),
        ];

        protected override Task MigrateAsync()
        {
            using (var scope = scopeProvider.CreateCoreScope())
            {
                var start = contentService.GetRootContent().FirstOrDefault(x => x.ContentType.Alias == AddBaseAndStart.StartTypeAlias);

                // Samma sökning som i AddLegalPages: en textsida under startsidan med det engelska namnet
                var page = start == null ? null : contentService
                    .GetPagedChildren(start.Id, 0, int.MaxValue, out _, propertyAliases: null, filter: null, ordering: null)
                    .FirstOrDefault(x => x.ContentType.Alias == AddMenuPages.TextPageTypeAlias
                        && x.GetCultureName(English) == AddLegalPages.PrivacyPageEnglishName);

                if (page == null)
                {
                    Logger.LogWarning("No {Page} page found, so the text about the map was not added.", AddLegalPages.PrivacyPageEnglishName);
                }
                else
                {
                    UpdatePage(page);
                }

                scope.Complete();
            }

            // Unscoped migrations måste själva säga att de är klara, då sparas planens nya läge
            Context.Complete();
            return Task.CompletedTask;
        }

        private void UpdatePage(IContent page)
        {
            var changed = false;

            foreach (var (culture, oldText, newText) in Paragraphs)
            {
                var description = page.GetValue<string>(AddRegionAndMunicipality.DescriptionAlias, culture);

                if (description == null || description.Contains(newText))
                {
                    continue;
                }

                if (!description.Contains(oldText))
                {
                    Logger.LogWarning("The paragraph about cookies with consent on {Page} ({Culture}) has been changed, so it was left as it is.",
                        AddLegalPages.PrivacyPageEnglishName, culture);
                    continue;
                }

                page.SetValue(AddRegionAndMunicipality.DescriptionAlias, description.Replace(oldText, newText), culture);
                changed = true;
            }

            if (!changed)
            {
                return;
            }

            var saved = contentService.Save(page);

            if (!saved.Success)
            {
                throw new InvalidOperationException($"Could not save the page '{AddLegalPages.PrivacyPageEnglishName}': {saved.Result}");
            }

            var published = contentService.Publish(page, ["*"]);

            if (!published.Success)
            {
                throw new InvalidOperationException($"Could not publish the page '{AddLegalPages.PrivacyPageEnglishName}': {published.Result}");
            }
        }
    }
}