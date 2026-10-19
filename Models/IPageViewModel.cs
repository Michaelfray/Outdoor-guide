using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models;

public interface IPageViewModel
{
    IBase Content { get; }
    Start? StartPage { get; }
    IReadOnlyList<IPublishedContent> MenuItems { get; }
    string UrlSegment { get; }
}
