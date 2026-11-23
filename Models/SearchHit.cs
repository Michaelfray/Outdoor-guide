using Umbraco.Cms.Core.Models.PublishedContent;

namespace OutdoorGuide.Models;

/// <summary>En träff i sökningen. Typen gör att vyn kan visa om det är en led eller en kommun.</summary>
public record SearchHit(IPublishedContent Page, string Name, string Url, SearchHitType Type);

public enum SearchHitType
{
	Trail,
	Municipality,
}
