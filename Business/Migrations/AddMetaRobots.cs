using OutdoorGuide.Models;
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
	
	public class AddMetaRobots(
		IMigrationContext context,
		ICoreScopeProvider scopeProvider,
		IContentTypeService contentTypeService,
		IDataTypeService dataTypeService,
		PropertyEditorCollection propertyEditors,
		IConfigurationEditorJsonSerializer configurationEditorJsonSerializer,
		IShortStringHelper shortStringHelper)
		: UnscopedAsyncMigrationBase(context)
	{
		public const string MetaRobotsAlias = "metaRobots";
		public const string DataTypeName = "Meta Robots";

		private const string SeoTabAlias = "seo";

		private static readonly Guid UserKey = Constants.Security.SuperUserKey;

		protected override async Task MigrateAsync()
		{
			using (var scope = scopeProvider.CreateCoreScope())
			{
				var dataType = await EnsureDataTypeAsync();
				await EnsureBasePropertyAsync(dataType);

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
					["items"] = MetaRobotsValues.Values.ToArray(),
				},
			};

			var attempt = await dataTypeService.CreateAsync(dataType, UserKey);

			if (!attempt.Success)
			{
				throw new InvalidOperationException($"Could not create the '{DataTypeName}' data type: {attempt.Status}");
			}

			return attempt.Result;
		}

		private async Task EnsureBasePropertyAsync(IDataType metaRobots)
		{
			var baseType = contentTypeService.Get(AddBaseAndStart.BaseTypeAlias)
				?? throw new InvalidOperationException($"The '{AddBaseAndStart.BaseTypeAlias}' content type does not exist.");

			if (baseType.PropertyTypeExists(MetaRobotsAlias))
			{
				return;
			}

			var property = new PropertyType(shortStringHelper, metaRobots, MetaRobotsAlias)
			{
				Name = "Meta Robots",
				Description = "NONE ber sökmotorer att inte indexera sidan, och sidan hoppas över i sitemapen.",
				Variations = ContentVariation.Nothing,
				SortOrder = 1,
			};

			baseType.AddPropertyType(property, SeoTabAlias);

			var attempt = await contentTypeService.UpdateAsync(baseType, UserKey);

			if (!attempt.Success)
			{
				throw new InvalidOperationException($"Could not add '{MetaRobotsAlias}' to '{AddBaseAndStart.BaseTypeAlias}': {attempt.Result}");
			}
		}
	}
}