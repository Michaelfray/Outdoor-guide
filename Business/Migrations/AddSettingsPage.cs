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
    /// Inställningarna för sajten: datatypen "Menu pages" (Multinode Treepicker), dokumenttypen <c>settings</c>
    /// med fältet <c>menuItems</c> för sidorna i huvudmenyn, och en publicerad Settings-sida under startsidan.
    /// Varje del kollar först om den redan finns, så inget som någon har skapat eller ändrat skrivs över.
    /// </summary>
    /// <remarks>
    /// Körs "unscoped" som <see cref="AddBaseAndStart"/>: att publicera kräver notifications, så att ModelsBuilder
    /// skapar modellen Settings. Dokumenttypen varierar inte per språk, eftersom menyn är densamma på sv och en;
    /// de valda sidorna visar ändå sitt namn på besökarens språk. Den har ingen mall, så den visas aldrig som en sida.
    /// </remarks>
    public class AddSettingsPage(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IContentTypeService contentTypeService,
        IDataTypeService dataTypeService,
        IContentService contentService,
        PropertyEditorCollection propertyEditors,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer,
        IShortStringHelper shortStringHelper)
        : UnscopedAsyncMigrationBase(context)
    {
        public const string SettingsTypeAlias = "settings";
        public const string MenuItemsPropertyAlias = "menuItems";
        public const string DataTypeName = "Menu pages";
        public const string SettingsPageName = "Settings";

        private const string MenuGroupAlias = "menu";
        private const string MenuGroupName = "Menu";

        private static readonly Guid UserKey = Constants.Security.SuperUserKey;

        protected override async Task MigrateAsync()
        {
            using (var scope = scopeProvider.CreateCoreScope())
            {
                var dataType = await EnsureDataTypeAsync();
                var settingsType = await EnsureSettingsTypeAsync(dataType);
                await EnsureSettingsAllowedUnderStartAsync(settingsType);
                EnsureSettingsPage(settingsType);

                scope.Complete();
            }

            // Unscoped migrations måste själva säga att de är klara, då sparas planens nya läge
            Context.Complete();
        }

        private async Task<IDataType> EnsureDataTypeAsync()
        {
            var existing = await dataTypeService.GetAsync(DataTypeName);

            if (existing != null)
            {
                return existing;
            }

            if (!propertyEditors.TryGet(Constants.PropertyEditors.Aliases.MultiNodeTreePicker, out var treePicker))
            {
                throw new InvalidOperationException("The Multinode Treepicker property editor is not available.");
            }

            var dataType = new DataType(treePicker, configurationEditorJsonSerializer)
            {
                Name = DataTypeName,
                EditorUiAlias = "Umb.PropertyEditorUi.ContentPicker",
                DatabaseType = ValueStorageType.Ntext,
                ConfigurationData = new Dictionary<string, object>
                {
                    // Sidor från innehållsroten; inget maxantal, så värdet blir en lista med sidor
                    ["startNode"] = new Dictionary<string, object> { ["type"] = "content" },
                    ["minNumber"] = 0,
                    ["maxNumber"] = 0,
                },
            };

            var attempt = await dataTypeService.CreateAsync(dataType, UserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not create the '{DataTypeName}' data type: {attempt.Status}");
            }

            return attempt.Result;
        }

        private async Task<IContentType> EnsureSettingsTypeAsync(IDataType menuPagesDataType)
        {
            var settingsType = contentTypeService.Get(SettingsTypeAlias);

            if (settingsType == null)
            {
                settingsType = new ContentType(shortStringHelper, Constants.System.Root)
                {
                    Alias = SettingsTypeAlias,
                    Name = "Settings",
                    Icon = "icon-settings",
                    Variations = ContentVariation.Nothing,
                };
                settingsType.AddPropertyType(NewMenuItemsProperty(menuPagesDataType), MenuGroupAlias, MenuGroupName);

                var created = await contentTypeService.CreateAsync(settingsType, UserKey);

                if (!created.Success)
                {
                    throw new InvalidOperationException($"Could not create the '{SettingsTypeAlias}' content type: {created.Result}");
                }

                return settingsType;
            }

            // Finns dokumenttypen redan (skapad i backoffice), lägg bara till fältet om det saknas
            if (!settingsType.PropertyTypeExists(MenuItemsPropertyAlias))
            {
                settingsType.AddPropertyType(NewMenuItemsProperty(menuPagesDataType), MenuGroupAlias, MenuGroupName);

                var attempt = await contentTypeService.UpdateAsync(settingsType, UserKey);

                if (!attempt.Success)
                {
                    throw new InvalidOperationException($"Could not add '{MenuItemsPropertyAlias}' to '{SettingsTypeAlias}': {attempt.Result}");
                }
            }

            return settingsType;
        }

        private PropertyType NewMenuItemsProperty(IDataType menuPagesDataType)
            => new(shortStringHelper, menuPagesDataType, MenuItemsPropertyAlias)
            {
                Name = "Menu items",
                Description = "The pages in the menu at the top of every page, in this order.",
                Variations = ContentVariation.Nothing,
            };

        private async Task EnsureSettingsAllowedUnderStartAsync(IContentType settingsType)
        {
            var startType = contentTypeService.Get(AddBaseAndStart.StartTypeAlias);
            var allowed = startType?.AllowedContentTypes?.ToList() ?? [];

            if (startType == null || allowed.Any(x => x.Key == settingsType.Key))
            {
                return;
            }

            allowed.Add(new ContentTypeSort(settingsType.Key, allowed.Count, settingsType.Alias));
            startType.AllowedContentTypes = allowed;

            var attempt = await contentTypeService.UpdateAsync(startType, UserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not allow '{SettingsTypeAlias}' under '{AddBaseAndStart.StartTypeAlias}': {attempt.Result}");
            }
        }

        private void EnsureSettingsPage(IContentType settingsType)
        {
            var start = contentService.GetRootContent().FirstOrDefault(x => x.ContentType.Alias == AddBaseAndStart.StartTypeAlias);

            if (start == null)
            {
                Logger.LogWarning("No start page found, so the {Page} page was not created.", SettingsPageName);
                return;
            }

            var hasSettings = contentService
                .GetPagedChildren(start.Id, 0, int.MaxValue, out _, propertyAliases: null, filter: null, ordering: null)
                .Any(x => x.ContentType.Alias == SettingsTypeAlias);

            if (hasSettings)
            {
                return;
            }

            // Publiceras med tom meny; redaktören väljer sidorna
            var settings = contentService.Create(SettingsPageName, start.Key, settingsType.Alias);

            var saved = contentService.Save(settings);

            if (!saved.Success)
            {
                throw new InvalidOperationException($"Could not save the {SettingsPageName} page: {saved.Result}");
            }

            var published = contentService.Publish(settings, ["*"]);

            if (!published.Success)
            {
                throw new InvalidOperationException($"Could not publish the {SettingsPageName} page: {published.Result}");
            }
        }
    }
}