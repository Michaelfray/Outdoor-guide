using OutdoorGuide.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models;

public class PageViewModel<T> : IPageViewModel where T : class, IBase
{
    public required T Content { get; init; }
    IBase IPageViewModel.Content => Content;

    public Start? StartPage => Content.AncestorOrSelf<Start>();
    public IReadOnlyList<IPublishedContent> MenuItems
        => StartPage?.FirstChild<Settings>()?.MenuItems?.Where(page => page.IsPublished()).ToList() ?? [];
    public string UrlSegment { get; set; } = string.Empty;
}