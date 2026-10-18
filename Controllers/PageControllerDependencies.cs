using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace OutdoorGuide.Controllers;

// ILogger hjälper till med: att skriva loggmeddelanden (fel, varningar, info) till loggen.
// ICompositeViewEngine hjälper till med: att hitta och kontrollera att en vy (.cshtml) finns innan den renderas.
// IUmbracoContextAccessor hjälper till med: att komma åt Umbraco-kontexten för requesten, t.ex. den publicerade sidan och cachen.
// IVariationContextAccessor hjälper till med: att veta vilket språk (kultur) requesten gäller, t.ex. en-US eller sv-SE.
// IDocumentUrlService hjälper till med: att ta fram URL:er för dokument, t.ex. för att länka mellan sidor.
public sealed class PageControllerDependencies(ILogger<RenderController> logger, ICompositeViewEngine viewEngine, IUmbracoContextAccessor umbracoContextAccessor, IVariationContextAccessor variationContextAccessor, IDocumentUrlService documentUrlService)
{
    public ILogger<RenderController> Logger { get; } = logger;

    public ICompositeViewEngine ViewEngine { get; } = viewEngine;

    public IUmbracoContextAccessor UmbracoContextAccessor { get; } = umbracoContextAccessor;

    public IVariationContextAccessor VariationContextAccessor { get; } = variationContextAccessor;

    public IDocumentUrlService DocumentUrlService { get; } = documentUrlService;
}
