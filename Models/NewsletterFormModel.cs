using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace OutdoorGuide.Models;

public class NewsletterFormModel
{
	[Required]
	[EmailAddress]
	[StringLength(NewsletterSubscription.EmailMaxLength)]
	public string? Email { get; set; }

	public string? Website { get; set; }

	
	public static bool IsPlainEmailAddress(string? email)
		=> !string.IsNullOrEmpty(email)
			&& MailAddress.TryCreate(email, out var address)
			&& address.Address == email;
}