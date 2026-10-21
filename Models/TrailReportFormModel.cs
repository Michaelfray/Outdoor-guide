using System.ComponentModel.DataAnnotations;

namespace OutdoorGuide.Models;

public class TrailReportFormModel
{
	[Required]
	[StringLength(TrailReport.ConditionMaxLength)]
	public string? Condition { get; set; }

	[Required]
	[StringLength(TrailReport.CommentMaxLength)]
	public string? Comment { get; set; }

	[StringLength(TrailReport.NameMaxLength)]
	public string? Name { get; set; }

	/// <summary>Honungsfälla: dold för människor, så det som står här har en bot fyllt i.</summary>
	public string? Website { get; set; }

	
	public static bool IsKnownCondition(string? condition)
		=> condition is not null && TrailConditionValues.All.Contains(condition);
}