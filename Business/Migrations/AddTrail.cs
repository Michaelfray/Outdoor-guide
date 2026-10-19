using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>
    /// Dokumenttypen <c>trail</c> (Led) under Kommun: datatypen "Decimal" för längd och tid, mallen Trail,
    /// dokumenttypen med Base som composition och fälten från leder.csv, och att Kommun tillåter Led som barn.
    /// Varje del kollar först om den redan finns, så inget som någon har skapat eller ändrat skrivs över.
    /// </summary>
    /// <remarks>
    /// Textfälten varierar per språk. Siffror, svårighet, ja/nej, bild och koordinater gör det inte, så redaktören
    /// fyller bara i dem en gång (OUT-6). Fälten som finns på både Kommun och Led har samma alias.
    /// </remarks>
    public class AddTrail(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        ITemplateService templateService,
        IContentTypeService contentTypeService,
        IDataTypeService dataTypeService,
        PropertyEditorCollection propertyEditors,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer,
        IShortStringHelper shortStringHelper,
        IHostEnvironment hostEnvironment)
        : UnscopedAsyncMigrationBase(context)
    {
        public const string TrailTypeAlias = "trail";
        public const string DecimalDataTypeName = "Decimal";

        public const string LengthAlias = "length";
        public const string DurationAlias = "duration";
        public const string DifficultyAlias = "difficulty";
        public const string ChildFriendlyAlias = "childFriendly";
        public const string LoopAlias = "loop";

        private static readonly Guid UserKey = Constants.Security.SuperUserKey;

        protected override async Task MigrateAsync()
        {
            using (var scope = scopeProvider.CreateCoreScope())
            {
                var decimalType = await EnsureDecimalDataTypeAsync();
                var template = await EnsureTemplateAsync("Trail", "Trail", "Trail.cshtml");

                var baseType = contentTypeService.Get(AddBaseAndStart.BaseTypeAlias)
                    ?? throw new InvalidOperationException($"The '{AddBaseAndStart.BaseTypeAlias}' content type does not exist.");

                var trailType = await EnsureTrailTypeAsync(baseType, template, decimalType);
                await EnsureAllowedChildAsync(AddRegionAndMunicipality.MunicipalityTypeAlias, trailType);

                scope.Complete();
            }

            // Unscoped migrations måste själva säga att de är klara, då sparas planens nya läge
            Context.Complete();
        }

        private async Task<IDataType> EnsureDecimalDataTypeAsync()
        {
            var existing = await dataTypeService.GetAsync(DecimalDataTypeName);

            if (existing != null)
            {
                return existing;
            }

            if (!propertyEditors.TryGet(Constants.PropertyEditors.Aliases.Decimal, out var decimalEditor))
            {
                throw new InvalidOperationException("The Decimal property editor is not available.");
            }

            var dataType = new DataType(decimalEditor, configurationEditorJsonSerializer)
            {
                Name = DecimalDataTypeName,
                EditorUiAlias = "Umb.PropertyEditorUi.Decimal",
                DatabaseType = ValueStorageType.Decimal,
                ConfigurationData = new Dictionary<string, object>
                {
                    // En decimal räcker för kilometer och timmar (3.5 km, 1.5 h)
                    ["min"] = 0,
                    ["step"] = 0.1,
                },
            };

            var attempt = await dataTypeService.CreateAsync(dataType, UserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not create the '{DecimalDataTypeName}' data type: {attempt.Status}");
            }

            return attempt.Result;
        }

        private async Task<ITemplate> EnsureTemplateAsync(string name, string alias, string viewFile)
        {
            var existing = await templateService.GetAsync(alias);

            if (existing != null)
            {
                return existing;
            }

            // Att skapa en mall skriver (om) vyfilen, så skicka med vyn som redan ligger i repot
            var viewPath = Path.Combine(hostEnvironment.ContentRootPath, "Views", viewFile);
            var content = System.IO.File.Exists(viewPath) ? await System.IO.File.ReadAllTextAsync(viewPath) : null;

            var attempt = await templateService.CreateAsync(name, alias, content, UserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not create the '{alias}' template: {attempt.Status}");
            }

            return attempt.Result;
        }

        private async Task<IContentType> EnsureTrailTypeAsync(IContentType baseType, ITemplate template, IDataType decimalType)
        {
            var existing = contentTypeService.Get(TrailTypeAlias);

            if (existing != null)
            {
                return existing;
            }

            var textarea = await GetDataTypeAsync(Constants.DataTypes.Guids.TextareaGuid);
            var richText = await GetDataTypeAsync(Constants.DataTypes.Guids.RichtextEditorGuid);
            var textstring = await GetDataTypeAsync(Constants.DataTypes.Guids.TextstringGuid);
            var image = await GetDataTypeAsync("Image Media Picker");
            var trueFalse = await GetDataTypeAsync("True/false");
            var difficulty = await GetDataTypeAsync(AddDifficultyDataType.DataTypeName);
            var coordinate = await GetDataTypeAsync(AddRegionAndMunicipality.CoordinateDataTypeName);

            // Inte tillåten i roten: Led ligger bara under Kommun
            var trailType = new ContentType(shortStringHelper, Constants.System.Root)
            {
                Alias = TrailTypeAlias,
                Name = "Trail",
                Icon = "icon-hiking",
                Variations = ContentVariation.Culture,
                AllowedTemplates = [template],
            };
            trailType.SetDefaultTemplate(template);
            trailType.AddContentType(baseType);

            trailType.AddPropertyType(NewProperty(textarea, AddRegionAndMunicipality.IntroAlias, "Intro", ContentVariation.Culture, 0), "content", "Content");
            trailType.AddPropertyType(NewProperty(richText, AddRegionAndMunicipality.DescriptionAlias, "Description", ContentVariation.Culture, 1), "content", "Content");

            trailType.AddPropertyType(NewProperty(decimalType, LengthAlias, "Length (km)", ContentVariation.Nothing, 0), "facts", "Facts");
            trailType.AddPropertyType(NewProperty(decimalType, DurationAlias, "Duration (hours)", ContentVariation.Nothing, 1), "facts", "Facts");
            trailType.AddPropertyType(NewProperty(difficulty, DifficultyAlias, "Difficulty", ContentVariation.Nothing, 2), "facts", "Facts");
            trailType.AddPropertyType(NewProperty(trueFalse, ChildFriendlyAlias, "Child friendly", ContentVariation.Nothing, 3), "facts", "Facts");
            trailType.AddPropertyType(NewProperty(trueFalse, LoopAlias, "Loop", ContentVariation.Nothing, 4), "facts", "Facts");

            // Bilden väljs en gång, men alt-texten skrivs på båda språken
            trailType.AddPropertyType(NewProperty(image, AddRegionAndMunicipality.ImageAlias, "Image", ContentVariation.Nothing, 0), "image", "Image");
            trailType.AddPropertyType(NewProperty(textstring, AddRegionAndMunicipality.ImageAltAlias, "Image alt text", ContentVariation.Culture, 1), "image", "Image");

            trailType.AddPropertyType(NewProperty(coordinate, AddRegionAndMunicipality.LatitudeAlias, "Latitude", ContentVariation.Nothing, 0), "location", "Location");
            trailType.AddPropertyType(NewProperty(coordinate, AddRegionAndMunicipality.LongitudeAlias, "Longitude", ContentVariation.Nothing, 1), "location", "Location");

            var attempt = await contentTypeService.CreateAsync(trailType, UserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not create the '{TrailTypeAlias}' content type: {attempt.Result}");
            }

            return trailType;
        }

        private PropertyType NewProperty(IDataType dataType, string alias, string name, ContentVariation variations, int sortOrder)
            => new(shortStringHelper, dataType, alias)
            {
                Name = name,
                Variations = variations,
                SortOrder = sortOrder,
            };

        private async Task EnsureAllowedChildAsync(string parentAlias, IContentType childType)
        {
            var parentType = contentTypeService.Get(parentAlias);

            if (parentType == null)
            {
                Logger.LogWarning("The '{Parent}' content type does not exist, so '{Child}' was not allowed under it.", parentAlias, childType.Alias);
                return;
            }

            var allowed = parentType.AllowedContentTypes?.ToList() ?? [];

            if (allowed.Any(x => x.Key == childType.Key))
            {
                return;
            }

            allowed.Add(new ContentTypeSort(childType.Key, allowed.Count, childType.Alias));
            parentType.AllowedContentTypes = allowed;

            var attempt = await contentTypeService.UpdateAsync(parentType, UserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not allow '{childType.Alias}' under '{parentAlias}': {attempt.Result}");
            }
        }

        private async Task<IDataType> GetDataTypeAsync(Guid key)
            => await dataTypeService.GetAsync(key)
                ?? throw new InvalidOperationException($"The data type {key} does not exist.");

        private async Task<IDataType> GetDataTypeAsync(string name)
            => await dataTypeService.GetAsync(name)
                ?? throw new InvalidOperationException($"The '{name}' data type does not exist.");
    }
}