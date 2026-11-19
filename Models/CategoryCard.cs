using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models;

/// <summary>
/// Ett kort för en aktivitetssida på Leder, eller en undersida på aktivitetssidan (OUT-105): sidan, aktiviteten för
/// färgen och ikonen, hur många publicerade leder den har och en led vars bild visas på kortet.
/// </summary>
public record CategoryCard(IPublishedContent Page, string? Intro, string? Activity, int TrailCount, Trail? Cover)
{
    /// <summary>Kortet för en undersida. Lederna är de som redaktören har valt, och bilden kommer från den första som har en bild</summary>
    public static CategoryCard For(TrailCategory category, string? activity)
    {
        var trails = category.Trails?.OfType<Trail>().Where(trail => trail.IsPublished()).ToList() ?? [];
        return new CategoryCard(category, category.Intro, activity, trails.Count, trails.FirstOrDefault(trail => trail.Image != null));
    }

    /// <summary>Kortet för en aktivitetssida, med alla publicerade leder som har samma aktivitet</summary>
    public static CategoryCard For(ActivityPage page, IReadOnlyList<Trail> allTrails)
    {
        var trails = string.IsNullOrEmpty(page.Activity) ? [] : allTrails.Where(trail => trail.Activity == page.Activity).ToList();
        return new CategoryCard(page, page.Intro, page.Activity, trails.Count, trails.FirstOrDefault(trail => trail.Image != null));
    }
}
