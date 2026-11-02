using Cultiv.Hangfire;
using Hangfire;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using OutdoorGuide.Business.Jobs;
using OutdoorGuide.Business.Jobs.Interfaces;

namespace OutdoorGuide.Composers;

/// <summary>Schemalägger jobben i Hangfire, som Carls ScheduledJobsComposer.</summary>
[ComposeAfter(typeof(HangfireComposer))]
public class ScheduledJobsComposer : IComposer
{
	public void Compose(IUmbracoBuilder builder)
	{
		// Carl registrerar jobbet i Program.cs, men här ligger det med schemat
		builder.Services.AddScoped<IWeatherJob, WeatherJob>();

		// null byts ut mot en riktig PerformContext när Hangfire kör jobbet
		RecurringJob.AddOrUpdate<IWeatherJob>("update-weather-forecasts", job => job.UpdateForecastsAsync(null!), Cron.Hourly());
	}
}
