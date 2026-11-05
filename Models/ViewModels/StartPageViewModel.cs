using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models.ViewModels;

public class StartPageViewModel : PageViewModel<Start>
{
    public TrailFilter Filter { get; init; } = new();

    public IReadOnlyList<Trail> Trails { get; init; } = [];

    public bool HasTrails { get; init; }
}