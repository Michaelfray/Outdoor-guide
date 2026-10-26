using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>
    /// Medlemssidorna (OUT-35): dokumenttyperna <c>login</c>, <c>register</c> och <c>account</c> med mallar, och
    /// sidorna Logga in, Skapa konto och Mitt konto under Start, publicerade på svenska och engelska.
    /// Varje del kollar först om den redan finns, så inget som någon har skapat eller ändrat skrivs över.
    /// </summary>
    /// <remarks>
    /// Byggt som Carls <c>AddMemberPages</c> i schele/umbraco26, med Skapa konto tillagd. Typerna har inte Base som
    /// composition, så sidorna hamnar inte i sitemap: där finns inget för sökmotorer. Controllers och ViewModels
    /// kommer i OUT-37 till OUT-39, när modellerna har genererats.
    /// </remarks>
    public class AddMemberPages(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IContentTypeService contentTypeService,
        ITemplateService templateService,
        IContentService contentService,
        ILanguageService languageService,
        IShortStringHelper shortStringHelper,
        IHostEnvironment hostEnvironment)
        : UnscopedAsyncMigrationBase(context)
    {
        public const string LoginTypeAlias = "login";
        public const string RegisterTypeAlias = "register";
        public const string AccountTypeAlias = "account";

        private static readonly Guid UserKey = Constants.Security.SuperUserKey;

        private static readonly PageSpec LoginPage = new(LoginTypeAlias, "Login", "Login", "icon-combination-lock", "Logga in", "Log in");
        private static readonly PageSpec RegisterPage = new(RegisterTypeAlias, "Register", "Register", "icon-document-html", "Skapa konto", "Create account");
        private static readonly PageSpec AccountPage = new(AccountTypeAlias, "Account", "Account", "icon-document-html", "Mitt konto", "My account");

        protected override async Task MigrateAsync()
        {
            using (var scope = scopeProvider.CreateCoreScope())
            {
                foreach (var page in new[] { LoginPage, RegisterPage, AccountPage })
                {
                    var template = await EnsureTemplateAsync(page.Template);
                    var contentType = await EnsureContentTypeAsync(page, template);
                    await EnsureAllowedUnderStartAsync(contentType);
                    await EnsurePageAsync(page, contentType);
                }

                scope.Complete();
            }

            // Unscoped migrations måste själva säga att de är klara, då sparas planens nya läge
            Context.Complete();
        }

        private async Task<ITemplate> EnsureTemplateAsync(string alias)
        {
            var existing = await templateService.GetAsync(alias);

            if (existing != null)
            {
                return existing;
            }

            // Att skapa en mall skriver (om) vyfilen, så skicka med vyn som redan ligger i repot
            var viewPath = Path.Combine(hostEnvironment.ContentRootPath, "Views", $"{alias}.cshtml");
            var content = System.IO.File.Exists(viewPath) ? await System.IO.File.ReadAllTextAsync(viewPath) : null;

            var attempt = await templateService.CreateAsync(alias, alias, content, UserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not create the '{alias}' template: {attempt.Status}");
            }

            return attempt.Result;
        }

        private async Task<IContentType> EnsureContentTypeAsync(PageSpec page, ITemplate template)
        {
            var existing = contentTypeService.Get(page.Alias);

            if (existing != null)
            {
                return existing;
            }

            // Varierar per språk som de andra sidorna, så varje språk får eget namn och egen adress.
            // Inte tillåten i roten: sidorna ligger under Start
            var contentType = new ContentType(shortStringHelper, Constants.System.Root)
            {
                Alias = page.Alias,
                Name = page.TypeName,
                Icon = page.Icon,
                Variations = ContentVariation.Culture,
                AllowedTemplates = [template],
            };
            contentType.SetDefaultTemplate(template);

            var attempt = await contentTypeService.CreateAsync(contentType, UserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not create the '{page.Alias}' content type: {attempt.Result}");
            }

            return contentType;
        }

        private async Task EnsureAllowedUnderStartAsync(IContentType contentType)
        {
            var startType = contentTypeService.Get(AddBaseAndStart.StartTypeAlias);

            if (startType == null)
            {
                Logger.LogWarning("The '{Parent}' content type does not exist, so '{Child}' was not allowed under it.", AddBaseAndStart.StartTypeAlias, contentType.Alias);
                return;
            }

            var allowed = startType.AllowedContentTypes?.ToList() ?? [];

            if (allowed.Any(x => x.Key == contentType.Key))
            {
                return;
            }

            allowed.Add(new ContentTypeSort(contentType.Key, allowed.Count, contentType.Alias));
            startType.AllowedContentTypes = allowed;

            var attempt = await contentTypeService.UpdateAsync(startType, UserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not allow '{contentType.Alias}' under '{AddBaseAndStart.StartTypeAlias}': {attempt.Result}");
            }
        }

        private async Task EnsurePageAsync(PageSpec page, IContentType contentType)
        {
            var start = contentService.GetRootContent().FirstOrDefault(x => x.ContentType.Alias == AddBaseAndStart.StartTypeAlias);

            if (start == null)
            {
                Logger.LogWarning("No start page found, so the {Page} page was not created.", page.English);
                return;
            }

            var exists = contentService
                .GetPagedChildren(start.Id, 0, int.MaxValue, out _, propertyAliases: null, filter: null, ordering: null)
                .Any(x => x.ContentType.Alias == page.Alias);

            if (exists)
            {
                return;
            }

            var content = contentService.Create(page.English, start.Key, page.Alias);

            // En sida som varierar per språk går inte att spara utan ett namn på varje språk
            foreach (var language in await languageService.GetAllAsync())
            {
                var isSwedish = language.IsoCode.StartsWith("sv", StringComparison.OrdinalIgnoreCase);
                content.SetCultureName(isSwedish ? page.Swedish : page.English, language.IsoCode);
            }

            var saved = contentService.Save(content);

            if (!saved.Success)
            {
                throw new InvalidOperationException($"Could not save the {page.English} page: {saved.Result}");
            }

            var published = contentService.Publish(content, ["*"]);

            if (!published.Success)
            {
                throw new InvalidOperationException($"Could not publish the {page.English} page: {published.Result}");
            }
        }

        /// <summary>En medlemssida: dokumenttypen, mallen och sidans namn på svenska och engelska.</summary>
        private sealed record PageSpec(string Alias, string Template, string TypeName, string Icon, string Swedish, string English);
    }
}