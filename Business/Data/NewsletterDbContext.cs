using Microsoft.EntityFrameworkCore;
using OutdoorGuide.Models;

namespace OutdoorGuide.Business.Data;

public class NewsletterDbContext(DbContextOptions<NewsletterDbContext> options) : DbContext(options)
{
	public DbSet<NewsletterSubscription> Subscriptions => Set<NewsletterSubscription>();

	public DbSet<NewsletterSend> Sends => Set<NewsletterSend>();

	public DbSet<NewsletterDelivery> Deliveries => Set<NewsletterDelivery>();

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

		modelBuilder.Entity<NewsletterSend>(send =>
		{
			send.ToTable("newsletterSend");
			send.HasKey(x => x.Id);
			send.Property(x => x.Id).ValueGeneratedOnAdd();
			send.Property(x => x.QueuedUtc)
				.HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

			// Ett nyhetsbrev köas bara en gång, även om två publiceringar kommer samtidigt
			send.HasIndex(x => x.NewsletterKey).IsUnique();
		});

		modelBuilder.Entity<NewsletterDelivery>(delivery =>
		{
			delivery.ToTable("newsletterDelivery");
			delivery.HasKey(x => x.Id);
			delivery.Property(x => x.Id).ValueGeneratedOnAdd();
			delivery.Property(x => x.SentUtc)
				.HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

			// Samma prenumerant får samma nyhetsbrev bara en gång
			delivery.HasIndex(x => new { x.NewsletterKey, x.SubscriptionId }).IsUnique();
		});
	}
}