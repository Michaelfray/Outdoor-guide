using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models;

/// <summary>Ett kommunkort på Platser och regionsidan (OUT-101): kommunen och hur många publicerade leder den har.</summary>
public record MunicipalityCard(Municipality Municipality, int TrailCount)
{
    /// <summary>Kortet för en kommun, med antalet publicerade leder.</summary>
    public static MunicipalityCard From(Municipality municipality) =>
        new(municipality, municipality.Children<Trail>()?.Count(trail => trail.IsPublished()) ?? 0);

    public static IReadOnlyList<MunicipalityCard> For(IPublishedContent parent) =>
        parent.Children<Municipality>()?
            .Where(municipality => municipality.IsPublished())
            .Select(From)
            .ToList() ?? [];
}
