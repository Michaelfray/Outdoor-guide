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
    /// Dokumenttyperna för menyns sidor (OUT-32): <c>trailsPage</c> (Leder), <c>activityPage</c> (Vandring, Paddling,
    /// Cykel, Vinter), <c>trailCategory</c> (undersidorna, till exempel Dagsturer), <c>placesPage</c> (Platser) och
    /// <c>textPage</c> (Säsongstips, Om oss, Kontakt), med mallar, datatypen "Trail picker" och vilka typer som får
    /// ligga var. Varje del kollar först om den redan finns, så inget som någon har skapat eller ändrat skrivs över.
    /// </summary>
    /// <remarks>
    /// En typ per sorts sida, inte en per sida (beslut 21/10): de fyra aktivitetssidorna skiljer sig bara i fältet
    /// <c>activity</c>, och de tre textsidorna har samma fält, som Carls <c>Article</c>. Sidorna skapas i
    /// <see cref="AddMenuContent"/>. Trädet blir Start → Leder → aktivitetssida → undersida, och Start → Platser och
    /// textsidorna.
    /// </remarks>
    public class AddMenuPages(
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
        public const string TrailsPageTypeAlias = "trailsPage";
        public const string ActivityPageTypeAlias = "activityPage";
        public const string TrailCategoryTypeAlias = "trailCategory";
        public const string PlacesPageTypeAlias = "placesPage";
        public const string TextPageTypeAlias = "textPage";

        public const string TrailPickerDataTypeName = "Trail picker";

        /// <summary>Lederna som en undersida visar, valda av redaktören (beslut 21/10, OUT-29).</summary>
        public const string TrailsAlias = "trails";

        private static readonly Guid UserKey = Constants.Security.SuperUserKey;

        protected override async Task MigrateAsync()
        {
            using (var scope = scopeProvider.CreateCoreScope())
            {
                var baseType = contentTypeService.Get(AddBaseAndStart.BaseTypeAlias)
                    ?? throw new InvalidOperationException($"The '{AddBaseAndStart.BaseTypeAlias}' content type does not exist.");

                var textarea = await GetDataTypeAsync(Constants.DataTypes.Guids.TextareaGuid);
                var richText = await GetDataTypeAsync(Constants.DataTypes.Guids.RichtextEditorGuid);
                var activity = await GetDataTypeAsync(AddActivityDataType.DataTypeName);
                var trailPicker = await EnsureTrailPickerDataTypeAsync();

                // Barnen skapas före föräldrarna, så att föräldern kan tillåta dem direkt efteråt
                var textPage = await EnsurePageTypeAsync(TextPageTypeAlias, "Text page", "icon-document", "TextPage", baseType, type =>
                {
                    type.AddPropertyType(NewProperty(textarea, AddRegionAndMunicipality.IntroAlias, "Intro", 0), "content", "Content");
                    type.AddPropertyType(NewProperty(richText, AddRegionAndMunicipality.DescriptionAlias, "Description", 1), "content", "Content");
                });

                var placesPage = await EnsurePageTypeAsync(PlacesPageTypeAlias, "Places page", "icon-globe", "PlacesPage", baseType, type =>
                {
                    type.AddPropertyType(NewProperty(textarea, AddRegionAndMunicipality.IntroAlias, "Intro", 0), "content", "Content");
                });

                var trailCategory = await EnsurePageTypeAsync(TrailCategoryTypeAlias, "Trail category", "icon-bulleted-list", "TrailCategory", baseType, type =>
                {
                    type.AddPropertyType(NewProperty(textarea, AddRegionAndMunicipality.IntroAlias, "Intro", 0), "content", "Content");

                    // Samma leder på båda språken, så redaktören väljer dem en gång
                    type.AddPropertyType(NewProperty(trailPicker, TrailsAlias, "Trails", 1, ContentVariation.Nothing), "content", "Content");
                });

                var activityPage = await EnsurePageTypeAsync(ActivityPageTypeAlias, "Activity page", "icon-tag", "ActivityPage", baseType, type =>
                {
                    type.AddPropertyType(NewProperty(textarea, AddRegionAndMunicipality.IntroAlias, "Intro", 0), "content", "Content");

                    // Vilken aktivitet sidan gäller, med samma värden som fältet på Led (OUT-31)
                    type.AddPropertyType(NewProperty(activity, AddTrailActivity.ActivityAlias, "Activity", 1, ContentVariation.Nothing), "content", "Content");
                });

                var trailsPage = await EnsurePageTypeAsync(TrailsPageTypeAlias, "Trails page", "icon-directions", "TrailsPage", baseType, type =>
                {
                    type.AddPropertyType(NewProperty(textarea, AddRegionAndMunicipality.IntroAlias, "Intro", 0), "content", "Content");
                });

                await EnsureAllowedChildAsync(AddBaseAndStart.StartTypeAlias, trailsPage);
                await EnsureAllowedChildAsync(AddBaseAndStart.StartTypeAlias, placesPage);
                await EnsureAllowedChildAsync(AddBaseAndStart.StartTypeAlias, textPage);
                await EnsureAllowedChildAsync(TrailsPageTypeAlias, activityPage);
                await EnsureAllowedChildAsync(ActivityPageTypeAlias, trailCategory);

                scope.Complete();
            }

            // Unscoped migrations måste själva säga att de är klara, då sparas planens nya läge
            Context.Complete();
        }

        private async Task<IDataType> EnsureTrailPickerDataTypeAsync()
        {
            var existing = await dataTypeService.GetAsync(TrailPickerDataTypeName);

            if (existing != null)
            {
                return existing;
            }

            if (!propertyEditors.TryGet(Constants.PropertyEditors.Aliases.MultiNodeTreePicker, out var treePicker))
            {
                throw new InvalidOperationException("The Multinode Treepicker property editor is not available.");
            }

            var trailType = contentTypeService.Get(AddTrail.TrailTypeAlias)
                ?? throw new InvalidOperationException($"The '{AddTrail.TrailTypeAlias}' content type does not exist.");

            var dataType = new DataType(treePicker, configurationEditorJsonSerializer)
            {
                Name = TrailPickerDataTypeName,
                EditorUiAlias = "Umb.PropertyEditorUi.ContentPicker",
                DatabaseType = ValueStorageType.Ntext,
                ConfigurationData = new Dictionary<string, object>
                {
                    // Som "Menu pages" i AddSettingsPage, men bara leder går att välja.
                    // Filtret är nycklarna (Key) till de tillåtna dokumenttyperna, inte deras alias.
                    ["startNode"] = new Dictionary<string, object> { ["type"] = "content" },
                    ["minNumber"] = 0,
                    ["maxNumber"] = 0,
                    ["filter"] = trailType.Key.ToString(),
                },
            };

            var attempt = await dataTypeService.CreateAsync(dataType, UserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not create the '{TrailPickerDataTypeName}' data type: {attempt.Status}");
            }

            return attempt.Result;
        }

        private async Task<IContentType> EnsurePageTypeAsync(string alias, string name, string icon, string templateAlias, IContentType baseType, Action<ContentType> addProperties)
        {
            var existing = contentTypeService.Get(alias);

            if (existing != null)
            {
                return existing;
            }

            var template = await EnsureTemplateAsync(templateAlias);

            // Inte tillåten i roten: sidorna ligger under Start, Leder eller en aktivitetssida
            var type = new ContentType(shortStringHelper, Constants.System.Root)
            {
                Alias = alias,
                Name = name,
                Icon = icon,
                Variations = ContentVariation.Culture,
                AllowedTemplates = [template],
            };
            type.SetDefaultTemplate(template);
            type.AddContentType(baseType);
            addProperties(type);

            var attempt = await contentTypeService.CreateAsync(type, UserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not create the '{alias}' content type: {attempt.Result}");
            }

            return type;
        }

        private async Task<ITemplate> EnsureTemplateAsync(string alias)
        {
            var existing = await templateService.GetAsync(alias);

            if (existing != null)
            {
                return existing;
            }

            // Att skapa en mall skriver (om) vyfilen, så skicka med vyn som redan ligger i repot
            var viewPath = Path.Combine(hostEnvironment.ContentRootPath, "Views", $"{alias}.cshtml");
            var content = System.IO.File.Exists(viewPath) ? await System.IO.File.ReadAllTextAsync(viewPath) : null;

            var attempt = await templateService.CreateAsync(alias, alias, content, UserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not create the '{alias}' template: {attempt.Status}");
            }

            return attempt.Result;
        }

        /// <summary>Ett fält som varierar per språk, om inget annat sägs.</summary>
        private PropertyType NewProperty(IDataType dataType, string alias, string name, int sortOrder, ContentVariation variations = ContentVariation.Culture)
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
