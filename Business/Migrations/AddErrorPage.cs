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
	/// 404-sidan (OUT-45): dokumenttypen <c>error</c> med Base och Intro, mallen Error, och sidan "Sidan finns inte"
	/// under Start, publicerad på svenska och engelska med Meta Robots NONE. Varje del kollar först om den redan finns.
	/// </summary>
	/// <remarks>
	/// Sidan får den fasta nyckeln <see cref="ErrorPageKey"/>, som också står i <c>Error404Collection</c> i
	/// appsettings.json. Hos läraren skapades sidan i backoffice, men vi har en databas var, så nyckeln måste sättas här.
	/// </remarks>
	public class AddErrorPage(
		IMigrationContext context,
		ICoreScopeProvider scopeProvider,
		IContentTypeService contentTypeService,
		IDataTypeService dataTypeService,
		ITemplateService templateService,
		IContentService contentService,
		ILanguageService languageService,
		IShortStringHelper shortStringHelper,
		IHostEnvironment hostEnvironment)
		: UnscopedAsyncMigrationBase(context)
	{
		public const string ErrorTypeAlias = "error";

		/// <summary>Samma nyckel som <c>ContentKey</c> i <c>Error404Collection</c> i appsettings.json.</summary>
		public static readonly Guid ErrorPageKey = new("26a27634-6906-4560-8590-7ae3907dcb8e");

		private const string TemplateAlias = "Error";

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
			var existing = contentTypeService.Get(ErrorTypeAlias);

			if (existing != null)
			{
				return existing;
			}

			var baseType = contentTypeService.Get(AddBaseAndStart.BaseTypeAlias)
				?? throw new InvalidOperationException($"The '{AddBaseAndStart.BaseTypeAlias}' content type does not exist.");

			var textarea = await dataTypeService.GetAsync(Constants.DataTypes.Guids.TextareaGuid)
				?? throw new InvalidOperationException("The Textarea data type does not exist.");

			// Varierar per språk som de andra sidorna. Inte tillåten i roten: sidan ligger under Start
			var contentType = new ContentType(shortStringHelper, Constants.System.Root)
			{
				Alias = ErrorTypeAlias,
				Name = "Error",
				Icon = "icon-alert",
				Variations = ContentVariation.Culture,
				AllowedTemplates = [template],
			};
			contentType.SetDefaultTemplate(template);
			contentType.AddContentType(baseType);
			contentType.AddPropertyType(
				new PropertyType(shortStringHelper, textarea, AddRegionAndMunicipality.IntroAlias)
				{
					Name = "Intro",
					Variations = ContentVariation.Culture,
				},
				"content",
				"Content");

			var attempt = await contentTypeService.CreateAsync(contentType, UserKey);

			if (!attempt.Success)
			{
				throw new InvalidOperationException($"Could not create the '{ErrorTypeAlias}' content type: {attempt.Result}");
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
			if (contentService.GetById(ErrorPageKey) != null)
			{
				return;
			}

			var start = contentService.GetRootContent().FirstOrDefault(x => x.ContentType.Alias == AddBaseAndStart.StartTypeAlias);

			if (start == null)
			{
				Logger.LogWarning("No start page found, so the 404 page was not created.");
				return;
			}

			var content = contentService.Create("Page not found", start.Key, ErrorTypeAlias);

			// Den fasta nyckeln gör att Error404Collection i appsettings.json hittar sidan i allas databaser
			content.Key = ErrorPageKey;

			foreach (var language in await languageService.GetAllAsync())
			{
				var isSwedish = language.IsoCode.StartsWith("sv", StringComparison.OrdinalIgnoreCase);

				content.SetCultureName(isSwedish ? "Sidan finns inte" : "Page not found", language.IsoCode);
				content.SetValue(
					AddRegionAndMunicipality.IntroAlias,
					isSwedish
						? "Sidan du letar efter finns inte. Den kan ha flyttats, eller så är adressen felstavad."
						: "The page you are looking for does not exist. It may have moved, or the address may be misspelled.",
					language.IsoCode);
			}

			// Meta Robots varierar inte per språk, så värdet sätts utan språk
			content.SetValue(AddMetaRobots.MetaRobotsAlias, MetaRobotsValues.None);

			var saved = contentService.Save(content);

			if (!saved.Success)
			{
				throw new InvalidOperationException($"Could not save the 404 page: {saved.Result}");
			}

			var published = contentService.Publish(content, ["*"]);

			if (!published.Success)
			{
				throw new InvalidOperationException($"Could not publish the 404 page: {published.Result}");
			}
		}
	}
}