using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace OutdoorGuide.Models;

public record TrailReportInput(string? Condition, string? Comment, string? Name);


public static class TrailReportTempData
{
	private const string ConditionKey = "TrailReport.Condition";
	private const string CommentKey = "TrailReport.Comment";
	private const string NameKey = "TrailReport.Name";

	public static void Keep(ITempDataDictionary tempData, string? condition, string? comment, string? name)
	{
		tempData[ConditionKey] = Limit(condition, TrailReport.ConditionMaxLength);
		tempData[CommentKey] = Limit(comment?.ReplaceLineEndings("\n"), TrailReport.CommentMaxLength);
		tempData[NameKey] = Limit(name, TrailReport.NameMaxLength);
	}

	public static TrailReportInput? Take(ITempDataDictionary tempData)
	{
		if (!tempData.ContainsKey(ConditionKey) && !tempData.ContainsKey(CommentKey) && !tempData.ContainsKey(NameKey))
		{
			return null;
		}

		return new TrailReportInput(
			tempData[ConditionKey] as string,
			tempData[CommentKey] as string,
			tempData[NameKey] as string);
	}

	private static string? Limit(string? value, int maxLength)
		=> value is { Length: > 0 } && value.Length > maxLength ? value[..maxLength] : value;
}