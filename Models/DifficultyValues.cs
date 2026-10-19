namespace OutdoorGuide.Models;

/// <summary>
/// De värden som datatypen "Difficulty" sparar. Neutrala, så att samma värde gäller på båda språken.
/// Etiketterna finns som dictionary items under <c>Trail.Difficulty</c>.
/// </summary>
public static class DifficultyValues
{
    public const string Easy = "easy";
    public const string Medium = "medium";
    public const string Hard = "hard";

    /// <summary>Alla värden, i den ordning redaktören ser dem.</summary>
    public static readonly IReadOnlyList<string> All = [Easy, Medium, Hard];

    /// <summary>Dictionary-nyckeln för etiketten, till exempel <c>Trail.Difficulty.Easy</c>.</summary>
    public static string DictionaryKey(string? value) => value switch
    {
        Easy => "Trail.Difficulty.Easy",
        Medium => "Trail.Difficulty.Medium",
        Hard => "Trail.Difficulty.Hard",
        _ => "Trail.Difficulty",
    };
}