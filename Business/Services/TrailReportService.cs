using Umbraco.Cms.Persistence.EFCore.Scoping;
using OutdoorGuide.Business.Data;
using OutdoorGuide.Business.Services.Interfaces;
using OutdoorGuide.Models;

namespace OutdoorGuide.Business.Services;

public class TrailReportService(IEFCoreScopeProvider<TrailReportDbContext> scopeProvider, TimeProvider timeProvider)
	: ITrailReportService
{
	public async Task<TrailReport> AddAsync(Guid trailKey, string condition, string comment, string? name, string culture)
	{
		var report = new TrailReport
		{
			TrailKey = trailKey,
			Condition = condition,
			Comment = Truncate(comment, TrailReport.CommentMaxLength),
			Name = string.IsNullOrWhiteSpace(name) ? null : Truncate(name, TrailReport.NameMaxLength),
			Culture = Truncate(culture, TrailReport.CultureMaxLength),
			CreatedUtc = timeProvider.GetUtcNow().UtcDateTime,
		};

		// Umbracos scope, eftersom tabellen delar anslutning med Umbraco (shareUmbracoConnection i composern)
		using var scope = scopeProvider.CreateScope();

		await scope.ExecuteWithContextAsync<Task>(async db =>
		{
			db.TrailReports.Add(report);
			await db.SaveChangesAsync();
		});

		scope.Complete();

		return report;
	}

	private static string Truncate(string value, int maxLength)
	{
		value = value.Trim();

		return value.Length > maxLength ? value[..maxLength] : value;
	}
}