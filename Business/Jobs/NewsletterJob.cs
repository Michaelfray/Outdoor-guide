using Hangfire.Console;
using Hangfire.Server;
using OutdoorGuide.Business.Jobs.Interfaces;
using OutdoorGuide.Business.Services.Interfaces;

namespace OutdoorGuide.Business.Jobs;

/// <summary>Skickar ett nyhetsbrev. Köas av NewsletterPublishedHandler när redaktören publicerar (OUT-117).</summary>
public class NewsletterJob(INewsletterMailService mailService) : INewsletterJob
{
	public async Task SendAsync(Guid newsletterKey, PerformContext context)
	{
		var result = await mailService.SendAsync(newsletterKey);

		context.WriteLine($"Skickade: {result.Sent}, misslyckade: {result.Failed}, hoppade över: {result.Skipped}");

		// Ett fel gör jobbet Failed i dashboarden. Körs det om får bara de som saknas mejlet
		if (result.Failed > 0)
		{
			throw new InvalidOperationException($"{result.Failed} mejl gick inte fram. Kör jobbet igen när felet är rättat.");
		}
	}
}