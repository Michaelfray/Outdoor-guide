namespace OutdoorGuide.Models;

/// <summary>En rapport om ledens skick, skickad med formuläret på ledsidan. Sparas i Umbracos databas.</summary>
public class TrailReport
{
	public const int ConditionMaxLength = 32;
	public const int CommentMaxLength = 1000;
	public const int NameMaxLength = 100;
	public const int CultureMaxLength = 16;

	public int Id { get; set; }

	/// <summary>Ledsidan rapporten gäller.</summary>
	public Guid TrailKey { get; set; }

	/// <summary>Skicket besökaren valde, till exempel <c>good</c>, <c>obstacle</c> eller <c>closed</c>.</summary>
	public string Condition { get; set; } = string.Empty;

	public string Comment { get; set; } = string.Empty;

	/// <summary>Frivilligt.</summary>
	public string? Name { get; set; }

	/// <summary>Språket sidan visades på, till exempel <c>sv-SE</c> eller <c>en-US</c>.</summary>
	public string Culture { get; set; } = string.Empty;

	public DateTime CreatedUtc { get; set; }
}

/// kanske lägga till e-post?
/// 