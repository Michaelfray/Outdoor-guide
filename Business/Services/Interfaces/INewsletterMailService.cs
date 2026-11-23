namespace OutdoorGuide.Business.Services.Interfaces;

public interface INewsletterMailService
{
	/// <summary>Skickar nyhetsbrevet till alla prenumeranter. Returnerar hur många mejl som skickades.</summary>
	Task<int> SendAsync(Guid newsletterKey);
}