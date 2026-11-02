using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace OutdoorGuide.Models;

/// <summary>
/// Sparar e-postadressen över en redirect när anmälan var ogiltig, så att besökaren slipper skriva den igen.
/// TempData läses en gång och försvinner sedan.
/// </summary>
public static class NewsletterTempData
{
	private const string EmailKey = "Newsletter.Email";

	public static void Keep(ITempDataDictionary tempData, string? email)
		=> tempData[EmailKey] = Limit(email, NewsletterSubscription.EmailMaxLength);

	public static string? Take(ITempDataDictionary tempData)
		=> tempData.ContainsKey(EmailKey) ? tempData[EmailKey] as string : null;

	private static string? Limit(string? value, int maxLength)
		=> value is { Length: > 0 } && value.Length > maxLength ? value[..maxLength] : value;
}