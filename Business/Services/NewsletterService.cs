using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using OutdoorGuide.Business.Data;
using OutdoorGuide.Business.Services.Interfaces;
using OutdoorGuide.Models;

namespace OutdoorGuide.Business.Services;

public class NewsletterService(
	IEFCoreScopeProvider<NewsletterDbContext> scopeProvider,
	TimeProvider timeProvider,
	ILogger<NewsletterService> logger)
	: INewsletterService
{
	public async Task<bool> SubscribeAsync(string email, string culture)
	{
		var normalizedEmail = email.Trim().ToLowerInvariant();

		try
		{
			using var scope = scopeProvider.CreateScope();

			var added = await scope.ExecuteWithContextAsync(async db =>
			{
				if (await db.Subscriptions.AnyAsync(x => x.Email == normalizedEmail))
				{
					return false;
				}

				db.Subscriptions.Add(new NewsletterSubscription
				{
					Email = normalizedEmail,
					Culture = Truncate(culture, NewsletterSubscription.CultureMaxLength),
					CreatedUtc = timeProvider.GetUtcNow().UtcDateTime,
					UnsubscribeToken = NewsletterSubscription.NewUnsubscribeToken(),
				});

				await db.SaveChangesAsync();

				return true;
			});

			scope.Complete();

			return added;
		}
		catch (DbUpdateException ex)
		{
			logger.LogInformation(ex, "En anmälan till nyhetsbrevet fanns redan. Inget sparades.");
			return false;
		}
	}

		public async Task<bool> UnsubscribeAsync(string token)
	{
		token = token.Trim();

		if (token.Length == 0 || token.Length > NewsletterSubscription.UnsubscribeTokenLength)
		{
			return false;
		}

		using var scope = scopeProvider.CreateScope();

		var removed = await scope.ExecuteWithContextAsync(async db =>
			await db.Subscriptions.Where(x => x.UnsubscribeToken == token).ExecuteDeleteAsync() > 0);

		scope.Complete();

		logger.LogInformation(removed
			? "En prenumeration på nyhetsbrevet avslutades."
			: "Ingen prenumeration med den token. Länken var redan använd eller fel.");

		return removed;
	}

	private static string Truncate(string value, int maxLength)
	{
		value = value.Trim();

		return value.Length > maxLength ? value[..maxLength] : value;
	}
}