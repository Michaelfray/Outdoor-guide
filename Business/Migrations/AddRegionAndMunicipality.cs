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
    /// Sidträdet Start → Region → Kommun: datatypen "Coordinate" (Decimal), mallarna Region och Municipality,
    /// dokumenttyperna <c>region</c> och <c>municipality</c> med Base som composition, och att Start tillåter Region
    /// och Region tillåter Kommun som barn.
    /// Varje del kollar först om den redan finns, så inget som någon har skapat eller ändrat skrivs över.
    /// </summary>
    /// <remarks>
    /// Textfälten varierar per språk. Bild och koordinater gör det inte, så redaktören fyller bara i dem en gång (OUT-6).
    /// Koordinaterna är Decimal, så att de kan skickas direkt till <c>WeatherService</c>.
    /// </remarks>
    public class AddRegionAndMunicipality(
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
        public const string RegionTypeAlias = "region";
        public const string MunicipalityTypeAlias = "municipality";
        public const string CoordinateDataTypeName = "Coordinate";

        public const string IntroAlias = "intro";
        public const string DescriptionAlias = "description";
        public const string ImageAlias = "image";
        public const string ImageAltAlias = "imageAlt";
        public const string LatitudeAlias = "latitude";
        public const string LongitudeAlias = "longitude";

        private static readonly Guid UserKey = Constants.Security.SuperUserKey;

        protected override async Task MigrateAsync()
        {
            using (var scope = scopeProvider.CreateCoreScope())
            {
                var coordinate = await EnsureCoordinateDataTypeAsync();

                var regionTemplate = await EnsureTemplateAsync("Region", "Region", "Region.cshtml");
                var municipalityTemplate = await EnsureTemplateAsync("Municipality", "Municipality", "Municipality.cshtml");

                var baseType = contentTypeService.Get(AddBaseAndStart.BaseTypeAlias)
                    ?? throw new InvalidOperationException($"The '{AddBaseAndStart.BaseTypeAlias}' content type does not exist.");

                var regionType = await EnsureRegionTypeAsync(baseType, regionTemplate);
                var municipalityType = await EnsureMunicipalityTypeAsync(baseType, municipalityTemplate, coordinate);

                await EnsureAllowedChildAsync(AddBaseAndStart.StartTypeAlias, regionType);
                await EnsureAllowedChildAsync(RegionTypeAlias, municipalityType);

                scope.Complete();
            }

            // Unscoped migrations måste själva säga att de är klara, då sparas planens nya läge
            Context.Complete();
        }

        private async Task<IDataType> EnsureCoordinateDataTypeAsync()
        {
            var existing = await dataTypeService.GetAsync(CoordinateDataTypeName);

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
                Name = CoordinateDataTypeName,
                EditorUiAlias = "Umb.PropertyEditorUi.Decimal",
                DatabaseType = ValueStorageType.Decimal,
                ConfigurationData = new Dictionary<string, object>
                {
                    // Fyra decimaler räcker, som i kommuner.csv (ungefär 10 meter)
                    ["step"] = 0.0001,
                },
            };

            var attempt = await dataTypeService.CreateAsync(dataType, UserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not create the '{CoordinateDataTypeName}' data type: {attempt.Status}");
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

        private async Task<IContentType> EnsureRegionTypeAsync(IContentType baseType, ITemplate template)
        {
            var existing = contentTypeService.Get(RegionTypeAlias);

            if (existing != null)
            {
                return existing;
            }

            var textarea = await GetDataTypeAsync(Constants.DataTypes.Guids.TextareaGuid);
            var richText = await GetDataTypeAsync(Constants.DataTypes.Guids.RichtextEditorGuid);

            var regionType = NewPageType(RegionTypeAlias, "Region", "icon-map", template);
            regionType.AddContentType(baseType);

            regionType.AddPropertyType(NewProperty(textarea, IntroAlias, "Intro", ContentVariation.Culture, 0), "content", "Content");
            regionType.AddPropertyType(NewProperty(richText, DescriptionAlias, "Description", ContentVariation.Culture, 1), "content", "Content");

            await CreateAsync(regionType);

            return regionType;
        }

        private async Task<IContentType> EnsureMunicipalityTypeAsync(IContentType baseType, ITemplate template, IDataType coordinate)
        {
            var existing = contentTypeService.Get(MunicipalityTypeAlias);

            if (existing != null)
            {
                return existing;
            }

            var textarea = await GetDataTypeAsync(Constants.DataTypes.Guids.TextareaGuid);
            var richText = await GetDataTypeAsync(Constants.DataTypes.Guids.RichtextEditorGuid);
            var image = await dataTypeService.GetAsync("Image Media Picker") ?? throw new InvalidOperationException("The 'Image Media Picker' data type does not exist.");
            var textstring = await GetDataTypeAsync(Constants.DataTypes.Guids.TextstringGuid);

            var municipalityType = NewPageType(MunicipalityTypeAlias, "Municipality", "icon-map-location", template);
            municipalityType.AddContentType(baseType);

            municipalityType.AddPropertyType(NewProperty(textarea, IntroAlias, "Intro", ContentVariation.Culture, 0), "content", "Content");
            municipalityType.AddPropertyType(NewProperty(richText, DescriptionAlias, "Description", ContentVariation.Culture, 1), "content", "Content");

            // Bilden väljs en gång, men alt-texten skrivs på båda språken
            municipalityType.AddPropertyType(NewProperty(image, ImageAlias, "Image", ContentVariation.Nothing, 0), "image", "Image");
            municipalityType.AddPropertyType(NewProperty(textstring, ImageAltAlias, "Image alt text", ContentVariation.Culture, 1), "image", "Image");

            municipalityType.AddPropertyType(NewProperty(coordinate, LatitudeAlias, "Latitude", ContentVariation.Nothing, 0), "location", "Location");
            municipalityType.AddPropertyType(NewProperty(coordinate, LongitudeAlias, "Longitude", ContentVariation.Nothing, 1), "location", "Location");

            await CreateAsync(municipalityType);

            return municipalityType;
        }

        private ContentType NewPageType(string alias, string name, string icon, ITemplate template)
        {
            // Inte tillåten i roten: Region ligger under Start, och Kommun under Region
            var type = new ContentType(shortStringHelper, Constants.System.Root)
            {
                Alias = alias,
                Name = name,
                Icon = icon,
                Variations = ContentVariation.Culture,
                AllowedTemplates = [template],
            };
            type.SetDefaultTemplate(template);

            return type;
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

        private async Task CreateAsync(IContentType contentType)
        {
            var attempt = await contentTypeService.CreateAsync(contentType, UserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not create the '{contentType.Alias}' content type: {attempt.Result}");
            }
        }
    }
}