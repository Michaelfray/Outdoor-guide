using System.Globalization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Extensions;
using OutdoorGuide.Business.Services.Interfaces;
using OutdoorGuide.Models;

namespace OutdoorGuide.Controllers;


public class NewsletterSurfaceController(
	IUmbracoContextAccessor umbracoContextAccessor,
	IUmbracoDatabaseFactory databaseFactory,
	ServiceContext services,
	AppCaches appCaches,
	IProfilingLogger profilingLogger,
	IPublishedUrlProvider publishedUrlProvider,
	IAntiforgery antiforgery,
	INewsletterService newsletterService,
	ILogger<NewsletterSurfaceController> logger)
	: SurfaceController(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
{
	[HttpPost]
	[IgnoreAntiforgeryToken] // Kontrolleras själv nedan, så att en utgången token ger ett meddelande i stället för en felsida
	public async Task<IActionResult> Submit(NewsletterFormModel model)
	{
		if (!string.IsNullOrEmpty(model.Website))
		{
			logger.LogInformation("Formuläret för nyhetsbrevet hade honungsfällan ifylld. Inget sparades.");
			return RedirectBack(NewsletterStatus.Sent);
		}

		if (!await antiforgery.IsRequestValidAsync(HttpContext))
		{
			logger.LogInformation("Formuläret för nyhetsbrevet klarade inte anti-forgery-kontrollen. Inget sparades.");
			return KeepInputAndRedirect(NewsletterStatus.Expired);
		}

		model.Email = model.Email?.Trim();

		ModelState.Clear();

		if (!TryValidateModel(model) || !NewsletterFormModel.IsPlainEmailAddress(model.Email))
		{
			return KeepInputAndRedirect(NewsletterStatus.Invalid);
		}

		await newsletterService.SubscribeAsync(model.Email!, GetCulture());

		return RedirectBack(NewsletterStatus.Sent);
	}

	private IActionResult KeepInputAndRedirect(string status)
	{
		string? Field(string name) => Request.HasFormContentType ? Request.Form[name].ToString() : null;

		NewsletterTempData.Keep(TempData, Field("email"));

		return RedirectBack(status);
	}

	private IActionResult RedirectBack(string status)
	{
		var url = CurrentPage?.Url(PublishedUrlProvider, GetCulture(), UrlMode.Relative);

		if (string.IsNullOrEmpty(url) || !Url.IsLocalUrl(url))
		{
			url = "/";
		}

		return LocalRedirect($"{url}?{NewsletterStatus.QueryKey}={status}#{NewsletterStatus.Anchor}");
	}

	private string GetCulture()
		=> HttpContext.Features.Get<UmbracoRouteValues>()?.PublishedRequest.Culture ?? CultureInfo.CurrentCulture.Name;
}