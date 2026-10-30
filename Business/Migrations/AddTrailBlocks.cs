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
    /// Adds the blocks for the trail page (OUT-49): a warning box, an image gallery and "Good to know".
    /// It also adds a Block List called "Trail blocks" and a "blocks" field on Trail.
    /// Each part checks first if it already exists, so nothing gets overwritten.
    /// </summary>
    /// <remarks>
    /// The field can be different per language, so a trail can have a warning in Swedish but not in English.
    /// Based on the teacher's AddContactFormBlock.
    /// </remarks>
    public class AddTrailBlocks(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IContentTypeService contentTypeService,
        IDataTypeService dataTypeService,
        PropertyEditorCollection propertyEditors,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer,
        IShortStringHelper shortStringHelper)
        : UnscopedAsyncMigrationBase(context)
    {
        public const string WarningBlockAlias = "warningBlock";
        public const string GalleryBlockAlias = "galleryBlock";
        public const string GoodToKnowBlockAlias = "goodToKnowBlock";

        public const string TextAlias = "text";
        public const string ImagesAlias = "images";

        public const string DataTypeName = "Trail blocks";
        public const string BlocksPropertyAlias = "blocks";

        private const string BlocksContainerName = "Blocks";
        private const string MultipleImagePickerName = "Multiple Image Media Picker";

        private static readonly Guid UserKey = Constants.Security.SuperUserKey;

        protected override async Task MigrateAsync()
        {
            // Everything runs in one scope, so if something fails nothing is saved
            using (var scope = scopeProvider.CreateCoreScope())
            {
                // Data types we already have in Umbraco
                var textarea = await GetDataTypeAsync(Constants.DataTypes.Guids.TextareaGuid);
                var images = await GetDataTypeAsync(MultipleImagePickerName);
                var container = GetBlocksContainerId();

                // The three blocks
                var warning = await EnsureElementTypeAsync(WarningBlockAlias, "Warning", "icon-alert", container, textarea, TextAlias, "Text");
                var gallery = await EnsureElementTypeAsync(GalleryBlockAlias, "Gallery", "icon-pictures-alt-2", container, images, ImagesAlias, "Images");
                var goodToKnow = await EnsureElementTypeAsync(GoodToKnowBlockAlias, "Good to know", "icon-info", container, textarea, TextAlias, "Text");

                // The block list, and the field on Trail that uses it
                var dataType = await EnsureDataTypeAsync(warning, gallery, goodToKnow);
                await EnsureBlocksOnTrailAsync(dataType);

                scope.Complete();
            }

            // Tell Umbraco this step is done, so it does not run again
            Context.Complete();
        }

        // Put the blocks in a "Blocks" folder if there is one, otherwise at the top level
        private int GetBlocksContainerId()
            => contentTypeService.GetContainers(BlocksContainerName, 1).FirstOrDefault()?.Id ?? Constants.System.Root;

        // Creates one block (an element type) with one field
        private async Task<IContentType> EnsureElementTypeAsync(
            string alias,
            string name,
            string icon,
            int containerId,
            IDataType dataType,
            string propertyAlias,
            string propertyName)
        {
            var existing = contentTypeService.Get(alias);

            // Already there? Then leave it as it is
            if (existing != null)
            {
                return existing;
            }

            var elementType = new ContentType(shortStringHelper, containerId)
            {
                Alias = alias,
                Name = name,
                Icon = icon,
                IsElement = true, // a block, not a page
                Variations = ContentVariation.Nothing,
            };

            elementType.AddPropertyType(
                new PropertyType(shortStringHelper, dataType, propertyAlias)
                {
                    Name = propertyName,
                    Mandatory = true, // an empty block makes no sense
                    SortOrder = 0,
                },
                "content",
                "Content");

            var attempt = await contentTypeService.CreateAsync(elementType, UserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not create the '{alias}' element type: {attempt.Result}");
            }

            return elementType;
        }

        // Creates the Block List that lets the editor pick between the three blocks
        private async Task<IDataType> EnsureDataTypeAsync(params IContentType[] elementTypes)
        {
            var existing = await dataTypeService.GetAsync(DataTypeName);

            if (existing != null)
            {
                return existing;
            }

            if (!propertyEditors.TryGet(Constants.PropertyEditors.Aliases.BlockList, out var blockList))
            {
                throw new InvalidOperationException("The Block List property editor is not available.");
            }

            var dataType = new DataType(blockList, configurationEditorJsonSerializer)
            {
                Name = DataTypeName,
                EditorUiAlias = "Umb.PropertyEditorUi.BlockList",
                DatabaseType = ValueStorageType.Ntext,
                ConfigurationData = new Dictionary<string, object>
                {
                    // The blocks the editor can choose from
                    ["blocks"] = elementTypes
                        .Select(x => new Dictionary<string, object> { ["contentElementTypeKey"] = x.Key })
                        .ToArray(),
                },
            };

            var attempt = await dataTypeService.CreateAsync(dataType, UserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not create the '{DataTypeName}' data type: {attempt.Status}");
            }

            return attempt.Result;
        }

        // Adds the "blocks" field to the Trail document type
        private async Task EnsureBlocksOnTrailAsync(IDataType blocksDataType)
        {
            var trailType = contentTypeService.Get(AddTrail.TrailTypeAlias)
                ?? throw new InvalidOperationException($"The '{AddTrail.TrailTypeAlias}' content type does not exist.");

            if (trailType.PropertyTypeExists(BlocksPropertyAlias))
            {
                return;
            }

            // Goes right after Description on the Content tab, like on the page ("Om leden")
            trailType.AddPropertyType(
                new PropertyType(shortStringHelper, blocksDataType, BlocksPropertyAlias)
                {
                    Name = "Blocks",
                    Variations = ContentVariation.Culture, // different per language
                    SortOrder = 2,
                },
                "content",
                "Content");

            var attempt = await contentTypeService.UpdateAsync(trailType, UserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not add '{BlocksPropertyAlias}' to '{AddTrail.TrailTypeAlias}': {attempt.Result}");
            }
        }

        // Small helpers that find a data type, or stop with a clear error
        private async Task<IDataType> GetDataTypeAsync(Guid key)
            => await dataTypeService.GetAsync(key)
                ?? throw new InvalidOperationException($"The data type {key} does not exist.");

        private async Task<IDataType> GetDataTypeAsync(string name)
            => await dataTypeService.GetAsync(name)
                ?? throw new InvalidOperationException($"The '{name}' data type does not exist.");
    }
}