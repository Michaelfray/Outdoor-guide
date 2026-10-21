namespace OutdoorGuide.Models;

/// <summary>
/// De värden som datatypen "Activity" sparar. Neutrala, så att samma värde gäller på båda språken.
/// Etiketterna finns som dictionary items under <c>Trail.Activity</c>.
/// </summary>
/// <remarks>
/// Vinter heter <c>winter</c> och inte <c>skiing</c>, så att snöskor också får plats (beslut 21/10, OUT-31).
/// Ett värde som har pushats går inte att byta, eftersom lederna redan har sparat det.
/// </remarks>
public static class ActivityValues
{
    public const string Hiking = "hiking";
    public const string Paddling = "paddling";
    public const string Cycling = "cycling";
    public const string Winter = "winter";

    /// <summary>Alla värden, i den ordning redaktören ser dem och som menyns kolumner.</summary>
    public static readonly IReadOnlyList<string> All = [Hiking, Paddling, Cycling, Winter];

    /// <summary>Dictionary-nyckeln för etiketten, till exempel <c>Trail.Activity.Hiking</c>.</summary>
    public static string DictionaryKey(string? value) => value switch
    {
        Hiking => "Trail.Activity.Hiking",
        Paddling => "Trail.Activity.Paddling",
        Cycling => "Trail.Activity.Cycling",
        Winter => "Trail.Activity.Winter",
        _ => "Trail.Activity",
    };
}
