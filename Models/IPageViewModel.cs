using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models;

public interface IPageViewModel
{
    IBase Content { get; }
    Start? StartPage { get; }
    string UrlSegment { get; }
}
