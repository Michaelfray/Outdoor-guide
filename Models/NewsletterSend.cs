namespace OutdoorGuide.Models;

/// <summary>Ett nyhetsbrev som har köats för utskick. Finns raden skickas nyhetsbrevet inte igen (OUT-117).</summary>
public class NewsletterSend
{
	public int Id { get; set; }

	public Guid NewsletterKey { get; set; }

	public DateTime QueuedUtc { get; set; }
}