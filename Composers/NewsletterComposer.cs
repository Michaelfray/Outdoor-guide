using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;
using OutdoorGuide.Business.Data;
using OutdoorGuide.Business.Notifications;
using OutdoorGuide.Business.Services;
using OutdoorGuide.Business.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OutdoorGuide.Business.Jobs.Interfaces;
using OutdoorGuide.Business.Jobs;

namespace OutdoorGuide.Composers;

public class NewsletterComposer : IComposer
{
	public void Compose(IUmbracoBuilder builder)
	{
		builder.Services.AddUmbracoDbContext<NewsletterDbContext>(
			(serviceProvider, options, _, _) => options.UseUmbracoDatabaseProvider(serviceProvider),
			shareUmbracoConnection: true);

		builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, NewsletterMigrationHandler>();
        builder.Services.TryAddSingleton(TimeProvider.System);
        builder.Services.AddScoped<INewsletterService, NewsletterService>();
        builder.Services.AddScoped<INewsletterMailService, NewsletterMailService>();
		builder.Services.AddScoped<INewsletterJob, NewsletterJob>();
        builder.AddNotificationAsyncHandler<ContentPublishedNotification, NewsletterPublishedHandler>();
	}
}