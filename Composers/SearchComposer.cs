using OutdoorGuide.Business.Services;
using OutdoorGuide.Business.Services.Interfaces;
using Umbraco.Cms.Core.Composing;

namespace OutdoorGuide.Composers;

public class SearchComposer : IComposer
{
	public void Compose(IUmbracoBuilder builder)
	{
		builder.Services.AddScoped<ISearchService, SearchService>();
	}
}
