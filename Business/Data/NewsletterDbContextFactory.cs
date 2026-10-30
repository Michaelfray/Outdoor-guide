using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OutdoorGuide.Business.Data;


public class NewsletterDbContextFactory : IDesignTimeDbContextFactory<NewsletterDbContext>
{
	public NewsletterDbContext CreateDbContext(string[] args)
		=> new(new DbContextOptionsBuilder<NewsletterDbContext>().UseSqlite().Options);
}