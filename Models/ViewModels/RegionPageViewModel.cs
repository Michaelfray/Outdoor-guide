using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models.ViewModels;

public class RegionPageViewModel : PageViewModel<Region>
{
    /// <summary>Kommunerna i regionen, med antal leder (OUT-101).</summary>
    public IReadOnlyList<MunicipalityCard> Municipalities { get; init; } = [];
}
