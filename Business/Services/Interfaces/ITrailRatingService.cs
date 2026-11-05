using OutdoorGuide.Models;

namespace OutdoorGuide.Business.Services.Interfaces;

public interface ITrailRatingService
{
	/// <summary>Sparar medlemmens betyg på leden. Har medlemmen redan röstat skrivs betyget över.</summary>
	/// <exception cref="ArgumentOutOfRangeException">Betyget ligger utanför <see cref="TrailRating.MinScore"/> och <see cref="TrailRating.MaxScore"/>.</exception>
	Task SaveAsync(Guid trailKey, Guid memberKey, int score);

	/// <summary>Snittet och antalet röster för leden, och medlemmens egen röst. Skicka <c>null</c> om besökaren inte är inloggad.</summary>
	Task<TrailRatingSummary> GetSummaryAsync(Guid trailKey, Guid? memberKey);
}
