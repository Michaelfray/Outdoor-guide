using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>
    /// En mening om utskicket och avregistreringen på Integritet och cookies (OUT-118). Stycket om nyhetsbrevet byts bara
    /// om det fortfarande är exakt som <see cref="AddLegalPages"/> skrev det. Har någon ändrat det i backoffice lämnas det
    /// som det är (undantaget under regel 3 i CLAUDE.md).
    /// </summary>
    public class AddPrivacyUnsubscribeText(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IContentService contentService)
        : UnscopedAsyncMigrationBase(context)
    {
        private const string English = "en-US";

        private static readonly (string Culture, string Old, string New)[] Paragraphs =
        [
            ("sv-SE",
                "<p>Vi sparar din e-postadress, bara för att kunna skicka nyhetsbrevet till dig.</p>",
                "<p>Vi sparar din e-postadress och vilket språk du anmälde dig på, bara för att kunna skicka nyhetsbrevet till dig. Längst ner i varje nyhetsbrev finns en länk där du avslutar prenumerationen, och då tas din adress bort.</p>"),
            (English,
                "<p>We save your email address, only to send you the newsletter.</p>",
                "<p>We save your email address and the language you signed up in, only to send you the newsletter. At the bottom of every newsletter there is a link where you can unsubscribe, and your address is then deleted.</p>"),
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
                    Logger.LogWarning("No {Page} page found, so the text about unsubscribing was not added.", AddLegalPages.PrivacyPageEnglishName);
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
                    Logger.LogWarning("The newsletter paragraph on {Page} ({Culture}) has been changed, so it was left as it is.",
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