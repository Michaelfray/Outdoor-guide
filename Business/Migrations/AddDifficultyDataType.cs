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
    /// Datatypen "Difficulty": en radioknappslista med de neutrala värdena i <see cref="DifficultyValues"/>
    /// (easy, medium, hard). Används av fältet svårighet på Led (OUT-9).
    /// Finns datatypen redan skapas ingenting, så inget som någon har ändrat skrivs över.
    /// </summary>
    /// <remarks>
    /// Värdena är neutrala och varierar inte per språk. Etiketterna på sajten kommer från dictionary items
    /// (<c>Trail.Difficulty.Easy</c> och så vidare). Byts mot en egen property i byggvecka 3.
    /// </remarks>
    public class AddDifficultyDataType(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDataTypeService dataTypeService,
        PropertyEditorCollection propertyEditors,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : UnscopedAsyncMigrationBase(context)
    {
        public const string DataTypeName = "Difficulty";

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
                    ["items"] = DifficultyValues.All.ToArray(),
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