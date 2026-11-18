using System.Globalization;
using System.Net;
using OutdoorGuide.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>
    /// Sajtens riktiga innehåll från Data/Innehall (OUT-100): regionen, kommunerna och lederna på båda språken,
    /// med aktivitet och bilder. Sidor som saknas skapas, och sidor som finns (samma namn under samma förälder) uppdateras.
    /// </summary>
    /// <remarks>
    /// Följer undantaget under regel 3 i CLAUDE.md: ett fält fylls i om det är tomt, och byts bara om värdet
    /// fortfarande är exakt samma som i Data/Testinnehall. Ett värde som skiljer sig har någon ändrat i backoffice.
    /// Bilderna läggs i mappen <see cref="MediaFolderName"/> i Media, eftersom wwwroot/media inte delas i git.
    /// </remarks>
    public class AddSiteContent(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IContentService contentService,
        IMediaService mediaService,
        IMediaImportService mediaImportService,
        IJsonSerializer jsonSerializer,
        IHostEnvironment hostEnvironment)
        : UnscopedAsyncMigrationBase(context)
    {
        public const string MediaFolderName = "Friluftsguiden";

        private const string English = "en-US";
        private const string Swedish = "sv-SE";

        private static readonly Guid UserKey = Constants.Security.SuperUserKey;

        // Bilderna som redan finns i mappen, efter filnamn, så att samma bild inte läggs in två gånger
        private readonly Dictionary<string, IMedia> _images = new(StringComparer.OrdinalIgnoreCase);

        protected override async Task MigrateAsync()
        {
            using (var scope = scopeProvider.CreateCoreScope())
            {
                var start = contentService.GetRootContent().FirstOrDefault(x => x.ContentType.Alias == AddBaseAndStart.StartTypeAlias);

                if (start == null)
                {
                    Logger.LogWarning("No start page found, so no site content was added.");
                }
                else
                {
                    await AddContentAsync(start);
                }

                scope.Complete();
            }

            // Unscoped migrations måste själva säga att de är klara, då sparas planens nya läge
            Context.Complete();
        }

        private async Task AddContentAsync(IContent start)
        {
            var folder = EnsureMediaFolder();

            // Testinnehållet, efter id, för att se om ett värde fortfarande är det gamla
            var oldMunicipalities = ReadById("Testinnehall", "kommuner.csv");
            var oldTrails = ReadById("Testinnehall", "leder.csv");

            var municipalityPages = new Dictionary<string, IContent>();

            foreach (var row in Read("Innehall", "kommuner.csv"))
            {
                var region = FindPage(start, AddRegionAndMunicipality.RegionTypeAlias, row["region_en"], row["region_sv"])
                    ?? CreatePage(start, AddRegionAndMunicipality.RegionTypeAlias, row["region_en"], row["region_sv"]);
                SaveAndPublish(region, false);

                var old = oldMunicipalities.GetValueOrDefault(row["id"]);
                var page = FindPage(region, AddRegionAndMunicipality.MunicipalityTypeAlias, row["namn_en"], row["namn_sv"]);
                var isNew = page == null;
                page ??= CreatePage(region, AddRegionAndMunicipality.MunicipalityTypeAlias, row["namn_en"], row["namn_sv"]);

                var changed = await SetCommonValuesAsync(page, row, old, folder);
                SaveAndPublish(page, isNew || changed);

                municipalityPages[row["id"]] = page;
            }

            foreach (var row in Read("Innehall", "leder.csv"))
            {
                if (!municipalityPages.TryGetValue(row["kommun_id"], out var municipality))
                {
                    Logger.LogWarning("The trail {Trail} belongs to {Municipality}, which is not in kommuner.csv.", row["id"], row["kommun_id"]);
                    continue;
                }

                var old = oldTrails.GetValueOrDefault(row["id"]);
                var page = FindPage(municipality, AddTrail.TrailTypeAlias, row["namn_en"], row["namn_sv"]);
                var isNew = page == null;
                page ??= CreatePage(municipality, AddTrail.TrailTypeAlias, row["namn_en"], row["namn_sv"]);

                // | och inte ||, så att alla fält kollas även när ett tidigare redan har ändrats
                var changed = await SetCommonValuesAsync(page, row, old, folder)
                    | SetIfEmpty(page, AddTrail.LengthAlias, ParseDecimal(row["langd_km"]))
                    | SetIfEmpty(page, AddTrail.DurationAlias, ParseDecimal(row["tid_timmar"]))
                    | SetIfEmptyOrUnchanged(page, AddTrail.DifficultyAlias, ParseDifficulty(row["svarighet"]), old == null ? null : ParseDifficulty(old["svarighet"]))
                    | SetIfEmpty(page, AddTrail.ChildFriendlyAlias, row["barnvanlig"] == "Ja")
                    | SetIfEmpty(page, AddTrail.LoopAlias, row["rundslinga"] == "Ja")
                    // Testinnehållet har ingen aktivitet, så det gamla värdet är alltid tomt
                    | SetIfEmptyOrUnchanged(page, AddTrailActivity.ActivityAlias, ParseActivity(row["aktivitet"]), null);

                SaveAndPublish(page, isNew || changed);
            }
        }

        /// <summary>Fälten som Kommun och Led har gemensamt: texterna på båda språken, bilden och koordinaterna.</summary>
        private async Task<bool> SetCommonValuesAsync(IContent page, Dictionary<string, string> row, Dictionary<string, string>? old, IMedia folder)
        {
            var changed = false;

            foreach (var (culture, suffix) in new[] { (English, "en"), (Swedish, "sv") })
            {
                changed |= SetIfEmptyOrUnchanged(page, AddRegionAndMunicipality.IntroAlias, row[$"ingress_{suffix}"], old?[$"ingress_{suffix}"], culture);
                changed |= SetIfEmptyOrUnchanged(page, AddRegionAndMunicipality.DescriptionAlias, Paragraph(row[$"beskrivning_{suffix}"]), old == null ? null : Paragraph(old[$"beskrivning_{suffix}"]), culture);
                changed |= SetIfEmptyOrUnchanged(page, AddRegionAndMunicipality.ImageAltAlias, row[$"bild_alt_{suffix}"], old?[$"bild_alt_{suffix}"], culture);
            }

            changed |= SetIfEmpty(page, AddRegionAndMunicipality.LatitudeAlias, ParseDecimal(row["lat"]));
            changed |= SetIfEmpty(page, AddRegionAndMunicipality.LongitudeAlias, ParseDecimal(row["lng"]));

            // Bilden läggs bara in i Media om sidan inte redan har en bild
            if (IsEmpty(page.GetValue(AddRegionAndMunicipality.ImageAlias)))
            {
                var image = await EnsureImageAsync(folder, row["bild_fil"]);

                if (image != null)
                {
                    // Samma format som Image Media Picker sparar: en lista med en bild
                    page.SetValue(AddRegionAndMunicipality.ImageAlias, jsonSerializer.Serialize(new[] { new { key = Guid.NewGuid(), mediaKey = image.Key } }));
                    changed = true;
                }
            }

            return changed;
        }

        /// <summary>Sätter värdet om fältet är tomt, eller om det fortfarande är samma som i testinnehållet.</summary>
        private static bool SetIfEmptyOrUnchanged(IContent page, string alias, string value, string? oldValue, string? culture = null)
        {
            var current = page.GetValue<string>(alias, culture);

            if (current == value || (!string.IsNullOrWhiteSpace(current) && current != oldValue))
            {
                return false;
            }

            page.SetValue(alias, value, culture);
            return true;
        }

        /// <summary>
        /// Sätter värdet bara om fältet är tomt. Används för siffror och ja/nej, som är likadana i testinnehållet
        /// och därför bara behöver sättas på nya sidor.
        /// </summary>
        private static bool SetIfEmpty(IContent page, string alias, object value)
        {
            if (!IsEmpty(page.GetValue(alias)))
            {
                return false;
            }

            page.SetValue(alias, value);
            return true;
        }

        private static bool IsEmpty(object? value) => value == null || value is string text && (string.IsNullOrWhiteSpace(text) || text == "[]");

        private IContent? FindPage(IContent parent, string alias, string nameEn, string nameSv)
        {
            // Samma sökning som EnsurePage i AddTestContent: samma typ och namn under samma förälder
            return contentService
                .GetPagedChildren(parent.Id, 0, int.MaxValue, out _, propertyAliases: null, filter: null, ordering: null)
                .FirstOrDefault(x => x.ContentType.Alias == alias
                    && (x.GetCultureName(English) == nameEn || x.GetCultureName(Swedish) == nameSv));
        }

        private IContent CreatePage(IContent parent, string alias, string nameEn, string nameSv)
        {
            var page = contentService.Create(nameEn, parent.Key, alias);
            page.SetCultureName(nameEn, English);
            page.SetCultureName(nameSv, Swedish);
            return page;
        }

        /// <summary>
        /// Sparar och publicerar på båda språken. En befintlig sida som är avpublicerad sparas men publiceras inte,
        /// eftersom någon har avpublicerat den med flit.
        /// </summary>
        private void SaveAndPublish(IContent page, bool changed)
        {
            var isNew = !page.HasIdentity;

            if (!changed && !isNew)
            {
                return;
            }

            var saved = contentService.Save(page);

            if (!saved.Success)
            {
                throw new InvalidOperationException($"Could not save the page '{page.Name}': {saved.Result}");
            }

            if (!isNew && !page.Published)
            {
                return;
            }

            var published = contentService.Publish(page, ["*"]);

            if (!published.Success)
            {
                throw new InvalidOperationException($"Could not publish the page '{page.Name}': {published.Result}");
            }
        }

        private IMedia EnsureMediaFolder()
        {
            var folder = mediaService.GetRootMedia()
                .FirstOrDefault(x => x.ContentType.Alias == Constants.Conventions.MediaTypes.Folder && x.Name == MediaFolderName);

            if (folder == null)
            {
                folder = mediaService.CreateMedia(MediaFolderName, Constants.System.Root, Constants.Conventions.MediaTypes.Folder);
                var saved = mediaService.Save(folder);

                if (!saved.Success)
                {
                    throw new InvalidOperationException($"Could not create the media folder '{MediaFolderName}': {saved.Result}");
                }
            }

            foreach (var media in mediaService.GetPagedChildren(folder.Id, 0, int.MaxValue, out _, filter: null, ordering: null))
            {
                if (media.Name != null)
                {
                    _images.TryAdd(media.Name, media);
                }
            }

            return folder;
        }

        /// <summary>Bilden från Data/Innehall/bilder i Media-mappen. Finns den redan där används den.</summary>
        private async Task<IMedia?> EnsureImageAsync(IMedia folder, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            if (_images.TryGetValue(fileName, out var existing))
            {
                return existing;
            }

            var path = Path.Combine(hostEnvironment.ContentRootPath, "Data", "Innehall", "bilder", fileName);

            if (!System.IO.File.Exists(path))
            {
                Logger.LogWarning("The image {File} does not exist in Data/Innehall/bilder.", fileName);
                return null;
            }

            IMedia image;

            await using (var stream = System.IO.File.OpenRead(path))
            {
                image = await mediaImportService.ImportAsync(fileName, stream, folder.Key, Constants.Conventions.MediaTypes.Image, UserKey);
            }

            // Filnamnet som namn i Media, så att bilden hittas igen och syns tydligt i backoffice
            image.Name = fileName;
            mediaService.Save(image);

            _images[fileName] = image;
            return image;
        }

        private List<Dictionary<string, string>> Read(string folder, string fileName) =>
            ContentCsv.Read(Path.Combine(hostEnvironment.ContentRootPath, "Data", folder, fileName));

        private Dictionary<string, Dictionary<string, string>> ReadById(string folder, string fileName) =>
            Read(folder, fileName).ToDictionary(row => row["id"]);

        // Samma format som AddTestContent sparade beskrivningen i
        private static string Paragraph(string text) => $"<p>{WebUtility.HtmlEncode(text)}</p>";

        // Punkt som decimaltecken, som i csv:n (3.5), oavsett om datorn är inställd på svenska
        private static decimal ParseDecimal(string value) => decimal.Parse(value, CultureInfo.InvariantCulture);

        private static string ParseDifficulty(string value) => value switch
        {
            "Lätt" => DifficultyValues.Easy,
            "Medel" => DifficultyValues.Medium,
            "Svår" => DifficultyValues.Hard,
            _ => throw new InvalidOperationException($"Unknown difficulty '{value}' in leder.csv."),
        };

        private static string ParseActivity(string value) => ActivityValues.All.Contains(value)
            ? value
            : throw new InvalidOperationException($"Unknown activity '{value}' in leder.csv.");
    }
}