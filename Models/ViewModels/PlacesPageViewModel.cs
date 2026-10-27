using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models.ViewModels;

public class PlacesPageViewModel : PageViewModel<PlacesPage>
{
    /// <summary>Regionerna under Start som är publicerade på aktuellt språk, i trädets ordning.</summary>
    public IReadOnlyList<Region> Regions { get; init; } = [];
}