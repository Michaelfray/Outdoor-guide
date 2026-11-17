using OutdoorGuide.Business.Migrations;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models;

/// <summary>
/// Sidorna som ligger i sidfoten men inte i menyn, till exempel Allemansrätten och Integritet och cookies (OUT-47, OUT-94).
/// </summary>
public static class LegalPages
{
    /// <summary>
    /// Textsidorna under startsidan som inte är valda i menyn. Säsongstips, Om oss och Kontakt ligger i menyn,
    /// så kvar blir Allemansrätten, Integritet och cookies och andra sidor av samma slag som redaktören lägger till.
    /// </summary>
    public static IReadOnlyList<TextPage> ForFooter(Start? start, IReadOnlyList<IPublishedContent> menuItems)
    {
        if (start == null)
        {
            return [];
        }

        var inMenu = menuItems.Select(item => item.Key).ToHashSet();

        return start.Children<TextPage>()?
            .Where(page => page.IsPublished() && !inMenu.Contains(page.Key))
            .ToList() ?? [];
    }

    /// <summary>
    /// Sidan Integritet och cookies, som cookie-rutan länkar till. Den hittas på sitt engelska namn,
    /// som <see cref="AddLegalPages"/> ger den. Döps sidan om på engelska hittas den inte, och då visas ingen länk.
    /// </summary>
    public static TextPage? PrivacyPage(Start? start) =>
        start?.Children<TextPage>()?.FirstOrDefault(page => page.IsPublished()
            && page.Cultures.Values.Any(culture => string.Equals(culture.Name, AddLegalPages.PrivacyPageEnglishName, StringComparison.OrdinalIgnoreCase)));
}
