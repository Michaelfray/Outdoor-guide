using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models.ViewModels;

public class SearchPageViewModel : PageViewModel<SearchPage>
{
    /// <summary>Det besökaren sökte på, utan mellanslag runt om. Tom när inget har sökts.</summary>
    public string Query { get; init; } = string.Empty;

    /// <summary>Träffarna. Tom när inget har sökts, när sökordet är för långt eller när inget hittades.</summary>
    public IReadOnlyList<SearchHit> Results { get; init; } = [];
}
