using System.Xml.Linq;
using OutdoorGuide.Business.Services.Interfaces;
using OutdoorGuide.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Business.Services;

/// <summary>Byggd som lärarens SitemapService i schele/umbraco26.</summary>
public class SitemapService(
	IUmbracoContextAccessor umbracoContextAccessor,
	IDocumentNavigationQueryService documentNavigationQueryService) : ISitemapService
{
	private static readonly XNamespace SitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";

	public IEnumerable<IPublishedContent> Pages(string? culture)
	{
		if (!umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext) || umbracoContext.Content is not { } content)
		{
			return [];
		}

		var startPage = documentNavigationQueryService.TryGetRootKeys(out var rootKeys)
			? rootKeys.Select(key => content.GetById(key)).OfType<Start>().FirstOrDefault()
			: null;

		if (startPage == null)
		{
			return [];
		}

		// Sidor utan Meta Robots-värde räknas som ALL, så bara NONE hoppas över.
		// Sidor utan Base (Settings, Logga in, Sitemap) har inget för sökmotorer och är inte med
		return startPage.DescendantsOrSelf<IPublishedContent>()
			.Where(page => page is IBase basePage && basePage.MetaRobots != MetaRobotsValues.None && page.IsPublished(culture))
			.ToList();
	}

	public string GenerateXml(IEnumerable<IPublishedContent> pages, string? culture)
	{
		var urlset = new XElement(SitemapNamespace + "urlset",
			pages.Select(page => new XElement(SitemapNamespace + "url",
				new XElement(SitemapNamespace + "loc", page.Url(culture: culture, mode: UrlMode.Absolute)),
				new XElement(SitemapNamespace + "lastmod", page.UpdateDate.ToString("yyyy-MM-dd")))));

		var document = new XDocument(new XDeclaration("1.0", "utf-8", null), urlset);

		// XDocument.ToString() skriver inte ut deklarationen, så den läggs till först
		return document.Declaration + Environment.NewLine + document;
	}
}