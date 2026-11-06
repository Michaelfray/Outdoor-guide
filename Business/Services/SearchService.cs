using System.Globalization;
using Examine;
using Examine.Search;
using OutdoorGuide.Business.Services.Interfaces;
using OutdoorGuide.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace OutdoorGuide.Business.Services;

/// <summary>Söker bland leder och kommuner på namn. Byggd som lärarens FindService i schele/umbraco26.</summary>
public class SearchService(
	IExamineManager examineManager,
	IUmbracoContextAccessor umbracoContextAccessor,
	ILogger<SearchService> logger) : ISearchService
{
	// Med cirka 25 leder behövs inga sidnummer
	private const int MaxHits = 50;

	public IReadOnlyList<SearchHit> Search(string query, CultureInfo cultureInfo)
	{
		if (string.IsNullOrWhiteSpace(query)
			|| !examineManager.TryGetIndex(Constants.UmbracoIndexes.ExternalIndexName, out var index)
			|| !umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext)
			|| umbracoContext.Content is not { } content)
		{
			return [];
		}

		// Leder och kommuner varierar per språk, så namnet och publiceringen finns i ett fält per språk (nodeName_sv-se)
		var culture = cultureInfo.Name.ToLowerInvariant();

		ISearchResults results;

		try
		{
			results = index.Searcher
				.CreateQuery(IndexTypes.Content)
				.ManagedQuery(query, [$"{UmbracoExamineFieldNames.NodeNameFieldName}_{culture}"])
				.And().Field($"{UmbracoExamineFieldNames.PublishedFieldName}_{culture}", "y")
				.And().Field(UmbracoExamineFieldNames.PublishedFieldName, "y")
				.And().GroupedOr([ExamineFieldNames.ItemTypeFieldName], [Trail.ModelTypeAlias, Municipality.ModelTypeAlias])
				.Execute(QueryOptions.SkipTake(0, MaxHits));
		}
		catch (Exception ex)
		{
			// Sökordet tolkas som en Lucene-fråga, så tecken som " ( ~ kan kasta. Det får inte krascha sidan
			logger.LogWarning(ex, "Kunde inte söka på {Query}", query);
			return [];
		}

		var hits = new List<SearchHit>();

		foreach (var item in results)
		{
			var page = content.GetById(int.Parse(item.Id));

			if (page == null)
			{
				continue;
			}

			hits.Add(new SearchHit(
				page.Name(cultureInfo.Name),
				page.Url(cultureInfo.Name),
				page is Trail ? SearchHitType.Trail : SearchHitType.Municipality));
		}

		return hits;
	}
}
