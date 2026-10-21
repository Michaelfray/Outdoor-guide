using OutdoorGuide.Models;

namespace OutdoorGuide.Business.Services.Interfaces;

public interface ITrailReportService
{
	/// <summary>Sparar en rapport om ledens skick.</summary>
	Task<TrailReport> AddAsync(Guid trailKey, string condition, string comment, string? name, string culture);
}