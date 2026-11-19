using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models.ViewModels;

public class TrailsPageViewModel : PageViewModel<TrailsPage>
{
    /// <summary>Korten för aktivitetssidorna under Leder (Vandring, Paddling, Cykel, Vinter) som är publicerade på aktuellt språk.</summary>
    public IReadOnlyList<CategoryCard> Activities { get; init; } = [];
}
