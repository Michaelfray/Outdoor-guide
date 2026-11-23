using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>
    /// Nyhetsbreven (OUT-115): dokumenttypen <c>newsletter</c> med ämne och text per språk, mappen
    /// <c>newsletterFolder</c> som bara tillåter nyhetsbrev, och en publicerad mapp under startsidan.
    /// Varje del kollar först om den redan finns, så inget som någon har skapat eller ändrat skrivs över.
    /// </summary>
    /// <remarks>
    /// Som <see cref="AddSettingsPage"/>: ingen mall och inte Base, så typerna syns inte som sidor, i sitemapen
    /// (bara IBase) eller i sökningen (bara Trail och Municipality). Menyn byggs från Settings.
    /// </remarks>
    public class AddNewsletterType(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IContentTypeService contentTypeService,
        IDataTypeService dataTypeService,
        IContentService contentService,
        IShortStringHelper shortStringHelper)
        : UnscopedAsyncMigrationBase(context)
    {
        public const string NewsletterTypeAlias = "newsletter";
        public const string FolderTypeAlias = "newsletterFolder";
        public const string FolderName = "Newsletters";

        private const string ContentGroupAlias = "content";
        private const string ContentGroupName = "Content";

        private static readonly Guid UserKey = Constants.Security.SuperUserKey;

        protected override async Task MigrateAsync()
        {
            using (var scope = scopeProvider.CreateCoreScope())
            {
                var newsletterType = await EnsureNewsletterTypeAsync();
                var folderType = await EnsureFolderTypeAsync(newsletterType);
                await EnsureFolderAllowedUnderStartAsync(folderType);
                EnsureFolder(folderType);

                scope.Complete();
            }

            // Unscoped migrations måste själva säga att de är klara, då sparas planens nya läge
            Context.Complete();
        }

        private async Task<IContentType> EnsureNewsletterTypeAsync()
        {
            var textstring = await GetDataTypeAsync(Constants.DataTypes.Guids.TextstringGuid);
            var richText = await GetDataTypeAsync(Constants.DataTypes.Guids.RichtextEditorGuid);

            var newsletterType = contentTypeService.Get(NewsletterTypeAlias);
            var isNew = newsletterType == null;

            newsletterType ??= new ContentType(shortStringHelper, Constants.System.Root)
            {
                Alias = NewsletterTypeAlias,
                Name = "Newsletter",
                Icon = "icon-message",
                Variations = ContentVariation.Culture,
            };

            // Finns typen redan (skapad i backoffice) läggs bara de fält till som saknas
            var changed = AddIfMissing(newsletterType, new PropertyType(shortStringHelper, textstring, "subject")
            {
                Name = "Subject",
                Description = "The subject line of the email.",
                Mandatory = true,
                Variations = ContentVariation.Culture,
            });
            changed |= AddIfMissing(newsletterType, new PropertyType(shortStringHelper, richText, "body")
            {
                Name = "Body",
                Description = "The text of the newsletter.",
                Variations = ContentVariation.Culture,
            });

            if (isNew)
            {
                await CreateAsync(newsletterType);
            }
            else if (changed)
            {
                await UpdateAsync(newsletterType);
            }

            return newsletterType;
        }

        private async Task<IContentType> EnsureFolderTypeAsync(IContentType newsletterType)
        {
            var folderType = contentTypeService.Get(FolderTypeAlias);

            if (folderType == null)
            {
                folderType = new ContentType(shortStringHelper, Constants.System.Root)
                {
                    Alias = FolderTypeAlias,
                    Name = "Newsletter folder",
                    Icon = "icon-folder",
                    Variations = ContentVariation.Nothing,
                    AllowedContentTypes = [new ContentTypeSort(newsletterType.Key, 0, newsletterType.Alias)],
                };

                await CreateAsync(folderType);
                return folderType;
            }

            var allowed = folderType.AllowedContentTypes?.ToList() ?? [];

            if (allowed.All(x => x.Key != newsletterType.Key))
            {
                allowed.Add(new ContentTypeSort(newsletterType.Key, allowed.Count, newsletterType.Alias));
                folderType.AllowedContentTypes = allowed;
                await UpdateAsync(folderType);
            }

            return folderType;
        }

        private async Task EnsureFolderAllowedUnderStartAsync(IContentType folderType)
        {
            var startType = contentTypeService.Get(AddBaseAndStart.StartTypeAlias);
            var allowed = startType?.AllowedContentTypes?.ToList() ?? [];

            if (startType == null || allowed.Any(x => x.Key == folderType.Key))
            {
                return;
            }

            allowed.Add(new ContentTypeSort(folderType.Key, allowed.Count, folderType.Alias));
            startType.AllowedContentTypes = allowed;
            await UpdateAsync(startType);
        }

        private void EnsureFolder(IContentType folderType)
        {
            var start = contentService.GetRootContent().FirstOrDefault(x => x.ContentType.Alias == AddBaseAndStart.StartTypeAlias);

            if (start == null)
            {
                Logger.LogWarning("No start page found, so the {Page} folder was not created.", FolderName);
                return;
            }

            var hasFolder = contentService
                .GetPagedChildren(start.Id, 0, int.MaxValue, out _, propertyAliases: null, filter: null, ordering: null)
                .Any(x => x.ContentType.Alias == FolderTypeAlias);

            if (hasFolder)
            {
                return;
            }

            var folder = contentService.Create(FolderName, start.Key, folderType.Alias);

            var saved = contentService.Save(folder);

            if (!saved.Success)
            {
                throw new InvalidOperationException($"Could not save the {FolderName} folder: {saved.Result}");
            }

            var published = contentService.Publish(folder, ["*"]);

            if (!published.Success)
            {
                throw new InvalidOperationException($"Could not publish the {FolderName} folder: {published.Result}");
            }
        }

        private static bool AddIfMissing(IContentType contentType, PropertyType property)
        {
            if (contentType.PropertyTypeExists(property.Alias))
            {
                return false;
            }

            contentType.AddPropertyType(property, ContentGroupAlias, ContentGroupName);
            return true;
        }

        private async Task<IDataType> GetDataTypeAsync(Guid key)
            => await dataTypeService.GetAsync(key)
                ?? throw new InvalidOperationException($"The data type {key} does not exist.");

        private async Task CreateAsync(IContentType contentType)
        {
            var attempt = await contentTypeService.CreateAsync(contentType, UserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not create the '{contentType.Alias}' content type: {attempt.Result}");
            }
        }

        private async Task UpdateAsync(IContentType contentType)
        {
            var attempt = await contentTypeService.UpdateAsync(contentType, UserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not update the '{contentType.Alias}' content type: {attempt.Result}");
            }
        }
    }
}