using System.Globalization;
using OutdoorGuide.Models;

namespace OutdoorGuide.Business.Services.Interfaces;

public interface ISearchService
{
	/// <summary>Söker bland leder och kommuner som är publicerade på <paramref name="cultureInfo"/>.</summary>
	IReadOnlyList<SearchHit> Search(string query, CultureInfo cultureInfo);
}
