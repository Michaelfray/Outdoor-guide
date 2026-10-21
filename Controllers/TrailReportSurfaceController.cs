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
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Extensions;
using OutdoorGuide.Business.Services.Interfaces;
using OutdoorGuide.Models;

namespace OutdoorGuide.Controllers;


public class TrailReportSurfaceController(
	IUmbracoContextAccessor umbracoContextAccessor,
	IUmbracoDatabaseFactory databaseFactory,
	ServiceContext services,
	AppCaches appCaches,
	IProfilingLogger profilingLogger,
	IPublishedUrlProvider publishedUrlProvider,
	IAntiforgery antiforgery,
	ITrailReportService reportService,
	ILogger<TrailReportSurfaceController> logger)
	: SurfaceController(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
{
	[HttpPost]
	[IgnoreAntiforgeryToken] 
	public async Task<IActionResult> Submit(TrailReportFormModel model)
	{
		if (CurrentPage is not Trail trail)
		{
			return NotFound();
		}

		if (!string.IsNullOrEmpty(model.Website))
		{
			logger.LogInformation("Rapportformuläret på led {TrailKey} hade honungsfällan ifylld. Inget sparades.", trail.Key);
			return RedirectToTrail(trail, TrailReportStatus.Sent);
		}

		if (!await antiforgery.IsRequestValidAsync(HttpContext))
		{
			logger.LogInformation("Rapportformuläret på led {TrailKey} klarade inte anti-forgery-kontrollen. Inget sparades.", trail.Key);
			return KeepInputAndRedirect(trail, TrailReportStatus.Expired);
		}

		model.Condition = model.Condition?.Trim();
		model.Comment = model.Comment?.Trim().ReplaceLineEndings("\n");
		model.Name = model.Name?.Trim();

		ModelState.Clear();

		if (!TryValidateModel(model) || !TrailReportFormModel.IsKnownCondition(model.Condition))
		{
			return KeepInputAndRedirect(trail, TrailReportStatus.Invalid);
		}

		await reportService.AddAsync(trail.Key, model.Condition!, model.Comment!, model.Name, GetCulture());

		return RedirectToTrail(trail, TrailReportStatus.Sent);
	}

	private IActionResult KeepInputAndRedirect(IPublishedContent trail, string status)
	{
		string? Field(string name) => Request.HasFormContentType ? Request.Form[name].ToString() : null;

		TrailReportTempData.Keep(TempData, Field("condition"), Field("comment"), Field("name"));

		return RedirectToTrail(trail, status);
	}

	
	private IActionResult RedirectToTrail(IPublishedContent trail, string status)
	{
		var url = trail.Url(PublishedUrlProvider, GetCulture(), UrlMode.Relative);

		if (!Url.IsLocalUrl(url))
		{
			url = "/";
		}

		return LocalRedirect($"{url}?{TrailReportStatus.QueryKey}={status}#{TrailReportStatus.Anchor}");
	}

	private string GetCulture()
		=> HttpContext.Features.Get<UmbracoRouteValues>()?.PublishedRequest.Culture ?? CultureInfo.CurrentCulture.Name;
}