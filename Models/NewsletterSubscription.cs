using System.Security.Cryptography;

namespace OutdoorGuide.Models;

/// <summary>En anmälan till nyhetsbrevet, gjord med formuläret i sidfoten. Sparas i Umbracos databas.</summary>
public class NewsletterSubscription
{
	/// <summary>Den längsta e-postadress som är tillåten enligt standarden (RFC 5321).</summary>
	public const int EmailMaxLength = 254;
	public const int CultureMaxLength = 16;

	/// <summary>32 slumpade byte som hex-text (OUT-118).</summary>
	public const int UnsubscribeTokenLength = 64;

	public int Id { get; set; }

	public string Email { get; set; } = string.Empty;

	public string Culture { get; set; } = string.Empty;

	public DateTime CreatedUtc { get; set; }

	/// <summary>Den hemliga delen av avregistreringslänken i varje mejl (OUT-118).</summary>
	public string UnsubscribeToken { get; set; } = string.Empty;

	/// <summary>En ny token som inte går att gissa.</summary>
	public static string NewUnsubscribeToken()
		=> Convert.ToHexString(RandomNumberGenerator.GetBytes(UnsubscribeTokenLength / 2)).ToLowerInvariant();
}