using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Notifications;
using OutdoorGuide.Business.Notifications;

namespace OutdoorGuide.Composers
{
    public class SiteMigrationComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
            => builder.AddNotificationAsyncHandler<UmbracoApplicationStartingNotification, SiteMigrationHandler>();
    }
}
