namespace OutdoorGuide.Models;

/// <summary>Värdena i frågesträngen efter en röst, så att ledsidan kan visa rätt meddelande vid betygen.</summary>
public static class TrailRatingStatus
{
	public const string QueryKey = "rating";
	public const string Anchor = "trail-rating";

	public const string Saved = "saved";
	public const string Invalid = "invalid";
	public const string Expired = "expired";
	public const string LoginRequired = "login";
	public const string Retry = "retry";
}
