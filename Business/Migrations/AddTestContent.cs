using System.Globalization;
using System.Net;
using System.Text;
using OutdoorGuide.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>
    /// Testinnehållet från Data/Testinnehall: regionen, kommunerna i kommuner.csv och lederna i leder.csv,
    /// på svenska och engelska, publicerade under startsidan.
    /// En sida som redan finns (samma namn under samma förälder) lämnas som den är, så inget skrivs över.
    /// </summary>
    /// <remarks>
    /// Körs bara i Development: testinnehåll ska inte hamna på en riktig sajt. Bilderna tas inte med,
    /// eftersom bildfilerna inte finns i repot; alt-texterna läggs in ändå.
    /// </remarks>
    public class AddTestContent(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IContentService contentService,
        IHostEnvironment hostEnvironment)
        : UnscopedAsyncMigrationBase(context)
    {
        private const string English = "en-US";
        private const string Swedish = "sv-SE";

        protected override Task MigrateAsync()
        {
            if (!hostEnvironment.IsDevelopment())
            {
                Logger.LogInformation("Not running in Development, so no test content was added.");
                Context.Complete();
                return Task.CompletedTask;
            }

            using (var scope = scopeProvider.CreateCoreScope())
            {
                var start = contentService.GetRootContent().FirstOrDefault(x => x.ContentType.Alias == AddBaseAndStart.StartTypeAlias);

                if (start == null)
                {
                    Logger.LogWarning("No start page found, so no test content was added.");
                }
                else
                {
                    AddContent(start);
                }

                scope.Complete();
            }

            // Unscoped migrations måste själva säga att de är klara, då sparas planens nya läge
            Context.Complete();
            return Task.CompletedTask;
        }

        private void AddContent(IContent start)
        {
            var municipalityPages = new Dictionary<string, IContent>();

            foreach (var row in ReadCsv("kommuner.csv"))
            {
                var region = EnsurePage(start, AddRegionAndMunicipality.RegionTypeAlias, row["region_en"], row["region_sv"], _ => { });

                municipalityPages[row["id"]] = EnsurePage(region, AddRegionAndMunicipality.MunicipalityTypeAlias, row["namn_en"], row["namn_sv"], page =>
                {
                    SetCommonValues(page, row);
                });
            }

            foreach (var row in ReadCsv("leder.csv"))
            {
                if (!municipalityPages.TryGetValue(row["kommun_id"], out var municipality))
                {
                    Logger.LogWarning("The trail {Trail} belongs to {Municipality}, which is not in kommuner.csv.", row["id"], row["kommun_id"]);
                    continue;
                }

                EnsurePage(municipality, AddTrail.TrailTypeAlias, row["namn_en"], row["namn_sv"], page =>
                {
                    SetCommonValues(page, row);
                    page.SetValue(AddTrail.LengthAlias, ParseDecimal(row["langd_km"]));
                    page.SetValue(AddTrail.DurationAlias, ParseDecimal(row["tid_timmar"]));
                    page.SetValue(AddTrail.DifficultyAlias, ParseDifficulty(row["svarighet"]));
                    page.SetValue(AddTrail.ChildFriendlyAlias, row["barnvanlig"] == "Ja");
                    page.SetValue(AddTrail.LoopAlias, row["rundslinga"] == "Ja");
                });
            }
        }

        /// <summary>Fälten som Kommun och Led har gemensamt: texterna på båda språken och koordinaterna.</summary>
        private static void SetCommonValues(IContent page, Dictionary<string, string> row)
        {
            foreach (var (culture, suffix) in new[] { (English, "en"), (Swedish, "sv") })
            {
                page.SetValue(AddRegionAndMunicipality.IntroAlias, row[$"ingress_{suffix}"], culture);
                page.SetValue(AddRegionAndMunicipality.DescriptionAlias, $"<p>{WebUtility.HtmlEncode(row[$"beskrivning_{suffix}"])}</p>", culture);
                page.SetValue(AddRegionAndMunicipality.ImageAltAlias, row[$"bild_alt_{suffix}"], culture);
            }

            page.SetValue(AddRegionAndMunicipality.LatitudeAlias, ParseDecimal(row["lat"]));
            page.SetValue(AddRegionAndMunicipality.LongitudeAlias, ParseDecimal(row["lng"]));
        }

        private IContent EnsurePage(IContent parent, string alias, string nameEn, string nameSv, Action<IContent> setValues)
        {
            var existing = contentService
                .GetPagedChildren(parent.Id, 0, int.MaxValue, out _, propertyAliases: null, filter: null, ordering: null)
                .FirstOrDefault(x => x.ContentType.Alias == alias
                    && (x.GetCultureName(English) == nameEn || x.GetCultureName(Swedish) == nameSv));

            if (existing != null)
            {
                return existing;
            }

            var page = contentService.Create(nameEn, parent.Key, alias);
            page.SetCultureName(nameEn, English);
            page.SetCultureName(nameSv, Swedish);
            setValues(page);

            var saved = contentService.Save(page);

            if (!saved.Success)
            {
                throw new InvalidOperationException($"Could not save the page '{nameEn}': {saved.Result}");
            }

            var published = contentService.Publish(page, ["*"]);

            if (!published.Success)
            {
                throw new InvalidOperationException($"Could not publish the page '{nameEn}': {published.Result}");
            }

            return page;
        }

        private List<Dictionary<string, string>> ReadCsv(string fileName)
        {
            var path = Path.Combine(hostEnvironment.ContentRootPath, "Data", "Testinnehall", fileName);
            var rows = ParseCsv(System.IO.File.ReadAllText(path, Encoding.UTF8));
            var header = rows[0];

            // Rader med fel antal kolumner (till exempel en tom rad sist) hoppas över
            return rows.Skip(1)
                .Where(row => row.Count == header.Count)
                .Select(row => header.Zip(row).ToDictionary(x => x.First, x => x.Second))
                .ToList();
        }

        /// <summary>
        /// Semikolonseparerad csv som Excel sparar den: fält inom citattecken får innehålla ; och "" betyder ett ".
        /// </summary>
        private static List<List<string>> ParseCsv(string text)
        {
            var rows = new List<List<string>>();
            var row = new List<string>();
            var field = new StringBuilder();
            var quoted = false;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];

                if (quoted)
                {
                    if (c == '"' && i + 1 < text.Length && text[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else if (c == '"')
                    {
                        quoted = false;
                    }
                    else
                    {
                        field.Append(c);
                    }
                }
                else if (c == '"')
                {
                    quoted = true;
                }
                else if (c == ';')
                {
                    row.Add(field.ToString());
                    field.Clear();
                }
                else if (c == '\n')
                {
                    row.Add(field.ToString().TrimEnd('\r'));
                    field.Clear();
                    rows.Add(row);
                    row = [];
                }
                else
                {
                    field.Append(c);
                }
            }

            if (field.Length > 0 || row.Count > 0)
            {
                row.Add(field.ToString().TrimEnd('\r'));
                rows.Add(row);
            }

            return rows;
        }

        // Punkt som decimaltecken, som i csv:n (3.5), oavsett om datorn är inställd på svenska
        private static decimal ParseDecimal(string value) => decimal.Parse(value, CultureInfo.InvariantCulture);

        private static string ParseDifficulty(string value) => value switch
        {
            "Lätt" => DifficultyValues.Easy,
            "Medel" => DifficultyValues.Medium,
            "Svår" => DifficultyValues.Hard,
            _ => throw new InvalidOperationException($"Unknown difficulty '{value}' in leder.csv."),
        };
    }
}