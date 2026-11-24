namespace OutdoorGuide.Business.Services.Interfaces;

public interface INewsletterService
{
	Task<bool> SubscribeAsync(string email, string culture);

	/// <summary>Tar bort prenumerationen med den här token. Ger false om ingen fanns, till exempel om länken redan är använd.</summary>
	Task<bool> UnsubscribeAsync(string token);
}