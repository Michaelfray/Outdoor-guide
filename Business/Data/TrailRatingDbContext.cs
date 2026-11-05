using Microsoft.EntityFrameworkCore;
using OutdoorGuide.Models;

namespace OutdoorGuide.Business.Data;

public class TrailRatingDbContext(DbContextOptions<TrailRatingDbContext> options) : DbContext(options)
{
	public DbSet<TrailRating> Ratings => Set<TrailRating>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<TrailRating>(rating =>
		{
			rating.ToTable("trailRating");
			rating.HasKey(x => x.Id);
			rating.Property(x => x.Id).ValueGeneratedOnAdd();

			// SQLite sparar inte vilken sorts tid det är, så tala om att den är UTC när den läses
			rating.Property(x => x.UpdatedUtc)
				.HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

			// En rad per medlem och led, den skrivs över när medlemmen röstar igen
			rating.HasIndex(x => new { x.TrailKey, x.MemberKey }).IsUnique();
		});
	}
}
