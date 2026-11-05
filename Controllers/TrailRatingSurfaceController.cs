using System.Globalization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Extensions;
using OutdoorGuide.Business.Services.Interfaces;
using OutdoorGuide.Models;

namespace OutdoorGuide.Controllers;

/// <summary>Tar emot en medlems betyg från ledsidan och skickar tillbaka besökaren med en status.</summary>
public class TrailRatingSurfaceController(
	IUmbracoContextAccessor umbracoContextAccessor,
	IUmbracoDatabaseFactory databaseFactory,
	ServiceContext services,
	AppCaches appCaches,
	IProfilingLogger profilingLogger,
	IPublishedUrlProvider publishedUrlProvider,
	IAntiforgery antiforgery,
	ITrailRatingService ratingService,
	IMemberManager memberManager,
	ILogger<TrailRatingSurfaceController> logger)
	: SurfaceController(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
{
	[HttpPost]
	[IgnoreAntiforgeryToken]
	public async Task<IActionResult> Rate(int? score)
	{
		if (CurrentPage is not Trail trail)
		{
			return NotFound();
		}

		if (!await antiforgery.IsRequestValidAsync(HttpContext))
		{
			logger.LogInformation("Betyget på led {TrailKey} klarade inte anti-forgery-kontrollen. Inget sparades.", trail.Key);
			return RedirectToTrail(trail, TrailRatingStatus.Expired);
		}

		// Både medlemmen och betyget läses från databasen, och ett databasfel får inte krascha sidan
		try
		{
			var member = await memberManager.GetCurrentMemberAsync();

			if (member is null)
			{
				return RedirectToTrail(trail, TrailRatingStatus.LoginRequired);
			}

			if (score is null or < TrailRating.MinScore or > TrailRating.MaxScore)
			{
				return RedirectToTrail(trail, TrailRatingStatus.Invalid);
			}

			await ratingService.SaveAsync(trail.Key, member.Key, score.Value);
		}
		catch (Exception ex)
		{
			// Till exempel en dubbelklickning, där det unika indexet stoppar den andra rösten, eller en SQLite-timeout
			logger.LogWarning(ex, "Kunde inte spara betyget på led {TrailKey}", trail.Key);
			return RedirectToTrail(trail, TrailRatingStatus.Retry);
		}

		return RedirectToTrail(trail, TrailRatingStatus.Saved);
	}

	private IActionResult RedirectToTrail(IPublishedContent trail, string status)
	{
		var url = trail.Url(PublishedUrlProvider, GetCulture(), UrlMode.Relative);

		if (!Url.IsLocalUrl(url))
		{
			url = "/";
		}

		return LocalRedirect($"{url}?{TrailRatingStatus.QueryKey}={status}#{TrailRatingStatus.Anchor}");
	}

	private string GetCulture()
		=> HttpContext.Features.Get<UmbracoRouteValues>()?.PublishedRequest.Culture ?? CultureInfo.CurrentCulture.Name;
}
