using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
	/// <summary>
	/// Sitemapen (OUT-43): dokumenttypen <c>sitemap</c> med mallen Sitemap, och sidan Sitemap under Start,
	/// publicerad på svenska och engelska. Varje del kollar först om den redan finns.
	/// </summary>
	/// <remarks>
	/// Byggt som <see cref="AddMemberPages"/>. Typen har inte Base som composition, så sidan listar inte sig själv.
	/// Sidan får adressen /sitemap/ (och /en/sitemap/), och Program.cs skriver om /sitemap.xml dit.
	/// </remarks>
	public class AddSitemapPage(
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
		public const string SitemapTypeAlias = "sitemap";
		public const string PageName = "Sitemap";

		private const string TemplateAlias = "Sitemap";

		private static readonly Guid UserKey = Constants.Security.SuperUserKey;

		protected override async Task MigrateAsync()
		{
			using (var scope = scopeProvider.CreateCoreScope())
			{
				var template = await EnsureTemplateAsync();
				var contentType = await EnsureContentTypeAsync(template);
				await EnsureAllowedUnderStartAsync(contentType);
				await EnsurePageAsync();

				scope.Complete();
			}

			Context.Complete();
		}

		private async Task<ITemplate> EnsureTemplateAsync()
		{
			var existing = await templateService.GetAsync(TemplateAlias);

			if (existing != null)
			{
				return existing;
			}

			var viewPath = Path.Combine(hostEnvironment.ContentRootPath, "Views", $"{TemplateAlias}.cshtml");
			var content = System.IO.File.Exists(viewPath) ? await System.IO.File.ReadAllTextAsync(viewPath) : null;

			var attempt = await templateService.CreateAsync(TemplateAlias, TemplateAlias, content, UserKey);

			if (!attempt.Success)
			{
				throw new InvalidOperationException($"Could not create the '{TemplateAlias}' template: {attempt.Status}");
			}

			return attempt.Result;
		}

		private async Task<IContentType> EnsureContentTypeAsync(ITemplate template)
		{
			var existing = contentTypeService.Get(SitemapTypeAlias);

			if (existing != null)
			{
				return existing;
			}

			var contentType = new ContentType(shortStringHelper, Constants.System.Root)
			{
				Alias = SitemapTypeAlias,
				Name = "Sitemap",
				Icon = "icon-sitemap",
				Variations = ContentVariation.Culture,
				AllowedTemplates = [template],
			};
			contentType.SetDefaultTemplate(template);

			var attempt = await contentTypeService.CreateAsync(contentType, UserKey);

			if (!attempt.Success)
			{
				throw new InvalidOperationException($"Could not create the '{SitemapTypeAlias}' content type: {attempt.Result}");
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

		private async Task EnsurePageAsync()
		{
			var start = contentService.GetRootContent().FirstOrDefault(x => x.ContentType.Alias == AddBaseAndStart.StartTypeAlias);

			if (start == null)
			{
				Logger.LogWarning("No start page found, so the {Page} page was not created.", PageName);
				return;
			}

			var exists = contentService
				.GetPagedChildren(start.Id, 0, int.MaxValue, out _, propertyAliases: null, filter: null, ordering: null)
				.Any(x => x.ContentType.Alias == SitemapTypeAlias);

			if (exists)
			{
				return;
			}

			var content = contentService.Create(PageName, start.Key, SitemapTypeAlias);

			
			foreach (var language in await languageService.GetAllAsync())
			{
				content.SetCultureName(PageName, language.IsoCode);
			}

			var saved = contentService.Save(content);

			if (!saved.Success)
			{
				throw new InvalidOperationException($"Could not save the {PageName} page: {saved.Result}");
			}

			var published = contentService.Publish(content, ["*"]);

			if (!published.Success)
			{
				throw new InvalidOperationException($"Could not publish the {PageName} page: {published.Result}");
			}
		}
	}
}