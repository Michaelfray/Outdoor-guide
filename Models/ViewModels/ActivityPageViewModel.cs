using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models.ViewModels;

public class ActivityPageViewModel : PageViewModel<ActivityPage>
{
    /// <summary>Undersidorna, till exempel Dagsturer och Flerdagsturer, som är publicerade på aktuellt språk.</summary>
    public IReadOnlyList<TrailCategory> Categories { get; init; } = [];

    /// <summary>Alla publicerade leder med samma aktivitet som sidan, i bokstavsordning.</summary>
    public IReadOnlyList<Trail> Trails { get; init; } = [];
}