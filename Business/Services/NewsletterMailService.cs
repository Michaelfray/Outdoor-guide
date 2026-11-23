using System.Net;
using Microsoft.EntityFrameworkCore;
using OutdoorGuide.Business.Data;
using OutdoorGuide.Business.Services.Interfaces;
using OutdoorGuide.Models;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Business.Services;

/// <summary>Skickar ett publicerat nyhetsbrev som HTML-mejl till varje prenumerant, på prenumerantens språk (OUT-116).</summary>
public class NewsletterMailService(
	IEFCoreScopeProvider<NewsletterDbContext> scopeProvider,
	IUmbracoContextFactory umbracoContextFactory,
	IEmailSender emailSender,
	ILogger<NewsletterMailService> logger) : INewsletterMailService
{
	// Används när nyhetsbrevet inte är publicerat på prenumerantens språk
	private const string DefaultCulture = "en-US";
	private const string EmailType = "Newsletter";

	public async Task<int> SendAsync(Guid newsletterKey)
	{
		if (!emailSender.CanSendRequiredEmail())
		{
			logger.LogWarning("SMTP är inte inställt, så nyhetsbrevet {Key} skickades inte.", newsletterKey);
			return 0;
		}

		// Jobbet körs utanför en webbförfrågan, så Umbraco-kontexten skapas här
		using var contextReference = umbracoContextFactory.EnsureUmbracoContext();

		if (contextReference.UmbracoContext.Content?.GetById(newsletterKey) is not Newsletter newsletter)
		{
			logger.LogWarning("Nyhetsbrevet {Key} finns inte eller är inte publicerat.", newsletterKey);
			return 0;
		}

		var sent = 0;

		foreach (var subscription in await GetSubscriptionsAsync())
		{
			var culture = newsletter.IsPublished(subscription.Culture) ? subscription.Culture : DefaultCulture;

			if (!newsletter.IsPublished(culture))
			{
				continue;
			}

			var subject = newsletter.Value<string>("subject", culture) ?? newsletter.Name(culture);
			var body = newsletter.Value<IHtmlEncodedString>("body", culture)?.ToHtmlString() ?? string.Empty;

			try
			{
				var message = new EmailMessage(null, subscription.Email, subject, BuildHtml(subject, body, culture), isBodyHtml: true);
				await emailSender.SendAsync(message, EmailType);
				sent++;
			}
			catch (Exception ex)
			{
				// Ett mejl som inte går fram får inte stoppa resten
				logger.LogError(ex, "Kunde inte skicka nyhetsbrevet {Key} till prenumerant {Id}.", newsletterKey, subscription.Id);
			}
		}

		logger.LogInformation("Nyhetsbrevet {Key} skickades till {Count} prenumeranter.", newsletterKey, sent);

		return sent;
	}

	private async Task<List<NewsletterSubscription>> GetSubscriptionsAsync()
	{
		using var scope = scopeProvider.CreateScope();

		var subscriptions = await scope.ExecuteWithContextAsync(db =>
			db.Subscriptions.AsNoTracking().OrderBy(x => x.Id).ToListAsync());

		scope.Complete();

		return subscriptions;
	}

	// Mejlprogram läser inte CSS-filer, så stilen står direkt på elementen, i sajtens färger
	private static string BuildHtml(string subject, string body, string culture) => $"""
		<!DOCTYPE html>
		<html lang="{culture[..2]}">
		<head><meta charset="utf-8" /><title>{WebUtility.HtmlEncode(subject)}</title></head>
		<body style="margin:0;padding:24px;background:#f4f2ec;font-family:Arial,sans-serif;color:#1c2a22;">
		  <div style="max-width:600px;margin:0 auto;padding:32px;background:#ffffff;border-radius:12px;">
		    <h1 style="margin-top:0;color:#1f4d3a;">{WebUtility.HtmlEncode(subject)}</h1>
		    {body}
		  </div>
		</body>
		</html>
		""";
}