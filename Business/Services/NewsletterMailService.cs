using System.Net;
using Microsoft.EntityFrameworkCore;
using OutdoorGuide.Business.Data;
using OutdoorGuide.Business.Services.Interfaces;
using OutdoorGuide.Models;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Business.Services;

/// <summary>
/// Skickar ett publicerat nyhetsbrev som HTML-mejl till varje prenumerant, på prenumerantens språk (OUT-116).
/// Sparar vilka nyhetsbrev som är köade och vilka mejl som har gått fram, så att inget skickas två gånger (OUT-117).
/// Varje mejl har en länk där prenumeranten avslutar nyhetsbrevet (OUT-118).
/// </summary>
public class NewsletterMailService(
	IEFCoreScopeProvider<NewsletterDbContext> scopeProvider,
	IUmbracoContextFactory umbracoContextFactory,
	IEmailSender emailSender,
	IDictionaryItemService dictionaryItemService,
	TimeProvider timeProvider,
	ILogger<NewsletterMailService> logger) : INewsletterMailService
{
	// Används när nyhetsbrevet inte är publicerat på prenumerantens språk
	private const string DefaultCulture = "en-US";
	private const string EmailType = "Newsletter";

	public async Task<bool> TryMarkQueuedAsync(Guid newsletterKey)
	{
		try
		{
			using var scope = scopeProvider.CreateScope();

			var added = await scope.ExecuteWithContextAsync(async db =>
			{
				if (await db.Sends.AnyAsync(x => x.NewsletterKey == newsletterKey))
				{
					return false;
				}

				db.Sends.Add(new NewsletterSend
				{
					NewsletterKey = newsletterKey,
					QueuedUtc = timeProvider.GetUtcNow().UtcDateTime,
				});

				await db.SaveChangesAsync();

				return true;
			});

			scope.Complete();

			return added;
		}
		catch (DbUpdateException)
		{
			// Två publiceringar samtidigt: det unika indexet stoppade den andra
			return false;
		}
	}

	public async Task<NewsletterSendResult> SendAsync(Guid newsletterKey)
	{
		// Ett fel här gör att Hangfire försöker igen, och jobbet kan köras om när felet är rättat
		if (!emailSender.CanSendRequiredEmail())
		{
			throw new InvalidOperationException("SMTP är inte inställt, så nyhetsbrevet kan inte skickas.");
		}

		// Jobbet körs utanför en webbförfrågan, så Umbraco-kontexten skapas här
		using var contextReference = umbracoContextFactory.EnsureUmbracoContext();

		if (contextReference.UmbracoContext.Content?.GetById(newsletterKey) is not Newsletter newsletter)
		{
			throw new InvalidOperationException($"Nyhetsbrevet {newsletterKey} finns inte eller är inte publicerat.");
		}

		var start = newsletter.AncestorOrSelf<Start>()
			?? throw new InvalidOperationException("Startsidan saknas, så nyhetsbrevet skickades inte.");

		// Varje mejl måste ha en avregistreringslänk, så utan sidan skickas inget (OUT-118).
		// Jobbet har inget språk, så det måste anges. Utan språk hittas inga sidor som varierar per språk
		if (start.FirstChild<UnsubscribePage>(DefaultCulture) == null)
		{
			throw new InvalidOperationException("Sidan Avsluta prenumeration saknas, så nyhetsbrevet skickades inte.");
		}

		int sent = 0, failed = 0, skipped = 0;

		foreach (var subscription in await GetPendingSubscriptionsAsync(newsletterKey))
		{
			var culture = newsletter.IsPublished(subscription.Culture) ? subscription.Culture : DefaultCulture;

			if (!newsletter.IsPublished(culture))
			{
				skipped++;
				continue;
			}

			var subject = newsletter.Value<string>("subject", culture) ?? newsletter.Name(culture);
			var body = newsletter.Value<IHtmlEncodedString>("body", culture)?.ToHtmlString() ?? string.Empty;

			try
			{
				var footer = await BuildFooterAsync(start, subscription, culture);
				var message = new EmailMessage(null, subscription.Email, subject, BuildHtml(subject, body, footer, culture), isBodyHtml: true);
				await emailSender.SendAsync(message, EmailType);
				await MarkDeliveredAsync(newsletterKey, subscription.Id);
				sent++;
			}
			catch (Exception ex)
			{
				// Ett mejl som inte går fram får inte stoppa resten
				failed++;
				logger.LogError(ex, "Kunde inte skicka nyhetsbrevet {Key} till prenumerant {Id}.", newsletterKey, subscription.Id);
			}
		}

		logger.LogInformation("Nyhetsbrevet {Key}: {Sent} skickade, {Failed} misslyckade, {Skipped} överhoppade.",
			newsletterKey, sent, failed, skipped);

		return new NewsletterSendResult(sent, failed, skipped);
	}

	// Bara de som inte redan har fått nyhetsbrevet, så att ett omkört jobb inte skickar två gånger
	private async Task<List<NewsletterSubscription>> GetPendingSubscriptionsAsync(Guid newsletterKey)
	{
		using var scope = scopeProvider.CreateScope();

		var subscriptions = await scope.ExecuteWithContextAsync(db =>
			db.Subscriptions.AsNoTracking()
				.Where(s => !db.Deliveries.Any(d => d.NewsletterKey == newsletterKey && d.SubscriptionId == s.Id))
				.OrderBy(x => x.Id)
				.ToListAsync());

		scope.Complete();

		return subscriptions;
	}

	private async Task MarkDeliveredAsync(Guid newsletterKey, int subscriptionId)
	{
		using var scope = scopeProvider.CreateScope();

		await scope.ExecuteWithContextAsync<object?>(async db =>
		{
			db.Deliveries.Add(new NewsletterDelivery
			{
				NewsletterKey = newsletterKey,
				SubscriptionId = subscriptionId,
				SentUtc = timeProvider.GetUtcNow().UtcDateTime,
			});

			await db.SaveChangesAsync();

			return null;
		});

		scope.Complete();
	}

	// Texten och länken längst ner i mejlet, på prenumerantens språk (OUT-118)
	private async Task<string> BuildFooterAsync(Start start, NewsletterSubscription subscription, string culture)
	{
		// Sidan på prenumerantens språk, och den engelska om den inte är publicerad på det språket
		var unsubscribePage = start.FirstChild<UnsubscribePage>(culture) ?? start.FirstChild<UnsubscribePage>(DefaultCulture)!;

		var intro = await TextAsync("Newsletter.UnsubscribeIntro", culture, "You are receiving this email because you signed up for the Friluftsguiden newsletter.");
		var linkText = await TextAsync("Newsletter.UnsubscribeLink", culture, "Unsubscribe");
		var url = $"{unsubscribePage.Url(culture, UrlMode.Absolute)}?token={Uri.EscapeDataString(subscription.UnsubscribeToken)}";

		return $"""
			<hr style="margin:32px 0 16px;border:0;border-top:1px solid #d9d4c7;" />
			<p style="margin:0;font-size:14px;color:#4f5b53;">{WebUtility.HtmlEncode(intro)} <a href="{WebUtility.HtmlEncode(url)}" style="color:#1f4d3a;">{WebUtility.HtmlEncode(linkText)}</a></p>
			""";
	}

	// Mejlet byggs utanför en webbförfrågan, så orden hämtas direkt från dictionary och inte via Umbraco-helpern
	private async Task<string> TextAsync(string key, string culture, string defaultValue)
	{
		var item = await dictionaryItemService.GetAsync(key);
		var value = item?.Translations
			.FirstOrDefault(t => string.Equals(t.LanguageIsoCode, culture, StringComparison.OrdinalIgnoreCase))?.Value;

		return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
	}

	// Mejlprogram läser inte CSS-filer, så stilen står direkt på elementen, i sajtens färger
	private static string BuildHtml(string subject, string body, string footer, string culture) => $"""
		<!DOCTYPE html>
		<html lang="{culture[..2]}">
		<head><meta charset="utf-8" /><title>{WebUtility.HtmlEncode(subject)}</title></head>
		<body style="margin:0;padding:24px;background:#f4f2ec;font-family:Arial,sans-serif;color:#1c2a22;">
		  <div style="max-width:600px;margin:0 auto;padding:32px;background:#ffffff;border-radius:12px;">
		    <h1 style="margin-top:0;color:#1f4d3a;">{WebUtility.HtmlEncode(subject)}</h1>
		    {body}
		    {footer}
		  </div>
		</body>
		</html>
		""";
}