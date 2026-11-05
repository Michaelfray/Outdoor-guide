namespace OutdoorGuide.Models;


public static class LengthValues
{
    public const string Short = "short";
    public const string Medium = "medium";
    public const string Long = "long";

    public const decimal ShortMax = 5;

    public const decimal LongMin = 15;

    public static readonly IReadOnlyList<string> All = [Short, Medium, Long];

    public static string DictionaryKey(string? value) => value switch
    {
        Short => "Filter.Length.Short",
        Medium => "Filter.Length.Medium",
        Long => "Filter.Length.Long",
        _ => "Filter.Length",
    };

  
    public static string? GroupOf(decimal lengthKm) => lengthKm switch
    {
        <= 0 => null,
        < ShortMax => Short,
        <= LongMin => Medium,
        _ => Long,
    };
}