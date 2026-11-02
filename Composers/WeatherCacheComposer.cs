using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;
using OutdoorGuide.Business.Data;
using OutdoorGuide.Business.Notifications;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OutdoorGuide.Business.Services;
using OutdoorGuide.Business.Services.Interfaces;

namespace OutdoorGuide.Composers;

public class WeatherCacheComposer : IComposer
{
	public void Compose(IUmbracoBuilder builder)
	{
		builder.Services.AddUmbracoDbContext<WeatherCacheDbContext>(
			(serviceProvider, options, _, _) => options.UseUmbracoDatabaseProvider(serviceProvider),
			shareUmbracoConnection: true);

		builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, WeatherCacheMigrationHandler>();

		// TryAdd, så att det inte spelar någon roll om TrailReportComposer också registrerar den
		builder.Services.TryAddSingleton(TimeProvider.System);
		builder.Services.AddScoped<IWeatherCacheService, WeatherCacheService>();
	}
}
