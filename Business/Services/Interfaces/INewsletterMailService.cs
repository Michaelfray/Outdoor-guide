using OutdoorGuide.Models;

namespace OutdoorGuide.Business.Services.Interfaces;

public interface INewsletterMailService
{
	/// <summary>Sparar att nyhetsbrevet är köat. Ger false om det redan har köats, och då ska det inte skickas igen.</summary>
	Task<bool> TryMarkQueuedAsync(Guid newsletterKey);

	/// <summary>Skickar nyhetsbrevet till alla prenumeranter som inte redan har fått det.</summary>
	Task<NewsletterSendResult> SendAsync(Guid newsletterKey);
}