using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models.ViewModels;

public class PlacesPageViewModel : PageViewModel<PlacesPage>
{
    /// <summary>Regionerna under Start som är publicerade på aktuellt språk, i trädets ordning.</summary>
    public IReadOnlyList<Region> Regions { get; init; } = [];

    /// <summary>Kommunerna per region, med antal leder, så att Platser visar alla kommuner direkt (OUT-101).</summary>
    public IReadOnlyDictionary<Guid, IReadOnlyList<MunicipalityCard>> MunicipalitiesByRegion { get; init; } =
        new Dictionary<Guid, IReadOnlyList<MunicipalityCard>>();
}
