using Hangfire.Server;

namespace OutdoorGuide.Business.Jobs.Interfaces;

public interface IWeatherJob
{
	/// <summary>Hämtar prognosen för alla leder med koordinater och sparar den i väder-cachen.</summary>
	Task UpdateForecastsAsync(PerformContext context);
}
