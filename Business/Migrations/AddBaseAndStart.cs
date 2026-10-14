using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>
    /// Grunden för sajten: språken (en-US, sv-SE), mallarna root och Start, compositionen <c>base</c>
    /// (Meta Description på fliken SEO), dokumenttypen <c>start</c> (Title) och en publicerad startsida
    /// Home/Hem med domänerna /en och /.
    /// Varje del kollar först om den redan finns, så inget som någon har skapat eller ändrat skrivs över.
    /// </summary>
    /// <remarks>
    /// Körs "unscoped" som lärarens AddContactFormBlock: att skapa och publicera innehåll kräver notifications,
    /// så att URL:er, cachen och ModelsBuilder uppdateras. Allt görs ändå i ett scope, så ett fel rullar
    /// tillbaka allt och steget körs igen vid nästa start.
    /// </remarks>
    public class AddBaseAndStart(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        ILanguageService languageService,
        ITemplateService templateService,
        IContentTypeService contentTypeService,
        IDataTypeService dataTypeService,
        IContentService contentService,
        IDomainService domainService,
        IShortStringHelper shortStringHelper,
        IHostEnvironment hostEnvironment)
        : UnscopedAsyncMigrationBase(context)
    {
        public const string BaseTypeAlias = "base";
        public const string StartTypeAlias = "start";

        private const string English = "en-US";
        private const string Swedish = "sv-SE";

        private static readonly Guid UserKey = Constants.Security.SuperUserKey;

        protected override async Task MigrateAsync()
        {
            using (var scope = scopeProvider.CreateCoreScope())
            {
                await EnsureLanguageAsync(Swedish, "Swedish (Sweden)");

                // root först, eftersom Start.cshtml har root som layout
                await EnsureTemplateAsync("_Root", "root", "root.cshtml");
                var startTemplate = await EnsureTemplateAsync("Start", "Start", "Start.cshtml");

                var baseType = await EnsureBaseTypeAsync();
                var startType = await EnsureStartTypeAsync(baseType, startTemplate);
                await EnsureStartPageAsync(startType);

                scope.Complete();
            }

            // Unscoped migrations måste själva säga att de är klara, då sparas planens nya läge
            Context.Complete();
        }

        private async Task EnsureLanguageAsync(string isoCode, string cultureName)
        {
            if (await languageService.GetAsync(isoCode) != null)
            {
                return;
            }

            var attempt = await languageService.CreateAsync(new Language(isoCode, cultureName), UserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not create the language '{isoCode}': {attempt.Status}");
            }
        }

        private async Task<ITemplate> EnsureTemplateAsync(string name, string alias, string viewFile)
        {
            var existing = await templateService.GetAsync(alias);

            if (existing != null)
            {
                return existing;
            }

            // Att skapa en mall skriver (om) vyfilen, så skicka med vyn som redan ligger i repot
            var viewPath = Path.Combine(hostEnvironment.ContentRootPath, "Views", viewFile);
            var content = System.IO.File.Exists(viewPath) ? await System.IO.File.ReadAllTextAsync(viewPath) : null;

            var attempt = await templateService.CreateAsync(name, alias, content, UserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not create the '{alias}' template: {attempt.Status}");
            }

            return attempt.Result;
        }

        private async Task<IContentType> EnsureBaseTypeAsync()
        {
            var existing = contentTypeService.Get(BaseTypeAlias);

            if (existing != null)
            {
                return existing;
            }

            var textstring = await GetDataTypeAsync(Constants.DataTypes.Guids.TextstringGuid);

            var baseType = new ContentType(shortStringHelper, Constants.System.Root)
            {
                Alias = BaseTypeAlias,
                Name = "Base",
                Icon = "icon-document",
                Variations = ContentVariation.Culture,
            };

            baseType.AddPropertyGroup("seo", "SEO");
            baseType.PropertyGroups["seo"].Type = PropertyGroupType.Tab;
            baseType.AddPropertyType(
                new PropertyType(shortStringHelper, textstring, "metaDescription")
                {
                    Name = "Meta Description",
                    Variations = ContentVariation.Culture,
                },
                "seo");

            await CreateAsync(baseType);

            return baseType;
        }

        private async Task<IContentType> EnsureStartTypeAsync(IContentType baseType, ITemplate template)
        {
            var existing = contentTypeService.Get(StartTypeAlias);

            if (existing != null)
            {
                return existing;
            }

            var textstring = await GetDataTypeAsync(Constants.DataTypes.Guids.TextstringGuid);

            var startType = new ContentType(shortStringHelper, Constants.System.Root)
            {
                Alias = StartTypeAlias,
                Name = "Start",
                Icon = "icon-document-html",
                Variations = ContentVariation.Culture,
                AllowedAsRoot = true,
                AllowedTemplates = [template],
            };
            startType.SetDefaultTemplate(template);

            // Base som composition, så att Start får Meta Description och blir IBase i koden
            startType.AddContentType(baseType);

            // Utan flik, precis som i backoffice där den skapades
            startType.AddPropertyType(new PropertyType(shortStringHelper, textstring, "title")
            {
                Name = "Title",
                Variations = ContentVariation.Culture,
            });

            await CreateAsync(startType);

            return startType;
        }

        private async Task EnsureStartPageAsync(IContentType startType)
        {
            if (contentService.GetRootContent().Any(x => x.ContentType.Alias == StartTypeAlias))
            {
                return;
            }

            var home = contentService.Create("Home", Constants.System.Root, StartTypeAlias);

            home.SetCultureName("Home", English);
            home.SetValue("title", "My Title", English);
            home.SetValue("metaDescription", "My meta description", English);

            home.SetCultureName("Hem", Swedish);
            home.SetValue("title", "Min titel", Swedish);
            home.SetValue("metaDescription", "Min meta beskrivning", Swedish);

            var saved = contentService.Save(home);

            if (!saved.Success)
            {
                throw new InvalidOperationException($"Could not save the start page: {saved.Result}");
            }

            var published = contentService.Publish(home, ["*"]);

            if (!published.Success)
            {
                throw new InvalidOperationException($"Could not publish the start page: {published.Result}");
            }

            await EnsureDomainsAsync(home);
        }

        /// <summary>/en för engelska och / för svenska, som i Culture and Hostnames på startsidan.</summary>
        private async Task EnsureDomainsAsync(IContent home)
        {
            if ((await domainService.GetAssignedDomainsAsync(home.Key, true)).Any())
            {
                return;
            }

            var attempt = await domainService.UpdateDomainsAsync(home.Key, new DomainsUpdateModel
            {
                DefaultIsoCode = null,
                Domains =
                [
                    new DomainModel { DomainName = "/en", IsoCode = English },
                    new DomainModel { DomainName = "/", IsoCode = Swedish },
                ],
            });

            // Sajten fungerar utan domäner också, så det här stoppar inte migrationen
            if (!attempt.Success)
            {
                Logger.LogWarning("Could not add the domains to the start page ({Status}). Add them in Culture and Hostnames.", attempt.Status);
            }
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
    }
}
