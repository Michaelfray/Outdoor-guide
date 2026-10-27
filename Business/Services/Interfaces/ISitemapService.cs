using Umbraco.Cms.Core.Models.PublishedContent;

namespace OutdoorGuide.Business.Services.Interfaces;

public interface ISitemapService
{
	IEnumerable<IPublishedContent> Pages(string? culture);

	string GenerateXml(IEnumerable<IPublishedContent> pages, string? culture);
}