using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>
    /// Fältet <c>activity</c> på Led, i gruppen Facts efter Loop (OUT-31). Använder datatypen från
    /// <see cref="AddActivityDataType"/>. Finns fältet redan skapas ingenting.
    /// </summary>
    /// <remarks>
    /// <see cref="AddTrail"/> finns redan på main och ändras aldrig, därför läggs fältet till i ett eget steg.
    /// Fältet varierar inte per språk, precis som <c>difficulty</c>: redaktören väljer aktivitet en gång.
    /// </remarks>
    public class AddTrailActivity(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IContentTypeService contentTypeService,
        IDataTypeService dataTypeService,
        IShortStringHelper shortStringHelper)
        : UnscopedAsyncMigrationBase(context)
    {
        public const string ActivityAlias = "activity";

        private static readonly Guid UserKey = Constants.Security.SuperUserKey;

        protected override async Task MigrateAsync()
        {
            using (var scope = scopeProvider.CreateCoreScope())
            {
                await EnsureActivityPropertyAsync();

                scope.Complete();
            }

            // Unscoped migrations måste själva säga att de är klara, då sparas planens nya läge
            Context.Complete();
        }

        private async Task EnsureActivityPropertyAsync()
        {
            var trailType = contentTypeService.Get(AddTrail.TrailTypeAlias)
                ?? throw new InvalidOperationException($"The '{AddTrail.TrailTypeAlias}' content type does not exist.");

            if (trailType.PropertyTypeExists(ActivityAlias))
            {
                return;
            }

            var activity = await dataTypeService.GetAsync(AddActivityDataType.DataTypeName)
                ?? throw new InvalidOperationException($"The '{AddActivityDataType.DataTypeName}' data type does not exist.");

            var property = new PropertyType(shortStringHelper, activity, ActivityAlias)
            {
                Name = "Activity",
                Variations = ContentVariation.Nothing,
                // Efter length, duration, difficulty, childFriendly och loop (0–4) i AddTrail
                SortOrder = 5,
            };

            trailType.AddPropertyType(property, "facts", "Facts");

            var attempt = await contentTypeService.UpdateAsync(trailType, UserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not add '{ActivityAlias}' to '{AddTrail.TrailTypeAlias}': {attempt.Result}");
            }
        }
    }
}
