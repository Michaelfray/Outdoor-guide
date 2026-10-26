using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models;

public interface IPageViewModel
{
    IPublishedContent Content { get; }
    Start? StartPage { get; }
    IReadOnlyList<IPublishedContent> MenuItems { get; }
    string MetaDescription { get; }
    string UrlSegment { get; }
    string MetaRobots { get; }
}