using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>
    /// Sidorna Allemansrätten och Integritet och cookies (OUT-94), på svenska och engelska, med texter.
    /// Sidorna är textPage under startsidan, som Säsongstips, Om oss och Kontakt, men ligger inte i menyn utan i sidfoten (OUT-47).
    /// En sida som finns används som den är, och ett fält som redan har text lämnas, så inget som någon har skrivit
    /// i backoffice skrivs över (undantaget under regel 3 i CLAUDE.md).
    /// </summary>
    public class AddLegalPages(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IContentService contentService)
        : UnscopedAsyncMigrationBase(context)
    {
        private const string English = "en-US";
        private const string Swedish = "sv-SE";

        /// <summary>Det engelska namnet på integritetssidan. Cookie-rutan letar efter sidan med det här namnet.</summary>
        public const string PrivacyPageEnglishName = "Privacy and cookies";

        private static readonly PageText[] Pages =
        [
            new("Right of Public Access", "Allemansrätten",
                IntroEn: "The Right of Public Access lets everyone enjoy nature in Sweden, as long as you don't disturb and don't destroy.",
                IntroSv: "Allemansrätten gör att alla får vistas i naturen i Sverige, så länge man inte stör och inte förstör.",
                DescriptionEn: """
                    <p>The Right of Public Access is a unique freedom, but it comes with a responsibility to take care of nature and show consideration for landowners and other visitors.</p>
                    <h2>You may</h2>
                    <ul>
                    <li>walk, cycle and ski on land that is not a garden or growing crops</li>
                    <li>pick wild berries, mushrooms and flowers that are not protected</li>
                    <li>swim, paddle and go ashore where it is not near someone's home</li>
                    <li>pitch a tent for one or two nights, away from houses and farmland</li>
                    </ul>
                    <h2>Fire</h2>
                    <p>Check whether there is a fire ban in the municipality before you light a fire. Never make a fire on bare rock, which can crack, and put it out completely before you leave. Use a fire pit where there is one.</p>
                    <h2>Dogs</h2>
                    <p>From 1 March to 20 August, dogs must not run loose where there may be wildlife. Keep your dog under control the rest of the year too.</p>
                    <h2>Nature reserves</h2>
                    <p>Many of the trails go through nature reserves and national parks, which have their own rules. Read the signs at the entrance, for example about camping, fires and dogs.</p>
                    <p>Read more about the Right of Public Access at the <a href="https://www.naturvardsverket.se/en/topics/the-right-of-public-access/">Swedish Environmental Protection Agency</a>.</p>
                    """,
                DescriptionSv: """
                    <p>Allemansrätten är en unik frihet, men den kommer med ett ansvar: att ta hand om naturen och visa hänsyn mot markägare och andra besökare.</p>
                    <h2>Det här får du göra</h2>
                    <ul>
                    <li>gå, cykla och åka skidor på mark som inte är tomt eller odlad mark</li>
                    <li>plocka vilda bär, svamp och blommor som inte är fridlysta</li>
                    <li>bada, paddla och gå i land där det inte är nära någons hus</li>
                    <li>tälta en eller två nätter, en bit från hus och jordbruksmark</li>
                    </ul>
                    <h2>Eld</h2>
                    <p>Kolla om det är eldningsförbud i kommunen innan du gör upp eld. Elda aldrig direkt på berghällar, som kan spricka, och släck helt innan du går. Använd en eldstad där det finns en.</p>
                    <h2>Hund</h2>
                    <p>Mellan 1 mars och 20 augusti får hundar inte springa lösa där det kan finnas vilt. Ha kontroll på hunden resten av året också.</p>
                    <h2>Naturreservat</h2>
                    <p>Många av lederna går genom naturreservat och nationalparker, som har egna regler. Läs skylten vid entrén, till exempel om tältning, eld och hundar.</p>
                    <p>Läs mer om allemansrätten hos <a href="https://www.naturvardsverket.se/amnesomraden/allemansratten/">Naturvårdsverket</a>.</p>
                    """),

            new(PrivacyPageEnglishName, "Integritet och cookies",
                IntroEn: "What we save about you, why, and how you change your choice of cookies.",
                IntroSv: "Vad vi sparar om dig, varför, och hur du ändrar ditt val av cookies.",
                DescriptionEn: """
                    <h2>Cookies</h2>
                    <p>We use necessary cookies, which the site needs to work: one that keeps you logged in as a member, one that protects the forms on the site, and one that remembers your choice in the cookie box. They are saved for as long as the choice in the cookie box, at most one year.</p>
                    <p>With your consent we may also use cookies for statistics. Right now the site does not use any such cookies.</p>
                    <p>You can change your choice at any time with the link "Cookie settings" at the bottom of every page.</p>
                    <h2>If you create an account</h2>
                    <p>We save your name, your email address and your password, which is stored encrypted. We also save the ratings you give trails, so that you can change them.</p>
                    <h2>If you sign up for the newsletter</h2>
                    <p>We save your email address, only to send you the newsletter.</p>
                    <h2>If you report a trail's condition</h2>
                    <p>We save the report, your comment and your name if you give one, so that we can update the trail. The name is optional, and you don't need to write any other personal details.</p>
                    <h2>Your rights</h2>
                    <p>You can ask us what we have saved about you, and ask us to correct or delete it. Email <a href="mailto:hej@friluftsguiden.se">hej@friluftsguiden.se</a>.</p>
                    <p>Friluftsguiden is a school project at Nackademin, and no data is shared with anyone else.</p>
                    """,
                DescriptionSv: """
                    <h2>Cookies</h2>
                    <p>Vi använder nödvändiga cookies, som sajten behöver för att fungera: en som håller dig inloggad som medlem, en som skyddar formulären på sajten och en som kommer ihåg ditt val i cookie-rutan. De sparas så länge valet i cookie-rutan gäller, högst ett år.</p>
                    <p>Med ditt samtycke kan vi också använda cookies för statistik. Just nu använder sajten inga sådana cookies.</p>
                    <p>Du kan ändra ditt val när som helst med länken "Cookie-inställningar" längst ner på varje sida.</p>
                    <h2>Om du skapar ett konto</h2>
                    <p>Vi sparar ditt namn, din e-postadress och ditt lösenord, som sparas krypterat. Vi sparar också de betyg du ger lederna, så att du kan ändra dem.</p>
                    <h2>Om du anmäler dig till nyhetsbrevet</h2>
                    <p>Vi sparar din e-postadress, bara för att kunna skicka nyhetsbrevet till dig.</p>
                    <h2>Om du rapporterar ledens skick</h2>
                    <p>Vi sparar rapporten, din kommentar och ditt namn om du skriver det, så att vi kan uppdatera leden. Namnet är frivilligt, och du behöver inte skriva några andra personuppgifter.</p>
                    <h2>Dina rättigheter</h2>
                    <p>Du kan fråga oss vad vi har sparat om dig, och be oss rätta eller ta bort det. Mejla <a href="mailto:hej@friluftsguiden.se">hej@friluftsguiden.se</a>.</p>
                    <p>Friluftsguiden är ett skolprojekt på Nackademin, och inga uppgifter delas med någon annan.</p>
                    """),
        ];

        protected override Task MigrateAsync()
        {
            using (var scope = scopeProvider.CreateCoreScope())
            {
                var start = contentService.GetRootContent().FirstOrDefault(x => x.ContentType.Alias == AddBaseAndStart.StartTypeAlias);

                if (start == null)
                {
                    Logger.LogWarning("No start page found, so the legal pages were not created.");
                }
                else
                {
                    foreach (var text in Pages)
                    {
                        EnsurePage(start, text);
                    }
                }

                scope.Complete();
            }

            // Unscoped migrations måste själva säga att de är klara, då sparas planens nya läge
            Context.Complete();
            return Task.CompletedTask;
        }

        private void EnsurePage(IContent start, PageText text)
        {
            // Samma sökning som EnsurePage i AddMenuContent: samma typ och namn under startsidan
            var page = contentService
                .GetPagedChildren(start.Id, 0, int.MaxValue, out _, propertyAliases: null, filter: null, ordering: null)
                .FirstOrDefault(x => x.ContentType.Alias == AddMenuPages.TextPageTypeAlias
                    && (x.GetCultureName(English) == text.EnglishName || x.GetCultureName(Swedish) == text.SwedishName));

            var created = page == null;

            if (page == null)
            {
                page = contentService.Create(text.EnglishName, start.Key, AddMenuPages.TextPageTypeAlias);
                page.SetCultureName(text.EnglishName, English);
                page.SetCultureName(text.SwedishName, Swedish);
            }

            // | och inte ||, så att alla fyra fälten kollas även när det första redan har text
            var changed = SetIfEmpty(page, AddRegionAndMunicipality.IntroAlias, text.IntroEn, English)
                | SetIfEmpty(page, AddRegionAndMunicipality.IntroAlias, text.IntroSv, Swedish)
                | SetIfEmpty(page, AddRegionAndMunicipality.DescriptionAlias, text.DescriptionEn, English)
                | SetIfEmpty(page, AddRegionAndMunicipality.DescriptionAlias, text.DescriptionSv, Swedish);

            if (!created && !changed)
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
