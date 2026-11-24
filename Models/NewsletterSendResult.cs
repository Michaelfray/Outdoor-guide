namespace OutdoorGuide.Models;

/// <summary>Hur det gick för ett utskick: skickade, misslyckade och överhoppade (inget publicerat språk).</summary>
public record NewsletterSendResult(int Sent, int Failed, int Skipped);