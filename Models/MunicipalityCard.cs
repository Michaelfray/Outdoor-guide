using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models;

/// <summary>Ett kommunkort på Platser och regionsidan (OUT-101): kommunen och hur många publicerade leder den har.</summary>
public record MunicipalityCard(Municipality Municipality, int TrailCount)
{
    /// <summary>Kommunerna under <paramref name="parent"/> som är publicerade, i trädets ordning, med antal leder.</summary>
    public static IReadOnlyList<MunicipalityCard> For(Umbraco.Cms.Core.Models.PublishedContent.IPublishedContent parent) =>
        parent.Children<Municipality>()?
            .Where(municipality => municipality.IsPublished())
            .Select(municipality => new MunicipalityCard(
                municipality,
                municipality.Children<Trail>()?.Count(trail => trail.IsPublished()) ?? 0))
            .ToList() ?? [];
}
