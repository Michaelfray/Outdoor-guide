using Hangfire;
using OutdoorGuide.Business.Jobs.Interfaces;
using OutdoorGuide.Business.Services.Interfaces;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Business.Notifications;

/// <summary>Köar utskicket när ett nyhetsbrev publiceras första gången. Sidan svarar direkt, Hangfire skickar (OUT-117).</summary>
public class NewsletterPublishedHandler(
	INewsletterMailService mailService,
	IBackgroundJobClient backgroundJobClient,
	ILogger<NewsletterPublishedHandler> logger)
	: INotificationAsyncHandler<ContentPublishedNotification>
{
	public async Task HandleAsync(ContentPublishedNotification notification, CancellationToken cancellationToken)
	{
		foreach (var newsletter in notification.PublishedEntities.Where(x => x.ContentType.Alias == Newsletter.ModelTypeAlias))
		{
			// Publiceras det igen efter en ändring skickas det inte en gång till
			if (!await mailService.TryMarkQueuedAsync(newsletter.Key))
			{
				logger.LogInformation("Nyhetsbrevet {Name} är redan skickat, så inget nytt utskick.", newsletter.Name);
				continue;
			}

			var key = newsletter.Key;
			backgroundJobClient.Enqueue<INewsletterJob>(job => job.SendAsync(key, null!));
		}
	}
}