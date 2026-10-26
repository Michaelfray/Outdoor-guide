using Microsoft.AspNetCore.Mvc;
using OutdoorGuide.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace OutdoorGuide.Controllers;

public abstract class BasePageController<TContent, TViewModel>(PageControllerDependencies dependencies)
    : RenderController(dependencies.Logger, dependencies.ViewEngine, dependencies.UmbracoContextAccessor)
    where TContent : class, IPublishedContent
    where TViewModel : PageViewModel<TContent>
{
    protected PageControllerDependencies Dependencies { get; } = dependencies;
    protected abstract TViewModel Build(TContent page);


    // GET: BasePageController
    public override IActionResult Index()
    {
        if (CurrentPage is not TContent page)
            {
                return NotFound();
            }

        TViewModel model = Build(page);
        model.UrlSegment = GetUrlSegment(page);

        return CurrentTemplate(model);
    }


    private string GetUrlSegment(IPublishedContent content)
    {
        string culture = Dependencies.VariationContextAccessor.VariationContext?.Culture ?? string.Empty;
        bool isDraft = Dependencies.UmbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext)
            && umbracoContext.InPreviewMode;

        return Dependencies.DocumentUrlService.GetUrlSegment(content.Key, culture, isDraft) ?? string.Empty;
    }

}