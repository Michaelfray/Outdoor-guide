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

    /// <summary>
    /// Meta description från fältet på sidan. Är fältet tomt används sidans ingress (Intro), kortad till högst
    /// <see cref="MetaDescriptionMaxLength"/> tecken, så att sökmotorer och Lighthouse får en beskrivning (OUT-53).
    /// Sidor utan både fält och ingress (Login, Register, Account) får en tom text.
    /// </summary>
    public string MetaDescription
    {
        get
        {
            var meta = (Content as IBase)?.MetaDescription;

            if (!string.IsNullOrWhiteSpace(meta))
            {
                return meta;
            }

            var intro = Content.HasProperty("intro") ? Content.Value<string>("intro") : null;

            return Shorten(intro?.Trim() ?? string.Empty, MetaDescriptionMaxLength);
        }
    }

    /// <summary>Google visar ungefär 155–160 tecken av en beskrivning.</summary>
    public const int MetaDescriptionMaxLength = 160;

    /// <summary>Kortar texten vid ett mellanslag och lägger till "…", så att inget ord klipps mitt i.</summary>
    private static string Shorten(string text, int maxLength)
    {
        if (text.Length <= maxLength)
        {
            return text;
        }

        var cut = text.LastIndexOf(' ', maxLength - 1);
        return text[..(cut > 0 ? cut : maxLength - 1)].TrimEnd(',', '.', ';', ':') + "…";
    }

    public string UrlSegment { get; set; } = string.Empty;
    /// <summary>Sidor utan värde, och sidor utan Base, räknas som ALL.</summary>
    public string MetaRobots => (Content as IBase)?.MetaRobots == MetaRobotsValues.None ? MetaRobotsValues.None : MetaRobotsValues.All;
}