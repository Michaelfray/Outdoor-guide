using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models.ViewModels;

public class ActivityPageViewModel : PageViewModel<ActivityPage>
{
    /// <summary>Korten för undersidorna, till exempel Dagsturer och Flerdagsturer, som är publicerade på aktuellt språk.</summary>
    public IReadOnlyList<CategoryCard> Categories { get; init; } = [];

    /// <summary>Alla publicerade leder med samma aktivitet som sidan, i bokstavsordning.</summary>
    public IReadOnlyList<Trail> Trails { get; init; } = [];

    /// <summary>Betygen för lederna, per ledens nyckel. Leder utan röster finns inte med.</summary>
    public IReadOnlyDictionary<Guid, TrailRatingSummary> Ratings { get; set; } = new Dictionary<Guid, TrailRatingSummary>();
}
