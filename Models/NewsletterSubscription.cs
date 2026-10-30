namespace OutdoorGuide.Models;

/// <summary>En anmälan till nyhetsbrevet, gjord med formuläret i sidfoten. Sparas i Umbracos databas.</summary>
public class NewsletterSubscription
{
	/// <summary>Den längsta e-postadress som är tillåten enligt standarden (RFC 5321).</summary>
	public const int EmailMaxLength = 254;
	public const int CultureMaxLength = 16;

	public int Id { get; set; }

	public string Email { get; set; } = string.Empty;

	public string Culture { get; set; } = string.Empty;

	public DateTime CreatedUtc { get; set; }
}