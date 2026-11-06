using System.Globalization;
using OutdoorGuide.Business.Services.Interfaces;
using OutdoorGuide.Models.ViewModels;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Controllers
{
    /// <summary>Söksidan: läser sökordet från <c>?q=</c> och söker bland leder och kommuner (OUT-74).</summary>
    public class SearchPageController(PageControllerDependencies dependencies, ISearchService searchService)
        : BasePageController<SearchPage, SearchPageViewModel>(dependencies)
    {
        private const int MaxQueryLength = 100;

        protected override SearchPageViewModel Build(SearchPage page)
        {
            var query = Request.Query["q"].ToString().Trim();

            return new SearchPageViewModel
            {
                Content = page,
                Query = query,
                Results = query.Length is > 0 and <= MaxQueryLength
                    ? searchService.Search(query, CultureInfo.CurrentCulture)
                    : [],
            };
        }
    }
}
