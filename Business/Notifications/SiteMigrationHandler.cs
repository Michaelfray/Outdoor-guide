using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using OutdoorGuide.Business.Migrations;

namespace OutdoorGuide.Business.Notifications
{
    /// <summary>Kör de steg i <see cref="SiteMigrationPlan"/> som inte har körts än, när Umbraco är installerat.</summary>
    public class SiteMigrationHandler(
        IRuntimeState runtimeState,
        IMigrationPlanExecutor migrationPlanExecutor,
        ICoreScopeProvider coreScopeProvider,
        IKeyValueService keyValueService,
        AppCaches appCaches)
        : INotificationAsyncHandler<UmbracoApplicationStartingNotification>
    {
        public async Task HandleAsync(UmbracoApplicationStartingNotification notification, CancellationToken cancellationToken)
        {
            // Under installationen är nivån Install, så på en helt ny databas körs planen först vid nästa start
            if (runtimeState.Level < RuntimeLevel.Run)
            {
                return;
            }

            var result = await new Upgrader(new SiteMigrationPlan()).ExecuteAsync(migrationPlanExecutor, coreScopeProvider, keyValueService);

            // Dictionary items som skapas i migrations hamnar inte i cachen förrän den töms
            if (result.CompletedTransitions.Count > 0)
            {
                appCaches.IsolatedCaches.ClearCache<IDictionaryItem>();
            }
        }
    }
}
