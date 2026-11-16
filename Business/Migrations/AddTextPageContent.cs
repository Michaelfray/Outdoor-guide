using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>
    /// Texterna på Säsongstips, Om oss och Kontakt (OUT-95), på svenska och engelska.
    /// Sidorna skapas i <see cref="AddMenuContent"/>. Ett fält som redan har text lämnas som det är,
    /// så inget som någon har skrivit i backoffice skrivs över (undantaget under regel 3 i CLAUDE.md).
    /// </summary>
    public class AddTextPageContent(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IContentService contentService)
        : UnscopedAsyncMigrationBase(context)
    {
        private const string English = "en-US";
        private const string Swedish = "sv-SE";

        private static readonly PageText[] Pages =
        [
            new("Seasonal tips", "Säsongstips",
                IntroEn: "What to keep in mind through the four seasons, from muddy spring trails to winter ice.",
                IntroSv: "Vad du behöver tänka på under årets fyra säsonger, från vårens blöta stigar till vinterns is.",
                DescriptionEn: """
                    <h2>Spring</h2>
                    <p>Trails can stay wet and muddy well into May, especially in forests and near lakes. Wear waterproof boots and expect to go slower than usual. Many birds are nesting now, so keep your dog on a lead.</p>
                    <h2>Summer</h2>
                    <p>Start early on hot days and bring more water than you think you need. Check whether there is a fire ban in the municipality before you light a barbecue, and check for ticks when you get home.</p>
                    <h2>Autumn</h2>
                    <p>Cooler air, fewer insects and colourful forests make autumn a favourite for many hikers. The days get shorter quickly, so plan your walk around when it gets dark and bring a head torch.</p>
                    <h2>Winter</h2>
                    <p>Never go out on the ice alone. Bring ice claws and an ice probe, and ask locally about the ice before you go. Trails can be slippery, and ice grips on your shoes make a big difference.</p>
                    """,
                DescriptionSv: """
                    <h2>Vår</h2>
                    <p>Stigarna kan vara blöta och leriga långt in i maj, särskilt i skogen och vid sjöarna. Välj vattentäta kängor och räkna med att det går långsammare än vanligt. Många fåglar häckar nu, så ha hunden kopplad.</p>
                    <h2>Sommar</h2>
                    <p>Börja tidigt på varma dagar och ta med mer vatten än du tror att du behöver. Kolla om det är eldningsförbud i kommunen innan du grillar, och leta efter fästingar när du kommer hem.</p>
                    <h2>Höst</h2>
                    <p>Svalare luft, färre insekter och färgglad skog gör hösten till många vandrares favorit. Dagarna blir snabbt kortare, så planera turen efter när det blir mörkt och ta med pannlampa.</p>
                    <h2>Vinter</h2>
                    <p>Gå aldrig ut på isen ensam. Ta med isdubbar och ispik, och fråga gärna lokalt om isen innan du går. På stigarna kan det vara halt, och broddar under skorna gör stor skillnad.</p>
                    """),

            new("About us", "Om oss",
                IntroEn: "Friluftsguiden brings together trails for hiking, paddling, cycling and winter in Stockholm County.",
                IntroSv: "Friluftsguiden samlar leder för vandring, paddling, cykel och vinter i Stockholms län.",
                DescriptionEn: """
                    <p>We want to make it easy to find a trip that suits you, whether you have an hour after work or a whole weekend. For every trail you can see the length, time, difficulty and whether it suits children, and you can filter to find the trails you are looking for.</p>
                    <p>The weather forecast on each trail page is updated every hour, and as a member you can rate the trails you have walked. If you have been on a trail recently, you can report its condition so the next visitor knows what to expect.</p>
                    <p>Friluftsguiden is a school project in the course CMS Development at Nackademin, built in Umbraco by three students.</p>
                    """,
                DescriptionSv: """
                    <p>Vi vill göra det enkelt att hitta en tur som passar dig, oavsett om du har en timme efter jobbet eller en hel helg. För varje led ser du längd, tid, svårighet och om den passar barn, och du kan filtrera fram de leder du letar efter.</p>
                    <p>Väderprognosen på ledsidan uppdateras varje timme, och som medlem kan du betygsätta lederna du har gått. Har du varit på en led nyligen kan du rapportera ledens skick, så att nästa besökare vet vad som väntar.</p>
                    <p>Friluftsguiden är ett skolprojekt i kursen CMS-utveckling på Nackademin, byggt i Umbraco av tre studenter.</p>
                    """),

            new("Contact", "Kontakt",
                IntroEn: "Have a question, a tip about a new trail or found something that is wrong? Get in touch.",
                IntroSv: "Har du en fråga, ett tips om en ny led eller hittat något som är fel? Hör av dig.",
                DescriptionEn: """
                    <h2>Email</h2>
                    <p><a href="mailto:hej@friluftsguiden.se">hej@friluftsguiden.se</a>. We reply within two working days.</p>
                    <h2>Trail condition</h2>
                    <p>Have you seen a fallen tree, a flooded path or a broken sign? Use the form "Report trail condition" on the trail page, and the report comes straight to us.</p>
                    <h2>Emergencies</h2>
                    <p>In an accident or emergency, call 112.</p>
                    """,
                DescriptionSv: """
                    <h2>E-post</h2>
                    <p><a href="mailto:hej@friluftsguiden.se">hej@friluftsguiden.se</a>. Vi svarar inom två arbetsdagar.</p>
                    <h2>Ledens skick</h2>
                    <p>Har du sett ett nedfallet träd, en översvämmad stig eller en trasig skylt? Använd formuläret "Rapportera ledens skick" på ledsidan, så kommer rapporten direkt till oss.</p>
                    <h2>Akut</h2>
                    <p>Vid olycka eller fara, ring 112.</p>
                    """),
        ];

        protected override Task MigrateAsync()
        {
            using (var scope = scopeProvider.CreateCoreScope())
            {
                var start = contentService.GetRootContent().FirstOrDefault(x => x.ContentType.Alias == AddBaseAndStart.StartTypeAlias);

                if (start == null)
                {
                    Logger.LogWarning("No start page found, so the text pages got no content.");
                }
                else
                {
                    foreach (var text in Pages)
                    {
                        FillPage(start, text);
                    }
                }

                scope.Complete();
            }

            // Unscoped migrations måste själva säga att de är klara, då sparas planens nya läge
            Context.Complete();
            return Task.CompletedTask;
        }

        private void FillPage(IContent start, PageText text)
        {
            // Samma sökning som EnsurePage i AddMenuContent: samma typ och namn under startsidan
            var page = contentService
                .GetPagedChildren(start.Id, 0, int.MaxValue, out _, propertyAliases: null, filter: null, ordering: null)
                .FirstOrDefault(x => x.ContentType.Alias == AddMenuPages.TextPageTypeAlias
                    && (x.GetCultureName(English) == text.EnglishName || x.GetCultureName(Swedish) == text.SwedishName));

            if (page == null)
            {
                Logger.LogWarning("The page {Page} does not exist, so it got no content.", text.EnglishName);
                return;
            }

            // | och inte ||, så att alla fyra fälten kollas även när det första redan har ändrats
            var changed = SetIfEmpty(page, AddRegionAndMunicipality.IntroAlias, text.IntroEn, English)
                | SetIfEmpty(page, AddRegionAndMunicipality.IntroAlias, text.IntroSv, Swedish)
                | SetIfEmpty(page, AddRegionAndMunicipality.DescriptionAlias, text.DescriptionEn, English)
                | SetIfEmpty(page, AddRegionAndMunicipality.DescriptionAlias, text.DescriptionSv, Swedish);

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

        private sealed record PageText(
            string EnglishName,
            string SwedishName,
            string IntroEn,
            string IntroSv,
            string DescriptionEn,
            string DescriptionSv);
    }
}