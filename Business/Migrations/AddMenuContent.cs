using OutdoorGuide.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>
    /// Menyns sidor under startsidan, på svenska och engelska och publicerade (OUT-32): Leder med de fyra
    /// aktivitetssidorna och deras undersidor, Platser, Säsongstips, Om oss och Kontakt. Väljer också sidorna i nivå 1
    /// i Settings (<c>menuItems</c>), men bara om ingen har valt några sidor där än.
    /// </summary>
    /// <remarks>
    /// Till skillnad från <see cref="AddTestContent"/> körs steget överallt, eftersom sidorna är sajtens struktur och
    /// inte testinnehåll. En sida som redan finns (samma typ och namn under samma förälder) lämnas som den är, så inget
    /// skrivs över. Dokumenttyperna skapas i <see cref="AddMenuPages"/>.
    /// </remarks>
    public class AddMenuContent(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IContentService contentService)
        : UnscopedAsyncMigrationBase(context)
    {
        private const string English = "en-US";
        private const string Swedish = "sv-SE";

        /// <summary>Nivå 1 i menyn, i den ordning de visas (beslut 20/10). Barnen är nivå 2 och 3 i megamenyn.</summary>
        private static readonly PageSpec[] MenuPages =
        [
            new(AddMenuPages.TrailsPageTypeAlias, "Trails", "Leder", Children:
            [
                new(AddMenuPages.ActivityPageTypeAlias, "Hiking", "Vandring", ActivityValues.Hiking,
                [
                    new(AddMenuPages.TrailCategoryTypeAlias, "Day hikes", "Dagsturer"),
                    new(AddMenuPages.TrailCategoryTypeAlias, "Multi-day hikes", "Flerdagsturer"),
                    new(AddMenuPages.TrailCategoryTypeAlias, "Accessible trails", "Tillgängliga leder"),
                ]),
                new(AddMenuPages.ActivityPageTypeAlias, "Paddling", "Paddling", ActivityValues.Paddling,
                [
                    new(AddMenuPages.TrailCategoryTypeAlias, "Lake canoeing", "Kanot i insjöar"),
                    new(AddMenuPages.TrailCategoryTypeAlias, "Archipelago kayaking", "Kajak i skärgården"),
                ]),
                new(AddMenuPages.ActivityPageTypeAlias, "Cycling", "Cykel", ActivityValues.Cycling,
                [
                    new(AddMenuPages.TrailCategoryTypeAlias, "Gravel cycling", "Grusvägscykling"),
                ]),
                new(AddMenuPages.ActivityPageTypeAlias, "Winter", "Vinter", ActivityValues.Winter,
                [
                    new(AddMenuPages.TrailCategoryTypeAlias, "Cross-country skiing", "Längdskidor"),
                    new(AddMenuPages.TrailCategoryTypeAlias, "Snowshoeing", "Snöskor"),
                ]),
            ]),
            new(AddMenuPages.PlacesPageTypeAlias, "Places", "Platser"),
            new(AddMenuPages.TextPageTypeAlias, "Seasonal tips", "Säsongstips"),
            new(AddMenuPages.TextPageTypeAlias, "About us", "Om oss"),
            new(AddMenuPages.TextPageTypeAlias, "Contact", "Kontakt"),
        ];

        protected override Task MigrateAsync()
        {
            using (var scope = scopeProvider.CreateCoreScope())
            {
                var start = contentService.GetRootContent().FirstOrDefault(x => x.ContentType.Alias == AddBaseAndStart.StartTypeAlias);

                if (start == null)
                {
                    Logger.LogWarning("No start page found, so the menu pages were not added.");
                }
                else
                {
                    var topLevel = MenuPages.Select(page => EnsurePageTree(start, page)).ToList();
                    EnsureMenuItems(start, topLevel);
                }

                scope.Complete();
            }

            // Unscoped migrations måste själva säga att de är klara, då sparas planens nya läge
            Context.Complete();
            return Task.CompletedTask;
        }

        /// <summary>Skapar sidan och sedan dess barn, i ordning. Returnerar sidan, ny eller befintlig.</summary>
        private IContent EnsurePageTree(IContent parent, PageSpec spec)
        {
            var page = EnsurePage(parent, spec);

            foreach (var child in spec.Children ?? [])
            {
                EnsurePageTree(page, child);
            }

            return page;
        }

        private IContent EnsurePage(IContent parent, PageSpec spec)
        {
            var existing = contentService
                .GetPagedChildren(parent.Id, 0, int.MaxValue, out _, propertyAliases: null, filter: null, ordering: null)
                .FirstOrDefault(x => x.ContentType.Alias == spec.Alias
                    && (x.GetCultureName(English) == spec.English || x.GetCultureName(Swedish) == spec.Swedish));

            if (existing != null)
            {
                return existing;
            }

            var page = contentService.Create(spec.English, parent.Key, spec.Alias);
            page.SetCultureName(spec.English, English);
            page.SetCultureName(spec.Swedish, Swedish);

            if (spec.Activity != null)
            {
                page.SetValue(AddTrailActivity.ActivityAlias, spec.Activity);
            }

            var saved = contentService.Save(page);

            if (!saved.Success)
            {
                throw new InvalidOperationException($"Could not save the page '{spec.English}': {saved.Result}");
            }

            var published = contentService.Publish(page, ["*"]);

            if (!published.Success)
            {
                throw new InvalidOperationException($"Could not publish the page '{spec.English}': {published.Result}");
            }

            return page;
        }

        /// <summary>
        /// Väljer sidorna i nivå 1 i Settings, så att alla får samma meny direkt (beslut 21/10). Har någon redan valt
        /// sidor där lämnas de som de är.
        /// </summary>
        private void EnsureMenuItems(IContent start, IReadOnlyList<IContent> menuPages)
        {
            var settings = contentService
                .GetPagedChildren(start.Id, 0, int.MaxValue, out _, propertyAliases: null, filter: null, ordering: null)
                .FirstOrDefault(x => x.ContentType.Alias == AddSettingsPage.SettingsTypeAlias);

            if (settings == null)
            {
                Logger.LogWarning("No {Page} page found, so no menu items were chosen.", AddSettingsPage.SettingsPageName);
                return;
            }

            if (!string.IsNullOrWhiteSpace(settings.GetValue<string>(AddSettingsPage.MenuItemsPropertyAlias)))
            {
                return;
            }

            // Väljaren sparar sidorna som en kommaseparerad lista med UDI:er, till exempel umb://document/...
            var value = string.Join(",", menuPages.Select(page => Udi.Create(Constants.UdiEntityType.Document, page.Key).ToString()));
            settings.SetValue(AddSettingsPage.MenuItemsPropertyAlias, value);

            var saved = contentService.Save(settings);

            if (!saved.Success)
            {
                throw new InvalidOperationException($"Could not save the {AddSettingsPage.SettingsPageName} page: {saved.Result}");
            }

            var published = contentService.Publish(settings, ["*"]);

            if (!published.Success)
            {
                throw new InvalidOperationException($"Could not publish the {AddSettingsPage.SettingsPageName} page: {published.Result}");
            }
        }

        /// <summary>En sida i menyn: dokumenttyp, namn på engelska och svenska, aktivitet och barn.</summary>
        private sealed record PageSpec(string Alias, string English, string Swedish, string? Activity = null, PageSpec[]? Children = null);
    }
}
