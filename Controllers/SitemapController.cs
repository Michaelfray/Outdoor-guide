using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using OutdoorGuide.Business.Migrations;
using OutdoorGuide.Business.Services.Interfaces;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace OutdoorGuide.Controllers
{
	/// <summary>Svarar på sidan av typen Sitemap med XML i stället för en vy. Byggd som lärarens SitemapController.</summary>
	public class SitemapController(
		ILogger<RenderController> logger,
		ICompositeViewEngine compositeViewEngine,
		IUmbracoContextAccessor umbracoContextAccessor,
		ISitemapService sitemapService)
		: RenderController(logger, compositeViewEngine, umbracoContextAccessor)
	{
		public override IActionResult Index()
		{
			if (CurrentPage?.ContentType.Alias != AddSitemapPage.SitemapTypeAlias)
			{
				return NotFound();
			}

			// sv-SE på /sitemap.xml och en-US på /en/sitemap.xml, från domänen
			var culture = UmbracoRouteValues.PublishedRequest.Culture;
			var xml = sitemapService.GenerateXml(sitemapService.Pages(culture), culture);

			return Content(xml, "application/xml");
		}
	}
}