using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models.ViewModels;

public class StartPageViewModel : PageViewModel<Start>
{
    public TrailFilter Filter { get; init; } = new();

    public IReadOnlyList<Trail> Trails { get; init; } = [];

    public bool HasTrails { get; init; }

    /// <summary>Alla publicerade leder, före filtret. Används i "Visar 4 av 6 leder".</summary>
    public int TotalTrails { get; init; }

    /// <summary>Betygen för lederna i <see cref="Trails"/>, per ledens Key. Leder utan röster saknas. Sätts efter Build, eftersom betygen hämtas asynkront.</summary>
    public IReadOnlyDictionary<Guid, TrailRatingSummary> Ratings { get; set; } = new Dictionary<Guid, TrailRatingSummary>();

    /// <summary>Adressen till söksidan i det aktuella språket, eller <c>null</c> om sidan inte finns. Då visas ingen sökruta.</summary>
    public string? SearchUrl { get; init; }
}