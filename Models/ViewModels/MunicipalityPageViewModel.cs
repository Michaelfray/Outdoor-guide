using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models.ViewModels;

public class MunicipalityPageViewModel : PageViewModel<Municipality>
{
    public TrailFilter Filter { get; init; } = new();

    public IReadOnlyList<Trail> Trails { get; init; } = [];

    public bool HasTrails { get; init; }

    /// <summary>Betygen för lederna i <see cref="Trails"/>, per ledens Key. Leder utan röster saknas.
    /// Sätts efter Build, eftersom betygen hämtas asynkront (OUT-101).</summary>
    public IReadOnlyDictionary<Guid, TrailRatingSummary> Ratings { get; set; } = new Dictionary<Guid, TrailRatingSummary>();
}
