using Microsoft.AspNetCore.Http;

namespace OutdoorGuide.Models;

/// <summary>
/// Besökarens val i cookie-rutan (OUT-77). Valet sparas i en egen cookie i ett år. Nödvändiga cookies (inloggning,
/// formulärens skydd mot förfalskning) används alltid. Allt annat, till exempel statistik, får bara läggas till när
/// <see cref="AcceptsAll"/> är sant.
/// </summary>
public static class CookieConsent
{
	public const string CookieName = "og_cookie_consent";

	public const string Necessary = "necessary";
	public const string All = "all";

	/// <summary>Har besökaren redan valt? Då visas inte rutan.</summary>
	public static bool HasChosen(HttpRequest request) =>
		request.Cookies[CookieName] is Necessary or All;

	/// <summary>Har besökaren godkänt alla cookies, inte bara de nödvändiga?</summary>
	public static bool AcceptsAll(HttpRequest request) =>
		request.Cookies[CookieName] == All;
}
