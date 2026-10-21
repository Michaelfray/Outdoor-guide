using OutdoorGuide.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>
    /// Datatypen "Activity": en radioknappslista med de neutrala värdena i <see cref="ActivityValues"/>
    /// (hiking, paddling, cycling, winter). Används av fältet aktivitet på Led (OUT-31).
    /// Finns datatypen redan skapas ingenting, så inget som någon har ändrat skrivs över.
    /// </summary>
    /// <remarks>
    /// Byggd som <see cref="AddDifficultyDataType"/>. Etiketterna på sajten kommer från dictionary items
    /// (<c>Trail.Activity.Hiking</c> och så vidare).
    /// </remarks>
    public class AddActivityDataType(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDataTypeService dataTypeService,
        PropertyEditorCollection propertyEditors,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : UnscopedAsyncMigrationBase(context)
    {
        public const string DataTypeName = "Activity";

        private static readonly Guid UserKey = Constants.Security.SuperUserKey;

        protected override async Task MigrateAsync()
        {
            using (var scope = scopeProvider.CreateCoreScope())
            {
                await EnsureDataTypeAsync();

                scope.Complete();
            }

            // Unscoped migrations måste själva säga att de är klara, då sparas planens nya läge
            Context.Complete();
        }

        private async Task EnsureDataTypeAsync()
        {
            if (await dataTypeService.GetAsync(DataTypeName) != null)
            {
                return;
            }

            if (!propertyEditors.TryGet(Constants.PropertyEditors.Aliases.RadioButtonList, out var radioButtonList))
            {
                throw new InvalidOperationException("The Radio Button List property editor is not available.");
            }

            var dataType = new DataType(radioButtonList, configurationEditorJsonSerializer)
            {
                Name = DataTypeName,
                EditorUiAlias = "Umb.PropertyEditorUi.RadioButtonList",
                DatabaseType = ValueStorageType.Nvarchar,
                ConfigurationData = new Dictionary<string, object>
                {
                    ["items"] = ActivityValues.All.ToArray(),
                },
            };

            var attempt = await dataTypeService.CreateAsync(dataType, UserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not create the '{DataTypeName}' data type: {attempt.Status}");
            }
        }
    }
}
