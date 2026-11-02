using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models.ViewModels;

public class TrailPageViewModel : PageViewModel<Trail>
{
    /// <summary>Prognosen vid ledens koordinater, eller null när Open-Meteo inte svarar.</summary>
    public WeatherForecast? Forecast { get; init; }

    /// <summary>När prognosen hämtades från Open-Meteo, i UTC. Null när det inte finns någon prognos.</summary>
    public DateTime? FetchedUtc { get; init; }

    /// <summary>Om prognosen är så gammal att den kan vara inaktuell. Gränsen står i TrailController.</summary>
    public bool IsStale { get; init; }

    /// <summary>Om leden har koordinater. Utan dem visas inget väder alls.</summary>
    public bool HasCoordinates { get; init; }
}