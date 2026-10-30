using Microsoft.EntityFrameworkCore;
using OutdoorGuide.Models;

namespace OutdoorGuide.Business.Data;

public class NewsletterDbContext(DbContextOptions<NewsletterDbContext> options) : DbContext(options)
{
	public DbSet<NewsletterSubscription> Subscriptions => Set<NewsletterSubscription>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<NewsletterSubscription>(subscription =>
		{
			subscription.ToTable("newsletterSubscription");
			subscription.HasKey(x => x.Id);
			subscription.Property(x => x.Id).ValueGeneratedOnAdd();
			subscription.Property(x => x.Email).HasMaxLength(NewsletterSubscription.EmailMaxLength).IsRequired();
			subscription.Property(x => x.Culture).HasMaxLength(NewsletterSubscription.CultureMaxLength).IsRequired();

			// SQLite sparar inte vilken sorts tid det är, så tala om att den är UTC när den läses
			subscription.Property(x => x.CreatedUtc)
				.HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

			// Samma adress får bara finnas en gång
			subscription.HasIndex(x => x.Email).IsUnique();
		});
	}
}