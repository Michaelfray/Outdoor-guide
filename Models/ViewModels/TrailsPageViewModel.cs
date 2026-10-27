using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models.ViewModels;

public class TrailsPageViewModel : PageViewModel<TrailsPage>
{
    /// <summary>Aktivitetssidorna under Leder (Vandring, Paddling, Cykel, Vinter) som är publicerade på aktuellt språk.</summary>
    public IReadOnlyList<ActivityPage> ActivityPages { get; init; } = [];
}