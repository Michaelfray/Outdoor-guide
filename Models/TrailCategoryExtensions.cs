using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models;

public static class TrailCategoryExtensions
{
    /// <summary>
    /// True om undersidan har minst en publicerad led. Undersidor utan leder, som Flerdagsturer, visas inte i megamenyn
    /// eller på aktivitetssidan. Sidan finns kvar i backoffice, och väljer någon leder där syns den igen av sig själv.
    /// </summary>
    public static bool HasTrails(this TrailCategory category) =>
        category.Trails?.OfType<Trail>().Any(trail => trail.IsPublished()) == true;
}
