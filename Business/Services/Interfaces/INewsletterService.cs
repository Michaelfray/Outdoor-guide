namespace OutdoorGuide.Business.Services.Interfaces;

public interface INewsletterService
{

	Task<bool> SubscribeAsync(string email, string culture);
}