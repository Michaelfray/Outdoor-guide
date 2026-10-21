using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>
    /// Dokumenttyper, sidor och inställningar för Outdoor Guide, som körs en gång per databas vid uppstart.
    /// Lägg alltid nya steg sist. Döp aldrig om eller flytta ett steg som redan finns (se CLAUDE.md).
    /// </summary>
    public class SiteMigrationPlan : MigrationPlan
    {
        public SiteMigrationPlan() : base("OutdoorGuide")
        {
            From(string.Empty)
                .To<AddBaseAndStart>("add-base-and-start")
                .To<AddStartIntro>("add-start-intro")
                .To<AddSettingsPage>("add-settings-page")
                .To<AddCommonDictionaryItems>("add-common-dictionary-items")
                .To<AddMenuDictionaryItems>("add-menu-dictionary-items")
                .To<AddFooterDictionaryItems>("add-footer-dictionary-items")
                .To<AddTrailDictionaryItems>("add-trail-dictionary-items")
                .To<AddMunicipalityDictionaryItems>("add-municipality-dictionary-items")
                .To<AddSearchDictionaryItems>("add-search-dictionary-items")
                .To<AddFilterDictionaryItems>("add-filter-dictionary-items")
                .To<AddRegionAndMunicipality>("add-region-and-municipality")
                .To<AddDifficultyDataType>("add-difficulty-data-type")
                .To<AddTrail>("add-trail")
                .To<AddCommonDictionaryItems>("add-common-dictionary-items-2")
                .To<AddTestContent>("add-test-content")
                .To<AddReportDictionaryItems>("add-report-dictionary-items")
                .To<AddActivityDataType>("add-activity-data-type")
                .To<AddTrailActivity>("add-trail-activity")
                .To<AddActivityDictionaryItems>("add-activity-dictionary-items")
                .To<AddMenuPages>("add-menu-pages")
                .To<AddMenuContent>("add-menu-content");
        }
    }
}
