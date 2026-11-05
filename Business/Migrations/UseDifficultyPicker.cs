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
	/// Datatypen "Difficulty Picker" och bytet av fältet svårighet på Led till den (OUT-81).
	/// Den gamla datatypen "Difficulty" lämnas kvar. Fältet byts bara om det fortfarande använder den,
	/// så en datatyp som någon har valt i backoffice skrivs inte över.
	/// </summary>
	public class UseDifficultyPicker(
		IMigrationContext context,
		ICoreScopeProvider scopeProvider,
		IContentTypeService contentTypeService,
		IDataTypeService dataTypeService,
		PropertyEditorCollection propertyEditors,
		IConfigurationEditorJsonSerializer configurationEditorJsonSerializer,
		ILogger<UseDifficultyPicker> logger)
		: UnscopedAsyncMigrationBase(context)
	{
		public const string DataTypeName = "Difficulty Picker";

		private static readonly Guid UserKey = Constants.Security.SuperUserKey;

		protected override async Task MigrateAsync()
		{
			using (var scope = scopeProvider.CreateCoreScope())
			{
				var picker = await EnsureDataTypeAsync();
				await EnsureTrailUsesPickerAsync(picker);

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

			if (!propertyEditors.TryGet(Constants.PropertyEditors.Aliases.PlainString, out var plainString))
			{
				throw new InvalidOperationException("The Plain String property editor is not available.");
			}

			var dataType = new DataType(plainString, configurationEditorJsonSerializer)
			{
				Name = DataTypeName,
				EditorUiAlias = "OutdoorGuide.PropertyEditorUi.DifficultyPicker",
				DatabaseType = ValueStorageType.Nvarchar,
			};

			var attempt = await dataTypeService.CreateAsync(dataType, UserKey);

			if (!attempt.Success)
			{
				throw new InvalidOperationException($"Could not create the '{DataTypeName}' data type: {attempt.Status}");
			}

			return attempt.Result;
		}

		private async Task EnsureTrailUsesPickerAsync(IDataType picker)
		{
			var trailType = contentTypeService.Get(AddTrail.TrailTypeAlias)
				?? throw new InvalidOperationException($"The '{AddTrail.TrailTypeAlias}' content type does not exist.");

			var property = trailType.PropertyTypes.First(x => x.Alias == AddTrail.DifficultyAlias);
			var original = await dataTypeService.GetAsync(AddDifficultyDataType.DataTypeName);

			if (property.DataTypeKey == picker.Key)
			{
				return;
			}

			if (original is null || property.DataTypeKey != original.Key)
			{
				logger.LogWarning(
					"The '{Property}' property on '{ContentType}' uses another data type than '{DataType}', so it was not changed.",
					AddTrail.DifficultyAlias,
					AddTrail.TrailTypeAlias,
					AddDifficultyDataType.DataTypeName);

				return;
			}

			property.DataTypeId = picker.Id;
			property.DataTypeKey = picker.Key;

			var attempt = await contentTypeService.UpdateAsync(trailType, UserKey);

			if (!attempt.Success)
			{
				throw new InvalidOperationException($"Could not change '{AddTrail.DifficultyAlias}' on '{AddTrail.TrailTypeAlias}': {attempt.Result}");
			}
		}
	}
}
