using OutdoorGuide.Controllers;
using Umbraco.Cms.Core.Composing;

namespace OutdoorGuide.Composers;

public class PageControllerComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder) => builder.Services.AddScoped<PageControllerDependencies>();

}
