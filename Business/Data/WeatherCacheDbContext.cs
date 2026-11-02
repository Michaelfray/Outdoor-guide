using Microsoft.EntityFrameworkCore;
using OutdoorGuide.Models;

namespace OutdoorGuide.Business.Data;

public class WeatherCacheDbContext(DbContextOptions<WeatherCacheDbContext> options) : DbContext(options)
{
	public DbSet<WeatherCacheEntry> Entries => Set<WeatherCacheEntry>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<WeatherCacheEntry>(entry =>
		{
			entry.ToTable("weatherCache");
			entry.HasKey(x => x.Id);
			entry.Property(x => x.Id).ValueGeneratedOnAdd();
			entry.Property(x => x.ForecastJson).IsRequired();

			// SQLite sparar inte vilken sorts tid det är, så tala om att den är UTC när den läses
			entry.Property(x => x.FetchedUtc)
				.HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

			// En rad per led, den skrivs över vid varje hämtning
			entry.HasIndex(x => x.TrailKey).IsUnique();
		});
	}
}
