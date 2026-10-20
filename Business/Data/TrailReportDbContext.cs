using Microsoft.EntityFrameworkCore;
using OutdoorGuide.Models;

namespace OutdoorGuide.Business.Data;

public class TrailReportDbContext(DbContextOptions<TrailReportDbContext> options) : DbContext(options)
{
	public DbSet<TrailReport> TrailReports => Set<TrailReport>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<TrailReport>(report =>
		{
			report.ToTable("trailReport");
			report.HasKey(x => x.Id);
			report.Property(x => x.Id).ValueGeneratedOnAdd();
			report.Property(x => x.Condition).HasMaxLength(TrailReport.ConditionMaxLength).IsRequired();
			report.Property(x => x.Comment).HasMaxLength(TrailReport.CommentMaxLength).IsRequired();
			report.Property(x => x.Name).HasMaxLength(TrailReport.NameMaxLength);
			report.Property(x => x.Culture).HasMaxLength(TrailReport.CultureMaxLength).IsRequired();

			
			report.Property(x => x.CreatedUtc)
				.HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

			report.HasIndex(x => new { x.TrailKey, x.CreatedUtc });
		});
	}
}