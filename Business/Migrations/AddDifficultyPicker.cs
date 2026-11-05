using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>
    /// Låter datatypen "Difficulty" använda vår egen svårighetsväljare (OUT-76, OUT-81, OUT-82) i stället för
    /// Umbracos radioknappslista. Bara utseendet i backoffice byts: datatypen bygger fortfarande på
    /// <c>Umbraco.RadioButtonList</c> och sparar samma värden (easy, medium, hard), så inga leder behöver
    /// sparas om och modellerna i <c>umbraco/models</c> ändras inte.
    /// </summary>
    /// <remarks>
    /// Väljaren finns i <c>App_Plugins/DifficultyPicker</c>. Har någon redan valt en annan editor för datatypen
    /// i backoffice rör steget ingenting.
    /// </remarks>
    public class AddDifficultyPicker(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDataTypeService dataTypeService)
        : UnscopedAsyncMigrationBase(context)
    {
        public const string EditorUiAlias = "OutdoorGuide.PropertyEditorUi.DifficultyPicker";

        private const string RadioButtonListUiAlias = "Umb.PropertyEditorUi.RadioButtonList";

        private static readonly Guid UserKey = Constants.Security.SuperUserKey;

        protected override async Task MigrateAsync()
        {
            using (var scope = scopeProvider.CreateCoreScope())
            {
                await UsePickerAsync();

                scope.Complete();
            }

            // Unscoped migrations måste själva säga att de är klara, då sparas planens nya läge
            Context.Complete();
        }

        private async Task UsePickerAsync()
        {
            var dataType = await dataTypeService.GetAsync(AddDifficultyDataType.DataTypeName);

            if (dataType == null)
            {
                Logger.LogWarning("The '{Name}' data type does not exist, so the difficulty picker was not set.", AddDifficultyDataType.DataTypeName);
                return;
            }

            // Redan bytt, eller ändrat till något annat i backoffice: skriv inte över
            if (dataType.EditorUiAlias != RadioButtonListUiAlias)
            {
                return;
            }

            dataType.EditorUiAlias = EditorUiAlias;

            var attempt = await dataTypeService.UpdateAsync(dataType, UserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not set the difficulty picker on the '{dataType.Name}' data type: {attempt.Status}");
            }
        }
    }
}
