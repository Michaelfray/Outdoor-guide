namespace OutdoorGuide.Models;

/// <summary>Ett mejl som har gått fram. Gör att jobbet kan köras om utan att någon får mejlet två gånger (OUT-117).</summary>
public class NewsletterDelivery
{
	public int Id { get; set; }

	public Guid NewsletterKey { get; set; }

	public int SubscriptionId { get; set; }

	public DateTime SentUtc { get; set; }
}