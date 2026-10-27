using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models.ViewModels;

public class TrailCategoryViewModel : PageViewModel<TrailCategory>
{
    /// <summary>Lederna som redaktören har valt, i redaktörens ordning, bara de som är publicerade på aktuellt språk.</summary>
    public IReadOnlyList<Trail> Trails { get; init; } = [];
}