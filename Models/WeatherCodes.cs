namespace OutdoorGuide.Models;

/// <summary>
/// WMO-koderna från Open-Meteo, grupperade till texter i dictionary (<c>Weather.Code.*</c>).
/// Koderna: https://open-meteo.com/en/docs (WMO Weather interpretation codes).
/// </summary>
public static class WeatherCodes
{
    public static string Name(int code) => code switch
    {
        0 => "Clear",
        1 => "MainlyClear",
        2 => "PartlyCloudy",
        3 => "Overcast",
        45 or 48 => "Fog",
        >= 51 and <= 57 => "Drizzle",
        (>= 61 and <= 67) or (>= 80 and <= 82) => "Rain",
        (>= 71 and <= 77) or 85 or 86 => "Snow",
        >= 95 and <= 99 => "Thunderstorm",
        _ => "Unknown",
    };

    public static string DictionaryKey(int code) => $"Weather.Code.{Name(code)}";
}