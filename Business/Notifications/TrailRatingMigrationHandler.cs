using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using OutdoorGuide.Business.Data;

namespace OutdoorGuide.Business.Notifications;

/// <summary>Kör de EF Core-migrationer för <see cref="TrailRatingDbContext"/> som inte har körts, när Umbraco har startat.</summary>
public class TrailRatingMigrationHandler(
	IRuntimeState runtimeState,
	IDbContextFactory<TrailRatingDbContext> contextFactory,
	ILogger<TrailRatingMigrationHandler> logger)
	: INotificationAsyncHandler<UmbracoApplicationStartedNotification>
{
	public async Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken cancellationToken)
	{
		// Inget att migrera förrän Umbraco är installerat
		if (runtimeState.Level < RuntimeLevel.Run)
		{
			return;
		}

		// Ett fel här får inte stoppa sajten
		try
		{
			await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

			var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

			if (pending.Count > 0)
			{
				logger.LogInformation("Kör migrationer för stjärnbetyg: {Migrations}", pending);
				await context.Database.MigrateAsync(cancellationToken);
				logger.LogInformation("Migrationerna för stjärnbetyg är klara.");
			}
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			logger.LogError(ex, "Kunde inte köra migrationerna för stjärnbetyg. Betyg kan inte sparas förrän det är löst.");
		}
	}
}
