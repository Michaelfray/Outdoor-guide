using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models.ViewModels;

public class TrailCategoryViewModel : PageViewModel<TrailCategory>
{
    /// <summary>Lederna som redaktören har valt, i redaktörens ordning, bara de som är publicerade på aktuellt språk.</summary>
    public IReadOnlyList<Trail> Trails { get; init; } = [];

    /// <summary>Betygen för lederna, per ledens nyckel. Leder utan röster finns inte med.</summary>
    public IReadOnlyDictionary<Guid, TrailRatingSummary> Ratings { get; set; } = new Dictionary<Guid, TrailRatingSummary>();
}
