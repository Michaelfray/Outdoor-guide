using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>
    /// Adds the field <c>intro</c> (Textarea, varies by culture) to the document type <c>start</c>,
    /// right after Title and without a tab, as it was created in the backoffice.
    /// Nothing is overwritten: if the field already exists, the step does nothing.
    /// </summary>
    public class AddStartIntro(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IContentTypeService contentTypeService,
        IDataTypeService dataTypeService,
        IShortStringHelper shortStringHelper)
        : UnscopedAsyncMigrationBase(context)
    {
        public const string IntroAlias = "intro";

        private static readonly Guid UserKey = Constants.Security.SuperUserKey;

        protected override async Task MigrateAsync()
        {
            using (var scope = scopeProvider.CreateCoreScope())
            {
                await EnsureIntroAsync();

                scope.Complete();
            }

            // Unscoped migrations måste själva säga att de är klara, då sparas planens nya läge
            Context.Complete();
        }

        private async Task EnsureIntroAsync()
        {
            var startType = contentTypeService.Get(AddBaseAndStart.StartTypeAlias);

            if (startType == null)
            {
                Logger.LogWarning("The '{Alias}' content type does not exist, so '{Property}' was not added.", AddBaseAndStart.StartTypeAlias, IntroAlias);
                return;
            }

            if (startType.PropertyTypeExists(IntroAlias))
            {
                return;
            }

            var textarea = await dataTypeService.GetAsync(Constants.DataTypes.Guids.TextareaGuid)
                ?? throw new InvalidOperationException("The Textarea data type does not exist.");

            // Utan flik, efter Title
            startType.AddPropertyType(new PropertyType(shortStringHelper, textarea, IntroAlias)
            {
                Name = "Intro",
                Variations = ContentVariation.Culture,
                SortOrder = 1,
            });

            var attempt = await contentTypeService.UpdateAsync(startType, UserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not add '{IntroAlias}' to the '{startType.Alias}' content type: {attempt.Result}");
            }
        }
    }
}
