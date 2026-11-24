using System.ComponentModel;
using Hangfire;
using Hangfire.Server;

namespace OutdoorGuide.Business.Jobs.Interfaces;

public interface INewsletterJob
{
	// Hangfire läser attributen härifrån, eftersom jobbet köas via interfacet
	[DisplayName("Skicka nyhetsbrev {0}")]
	[AutomaticRetry(Attempts = 3)]
	Task SendAsync(Guid newsletterKey, PerformContext context);
}