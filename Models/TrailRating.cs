namespace OutdoorGuide.Models;

/// <summary>En medlems betyg på en led. Sparas i Umbracos databas, en rad per medlem och led.</summary>
public class TrailRating
{
	public const int MinScore = 1;
	public const int MaxScore = 5;

	public int Id { get; set; }

	/// <summary>Ledsidan betyget gäller.</summary>
	public Guid TrailKey { get; set; }

	/// <summary>Medlemmen som röstade.</summary>
	public Guid MemberKey { get; set; }

	/// <summary>Antal hela stjärnor, från <see cref="MinScore"/> till <see cref="MaxScore"/>.</summary>
	public int Score { get; set; }

	/// <summary>När medlemmen röstade senast.</summary>
	public DateTime UpdatedUtc { get; set; }
}
