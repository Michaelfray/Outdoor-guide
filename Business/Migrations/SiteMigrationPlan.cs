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
                .To<AddSettingsPage>("add-settings-page");
        }
    }
}
