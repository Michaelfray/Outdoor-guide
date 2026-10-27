using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Models.ViewModels;

public class AccountPageViewModel : PageViewModel<Account>
{
    /// <summary>Namnet på den inloggade medlemmen, eller användarnamnet om namnet saknas.</summary>
    public string MemberName { get; init; } = string.Empty;
}