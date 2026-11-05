namespace OutdoorGuide.Models;

/// <summary>
/// Det ledsidan visar om betygen. <paramref name="Average"/> är avrundat till en decimal och <c>null</c> om ingen har röstat.
/// <paramref name="MemberScore"/> är medlemmens egen röst, <c>null</c> om medlemmen inte har röstat eller inte är inloggad.
/// </summary>
public record TrailRatingSummary(double? Average, int Count, int? MemberScore);
