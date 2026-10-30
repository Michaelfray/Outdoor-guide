using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;
using OutdoorGuide.Business.Data;
using OutdoorGuide.Business.Notifications;

namespace OutdoorGuide.Composers;

public class NewsletterComposer : IComposer
{
	public void Compose(IUmbracoBuilder builder)
	{
		builder.Services.AddUmbracoDbContext<NewsletterDbContext>(
			(serviceProvider, options, _, _) => options.UseUmbracoDatabaseProvider(serviceProvider),
			shareUmbracoConnection: true);

		builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, NewsletterMigrationHandler>();
	}
}