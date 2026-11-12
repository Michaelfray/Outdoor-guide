using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using OutdoorGuide.Business.Data;
using OutdoorGuide.Business.Services.Interfaces;
using OutdoorGuide.Models;

namespace OutdoorGuide.Business.Services;

public class TrailRatingService(IEFCoreScopeProvider<TrailRatingDbContext> scopeProvider, TimeProvider timeProvider)
	: ITrailRatingService
{
	public async Task SaveAsync(Guid trailKey, Guid memberKey, int score)
	{
		// Controllern validerar också, men servicen litar inte på att alla som anropar den gör det
		if (score is < TrailRating.MinScore or > TrailRating.MaxScore)
		{
			throw new ArgumentOutOfRangeException(
				nameof(score),
				score,
				$"Betyget ska vara mellan {TrailRating.MinScore} och {TrailRating.MaxScore}.");
		}

		var now = timeProvider.GetUtcNow().UtcDateTime;

		// Umbracos scope, eftersom tabellen delar anslutning med Umbraco (shareUmbracoConnection i composern)
		using var scope = scopeProvider.CreateScope();

		await scope.ExecuteWithContextAsync<Task>(async db =>
		{
			// Paret led och medlem är unikt, så en befintlig röst uppdateras i stället för att en ny läggs till
			var rating = await db.Ratings.FirstOrDefaultAsync(x => x.TrailKey == trailKey && x.MemberKey == memberKey);

			if (rating is null)
			{
				db.Ratings.Add(new TrailRating
				{
					TrailKey = trailKey,
					MemberKey = memberKey,
					Score = score,
					UpdatedUtc = now,
				});
			}
			else
			{
				rating.Score = score;
				rating.UpdatedUtc = now;
			}

			await db.SaveChangesAsync();
		});

		scope.Complete();
	}

	public async Task<TrailRatingSummary> GetSummaryAsync(Guid trailKey, Guid? memberKey)
	{
		using var scope = scopeProvider.CreateScope();

		var ratings = await scope.ExecuteWithContextAsync(async db =>
			await db.Ratings
				.AsNoTracking()
				.Where(x => x.TrailKey == trailKey)
				.Select(x => new { x.MemberKey, x.Score })
				.ToListAsync());

		scope.Complete();

		// Snittet räknas här och inte i databasen, så att det fungerar likadant på SQLite och SQL Server
		double? average = ratings.Count > 0 ? Math.Round(ratings.Average(x => x.Score), 1, MidpointRounding.AwayFromZero) : null;

		// Är memberKey null matchar ingen rad, och då blir MemberScore null
		var memberScore = ratings.FirstOrDefault(x => x.MemberKey == memberKey)?.Score;

		return new TrailRatingSummary(average, ratings.Count, memberScore);
	}

	public async Task<IReadOnlyDictionary<Guid, TrailRatingSummary>> GetSummariesAsync(IReadOnlyCollection<Guid> trailKeys)
	{
		if (trailKeys.Count == 0)
		{
			return new Dictionary<Guid, TrailRatingSummary>();
		}

		using var scope = scopeProvider.CreateScope();

		// En fråga för alla leder i stället för en per kort
		var ratings = await scope.ExecuteWithContextAsync(async db =>
			await db.Ratings
				.AsNoTracking()
				.Where(x => trailKeys.Contains(x.TrailKey))
				.Select(x => new { x.TrailKey, x.Score })
				.ToListAsync());

		scope.Complete();

		// Samma avrundning som i GetSummaryAsync, så att kortet och ledsidan visar samma snitt
		return ratings
			.GroupBy(x => x.TrailKey)
			.ToDictionary(
				g => g.Key,
				g => new TrailRatingSummary(
					Math.Round(g.Average(x => x.Score), 1, MidpointRounding.AwayFromZero),
					g.Count(),
					null));
	}
}
