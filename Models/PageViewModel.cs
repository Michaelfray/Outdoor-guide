using OutdoorGuide.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models;

public class PageViewModel<T> : IPageViewModel where T : class, IPublishedContent
{
    public required T Content { get; init; }
    IPublishedContent IPageViewModel.Content => Content;

    public Start? StartPage => Content.AncestorOrSelf<Start>();
    public IReadOnlyList<IPublishedContent> MenuItems
        => StartPage?.FirstChild<Settings>()?.MenuItems?.Where(page => page.IsPublished()).ToList() ?? [];

    /// <summary>Sidor utan Base (Login, Register, Account) har ingen meta description, så de får en tom text.</summary>
    public string MetaDescription => (Content as IBase)?.MetaDescription ?? string.Empty;

    public string UrlSegment { get; set; } = string.Empty;
}