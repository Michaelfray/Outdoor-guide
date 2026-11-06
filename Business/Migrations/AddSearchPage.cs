using OutdoorGuide.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
	/// <summary>
	/// Söksidan (OUT-74): dokumenttypen <c>searchPage</c> med Base och mallen SearchPage, och sidan Sök/Search under
	/// Start, publicerad på svenska och engelska med Meta Robots NONE. Varje del kollar först om den redan finns, så
	/// inget som någon har skapat eller ändrat skrivs över.
	/// </summary>
	/// <remarks>
	/// Byggt som <see cref="AddMemberPages"/>, men med Base som composition, så att sidan har SEO-fälten. Sökresultat ska
	/// inte indexeras, så sidan får Meta Robots NONE, vilket också håller den utanför sitemap.
	/// </remarks>
	public class AddSearchPage(
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
		public const string SearchPageTypeAlias = "searchPage";

		private const string TemplateAlias = "SearchPage";
		private const string SwedishName = "Sök";
		private const string EnglishName = "Search";

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

			// Unscoped migrations måste själva säga att de är klara, då sparas planens nya läge
			Context.Complete();
		}

		private async Task<ITemplate> EnsureTemplateAsync()
		{
			var existing = await templateService.GetAsync(TemplateAlias);

			if (existing != null)
			{
				return existing;
			}

			// Att skapa en mall skriver (om) vyfilen, så skicka med vyn som redan ligger i repot
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
			var existing = contentTypeService.Get(SearchPageTypeAlias);

			if (existing != null)
			{
				return existing;
			}

			var baseType = contentTypeService.Get(AddBaseAndStart.BaseTypeAlias)
				?? throw new InvalidOperationException($"The '{AddBaseAndStart.BaseTypeAlias}' content type does not exist.");

			// Varierar per språk som de andra sidorna, så varje språk får eget namn och egen adress.
			// Inte tillåten i roten: sidan ligger under Start
			var contentType = new ContentType(shortStringHelper, Constants.System.Root)
			{
				Alias = SearchPageTypeAlias,
				Name = "Search page",
				Icon = "icon-search",
				Variations = ContentVariation.Culture,
				AllowedTemplates = [template],
			};
			contentType.SetDefaultTemplate(template);
			contentType.AddContentType(baseType);

			var attempt = await contentTypeService.CreateAsync(contentType, UserKey);

			if (!attempt.Success)
			{
				throw new InvalidOperationException($"Could not create the '{SearchPageTypeAlias}' content type: {attempt.Result}");
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
				Logger.LogWarning("No start page found, so the {Page} page was not created.", EnglishName);
				return;
			}

			var exists = contentService
				.GetPagedChildren(start.Id, 0, int.MaxValue, out _, propertyAliases: null, filter: null, ordering: null)
				.Any(x => x.ContentType.Alias == SearchPageTypeAlias);

			if (exists)
			{
				return;
			}

			var content = contentService.Create(EnglishName, start.Key, SearchPageTypeAlias);

			// En sida som varierar per språk går inte att spara utan ett namn på varje språk
			foreach (var language in await languageService.GetAllAsync())
			{
				var isSwedish = language.IsoCode.StartsWith("sv", StringComparison.OrdinalIgnoreCase);
				content.SetCultureName(isSwedish ? SwedishName : EnglishName, language.IsoCode);
			}

			// Meta Robots varierar inte per språk, så värdet sätts utan språk
			content.SetValue(AddMetaRobots.MetaRobotsAlias, MetaRobotsValues.None);

			var saved = contentService.Save(content);

			if (!saved.Success)
			{
				throw new InvalidOperationException($"Could not save the {EnglishName} page: {saved.Result}");
			}

			var published = contentService.Publish(content, ["*"]);

			if (!published.Success)
			{
				throw new InvalidOperationException($"Could not publish the {EnglishName} page: {published.Result}");
			}
		}
	}
}
