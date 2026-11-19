using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>
    /// Fyller undersidorna under Leder (OUT-105): väljer leder på varje undersida, till exempel Dagsturer och Längdskidor,
    /// och skriver en kort ingress på Leder, aktivitetssidorna och undersidorna. Lederna kommer från Data/Innehall/leder.csv
    /// (AddSiteContent och OUT-104).
    /// </summary>
    /// <remarks>
    /// Ett innehållssteg (undantaget under regel 3 i CLAUDE.md): bara tomma fält fylls i. Har någon redan valt leder
    /// eller skrivit en ingress i backoffice lämnas det som det är. Sidorna hittas på sitt engelska eller svenska namn,
    /// som i AddMenuContent, och lederna på sitt namn bland alla leder under startsidan. En led som saknas hoppas över.
    /// </remarks>
    public class AddCategoryTrails(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IContentService contentService)
        : UnscopedAsyncMigrationBase(context)
    {
        private const string English = "en-US";
        private const string Swedish = "sv-SE";

        private static readonly string[] HikingTrails =
        [
            "Around Lake Källtorpssjön", "Around Lake Ältasjön", "Nyckelviken nature trail", "Erstavik coastal trail",
            "Velamsund loop", "Tyresta old-growth forest loop", "Around Lake Stensjön", "Alby loop",
            "Tyresö Castle and Notholmen", "Around Lake Gömmaren", "Flottsbro loop", "Paradiset in Hanveden",
        ];

        /// <summary>De lätta och barnvänliga vandringslederna. Nyckelviken först, så att kortet inte får samma bild som Dagsturer</summary>
        private static readonly string[] AccessibleTrails =
        [
            "Nyckelviken nature trail", "Around Lake Källtorpssjön", "Velamsund loop", "Alby loop",
            "Tyresö Castle and Notholmen", "Flottsbro loop",
        ];

        private static readonly PageText TrailsPage = new("Trails", "Leder",
            "Find a trail for your next trip, on foot, by canoe, by bike or on skis.",
            "Hitta en led till nästa tur, till fots, med kanot, på cykel eller på skidor.");

        private static readonly ActivityText[] Activities =
        [
            new(new("Hiking", "Vandring",
                "Walks in forests, along lakes and by the sea, from short loops to full-day hikes.",
                "Promenader i skog, längs sjöar och vid havet, från korta rundor till heldagsturer."),
            [
                new(new("Day hikes", "Dagsturer",
                    "Trails you walk in a day, with a packed lunch and home for dinner.",
                    "Leder som du går på en dag, med matsäck och hemma till middag."), HikingTrails),
                new(new("Multi-day hikes", "Flerdagsturer",
                    "Longer trails with a night in a tent or a shelter along the way.",
                    "Längre leder med en natt i tält eller vindskydd på vägen."), []),
                new(new("Accessible trails", "Tillgängliga leder",
                    "Easy, wide paths that suit children, prams and anyone who wants to take it slow.",
                    "Lätta, breda stigar som passar barn, barnvagnar och alla som vill ta det lugnt."), AccessibleTrails),
            ]),
            new(new("Paddling", "Paddling",
                "Canoe on calm lakes or kayak out into the archipelago.",
                "Paddla kanot på lugna sjöar eller kajak ut i skärgården."),
            [
                new(new("Lake canoeing", "Kanot i insjöar",
                    "Calm water close to the city, with places to rent a canoe.",
                    "Lugnt vatten nära stan, med ställen där du kan hyra kanot."), ["Paddling on Lake Källtorpssjön"]),
                new(new("Archipelago kayaking", "Kajak i skärgården",
                    "Bays, islands and rocky shores for those who have paddled before.",
                    "Vikar, öar och klippor för dig som har paddlat förut."), ["Kayaking in Kalvfjärden"]),
            ]),
            new(new("Cycling", "Cykel",
                "Gravel roads and forest tracks through the nature reserves.",
                "Grusvägar och skogsvägar genom naturreservaten."),
            [
                new(new("Gravel cycling", "Grusvägscykling",
                    "Loops on gravel roads, on a gravel bike or a sturdy hybrid.",
                    "Rundor på grusväg, med grusracer eller en stadig hybridcykel."),
                    ["Gravel roads in the Nacka reserve", "Hanveden by gravel"]),
            ]),
            new(new("Winter", "Vinter",
                "Ski tracks and snowy forests when the winter comes.",
                "Skidspår och snöig skog när vintern kommer."),
            [
                new(new("Cross-country skiing", "Längdskidor",
                    "Prepared tracks when there is enough snow. Check the conditions before you go.",
                    "Preparerade spår när det finns snö. Kolla läget innan du åker."), ["Ski tracks at Hellasgården"]),
                new(new("Snowshoeing", "Snöskor",
                    "Walk where there are no tracks, through deep snow in the forest.",
                    "Gå där det inte finns några spår, genom djup snö i skogen."), ["Snowshoeing in Tyresta"]),
            ]),
        ];

        protected override Task MigrateAsync()
        {
            using (var scope = scopeProvider.CreateCoreScope())
            {
                var start = contentService.GetRootContent().FirstOrDefault(x => x.ContentType.Alias == AddBaseAndStart.StartTypeAlias);
                var trailsPage = start == null ? null : FindChild(start, AddMenuPages.TrailsPageTypeAlias, TrailsPage);

                if (start == null || trailsPage == null)
                {
                    Logger.LogWarning("No start page or Trails page found, so the trail categories were not filled in.");
                }
                else
                {
                    var trails = FindTrails(start);

                    FillIn(trailsPage, TrailsPage, null);

                    foreach (var activity in Activities)
                    {
                        var activityPage = FindChild(trailsPage, AddMenuPages.ActivityPageTypeAlias, activity.Text);

                        if (activityPage == null)
                        {
                            Logger.LogWarning("The activity page {Page} was not found.", activity.Text.EnglishName);
                            continue;
                        }

                        FillIn(activityPage, activity.Text, null);

                        foreach (var category in activity.Categories)
                        {
                            var categoryPage = FindChild(activityPage, AddMenuPages.TrailCategoryTypeAlias, category.Text);

                            if (categoryPage == null)
                            {
                                Logger.LogWarning("The trail category {Page} was not found.", category.Text.EnglishName);
                                continue;
                            }

                            var picked = category.TrailNames
                                .Select(name => trails.GetValueOrDefault(name))
                                .OfType<IContent>()
                                .ToList();

                            FillIn(categoryPage, category.Text, picked);
                        }
                    }
                }

                scope.Complete();
            }

            // Unscoped migrations måste själva säga att de är klara, då sparas planens nya läge
            Context.Complete();
            return Task.CompletedTask;
        }

        /// <summary>Alla leder under startsidan, efter engelskt namn. Lederna ligger under regioner och kommuner.</summary>
        private Dictionary<string, IContent> FindTrails(IContent start)
        {
            var trails = new Dictionary<string, IContent>();
            var queue = new Queue<IContent>([start]);

            // Går igenom trädet nivå för nivå med samma anrop som FindChild
            while (queue.Count > 0)
            {
                foreach (var child in Children(queue.Dequeue()))
                {
                    if (child.ContentType.Alias == AddTrail.TrailTypeAlias)
                    {
                        if (child.GetCultureName(English) is { } name)
                        {
                            trails.TryAdd(name, child);
                        }
                    }
                    else
                    {
                        queue.Enqueue(child);
                    }
                }
            }

            return trails;
        }

        private IEnumerable<IContent> Children(IContent parent) =>
            contentService.GetPagedChildren(parent.Id, 0, int.MaxValue, out _, propertyAliases: null, filter: null, ordering: null);

        /// <summary>Samma sökning som EnsurePage i AddMenuContent: samma typ och namn under föräldern</summary>
        private IContent? FindChild(IContent parent, string alias, PageText text) =>
            Children(parent)
                .FirstOrDefault(x => x.ContentType.Alias == alias
                    && (x.GetCultureName(English) == text.EnglishName || x.GetCultureName(Swedish) == text.SwedishName));

        /// <summary>Fyller i ingressen på båda språken och, för en undersida, lederna. Bara tomma fält, och sparar bara om något ändrades</summary>
        private void FillIn(IContent page, PageText text, IReadOnlyList<IContent>? trails)
        {
            // | och inte ||, så att alla fälten kollas även när det första redan har text
            var changed = SetIfEmpty(page, AddRegionAndMunicipality.IntroAlias, text.IntroEn, English)
                | SetIfEmpty(page, AddRegionAndMunicipality.IntroAlias, text.IntroSv, Swedish);

            if (trails is { Count: > 0 } && string.IsNullOrWhiteSpace(page.GetValue<string>(AddMenuPages.TrailsAlias)))
            {
                // Väljaren sparar lederna som UDI:er med kommatecken emellan, som i AddMenuContent
                page.SetValue(AddMenuPages.TrailsAlias,
                    string.Join(",", trails.Select(trail => Udi.Create(Constants.UdiEntityType.Document, trail.Key).ToString())));
                changed = true;
            }

            if (!changed)
            {
                return;
            }

            var saved = contentService.Save(page);

            if (!saved.Success)
            {
                throw new InvalidOperationException($"Could not save the page '{text.EnglishName}': {saved.Result}");
            }

            var published = contentService.Publish(page, ["*"]);

            if (!published.Success)
            {
                throw new InvalidOperationException($"Could not publish the page '{text.EnglishName}': {published.Result}");
            }
        }

        /// <summary>Sätter värdet bara om fältet är tomt på det språket. Returnerar true om något sattes.</summary>
        private static bool SetIfEmpty(IContent page, string alias, string value, string culture)
        {
            if (!string.IsNullOrWhiteSpace(page.GetValue<string>(alias, culture)))
            {
                return false;
            }

            page.SetValue(alias, value, culture);
            return true;
        }

        private sealed record PageText(string EnglishName, string SwedishName, string IntroEn, string IntroSv);

        private sealed record CategoryText(PageText Text, string[] TrailNames);

        private sealed record ActivityText(PageText Text, CategoryText[] Categories);
    }
}
