using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models.ViewModels;

public class TrailPageViewModel : PageViewModel<Trail>
{
    /// <summary>Prognosen vid ledens koordinater, eller null när Open-Meteo inte svarar.</summary>
    public WeatherForecast? Forecast { get; init; }

    /// <summary>Om leden har koordinater. Utan dem visas inget väder alls.</summary>
    public bool HasCoordinates { get; init; }
}