namespace OutdoorGuide.Models;


public static class TrailConditionValues
{
	public const string Good = "good";
	public const string Obstacle = "obstacle";
	public const string Closed = "closed";

	public static readonly IReadOnlyList<string> All = [Good, Obstacle, Closed];

	public static string DictionaryKey(string? value) => value switch
	{
		Good => "Report.Condition.Good",
		Obstacle => "Report.Condition.Obstacle",
		Closed => "Report.Condition.Closed",
		_ => "Report.Condition",
	};
}