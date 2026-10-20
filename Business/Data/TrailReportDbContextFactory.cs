using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OutdoorGuide.Business.Data;

/// <summary>
/// Används bara  så att migrationer kan genereras utan att Umbraco startar.
/// Det behövs ingen databas för att generera en migration, så det finns ingen connection string.
/// </summary>
public class TrailReportDbContextFactory : IDesignTimeDbContextFactory<TrailReportDbContext>
{
	public TrailReportDbContext CreateDbContext(string[] args)
		=> new(new DbContextOptionsBuilder<TrailReportDbContext>().UseSqlite().Options);
}