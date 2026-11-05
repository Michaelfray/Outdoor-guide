using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models;


public class TrailFilter
{
    public const string ActivityKey = "activity";
    public const string DifficultyKey = "difficulty";
    public const string LengthKey = "length";
    public const string ChildFriendlyKey = "childFriendly";

    public string? Activity { get; init; }

    public string? Difficulty { get; init; }

    public string? Length { get; init; }

    public bool ChildFriendly { get; init; }

    public bool IsActive => Activity != null || Difficulty != null || Length != null || ChildFriendly;

    public static TrailFilter FromQuery(IQueryCollection query) => new()
    {
        Activity = Known(query[ActivityKey], ActivityValues.All),
        Difficulty = Known(query[DifficultyKey], DifficultyValues.All),
        Length = Known(query[LengthKey], LengthValues.All),
        ChildFriendly = bool.TryParse(query[ChildFriendlyKey], out var childFriendly) && childFriendly,
    };

    public IEnumerable<Trail> Apply(IEnumerable<Trail> trails) => trails.Where(Matches);

    public bool Matches(Trail trail)
        => (Activity == null || trail.Activity == Activity)
            && (Difficulty == null || trail.Difficulty == Difficulty)
            && (Length == null || LengthValues.GroupOf(trail.Length) == Length)
            && (!ChildFriendly || trail.ChildFriendly);

    private static string? Known(string? value, IReadOnlyList<string> allowed)
    {
        var normalized = value?.Trim().ToLowerInvariant();
        return normalized != null && allowed.Contains(normalized) ? normalized : null;
    }
}