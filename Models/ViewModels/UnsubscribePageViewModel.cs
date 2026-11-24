using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models.ViewModels;

public class UnsubscribePageViewModel : PageViewModel<UnsubscribePage>
{
	/// <summary>Token från länken i mejlet. Tom om länken saknar den eller den är för lång.</summary>
	public string Token { get; init; } = string.Empty;

	/// <summary>True när prenumerationen just har avslutats och bekräftelsen ska visas.</summary>
	public bool Unsubscribed { get; init; }
}