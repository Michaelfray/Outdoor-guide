# Outdoor Guide

Grupparbete i kursen CMS-utveckling (Nackademin, CMS25). Projektet är en Umbraco-sajt. Den här filen är skriven för Claude, men gruppen kan läsa den också: här står hur vi delar dokumenttyper mellan oss.

## Projektet

- Umbraco 18.2 på .NET 10. Paketversionerna står i `Directory.Packages.props`.
- Databasen är SQLite (`umbraco/Data/Umbraco.sqlite.db`). Den är gitignorerad, så alla har en egen. Connection string står i `appsettings.Local.json`, som också är gitignorerad och skapas av installationsguiden.
- ModelsBuilder kör `SourceCodeAuto`. Modellerna i `umbraco/models` genereras från databasen och checkas in.
- Två språk: en-US (standard) och sv-SE. Startsidan Home/Hem har domänerna `/en` och `/`. Dokumenttyperna varierar per språk.
- Läraren har ett referensrepo, `schele/umbraco26`, som vi följer i struktur och stil.

## Arbetsregler

- **Ändra aldrig filer i `umbraco/models` för hand.** De skrivs om från databasen.
- **Ny dokumenttyp eller nytt fält kräver omstart.** Använd `dotnet run`, inte `dotnet watch`, när dokumenttyper ändras.
- **Stoppa sajten före git-kommandon som ändrar filer.**
- **SQLite tillåter en skrivning åt gången.** Den som kör sajten och skriver till databasen samtidigt kan få timeout.
- När backoffice skapar en mall kan den skriva över `.cshtml`-filen. Kolla `git diff Views` efteråt.

## Migrations: så delar vi dokumenttyper

Databasen delas inte. Det som alla ska ha (dokumenttyper, datatyper, mallar, språk, viktiga sidor) skrivs därför som steg i en Umbraco migration plan, som körs automatiskt en gång per databas vid uppstart.

| Fil | Vad den gör |
|---|---|
| `Business/Migrations/SiteMigrationPlan.cs` | Listan med steg i ordning |
| `Business/Migrations/AddBaseAndStart.cs` | Steg 1. **Mall för nya steg.** |
| `Business/Notifications/SiteMigrationHandler.cs` | Kör stegen som inte har körts, vid uppstart |
| `Composers/SiteMigrationComposer.cs` | Registrerar handlern |

Hur långt planen har kommit sparas i tabellen `umbracoKeyValue`, nyckeln `Umbraco.Core.Upgrader.State+OutdoorGuide`.

### Regler för steg

1. **Nya steg läggs alltid sist** i `SiteMigrationPlan`: `.To<AddXxx>("add-xxx")`.
2. **Ett steg som finns på `main` ändras aldrig**, och det döps aldrig om eller flyttas. Det har redan körts hos de andra och körs aldrig igen. Ändringar görs i ett nytt steg.
3. **Varje del i ett steg kollar först om saken redan finns** och skapar den bara om den saknas (`Ensure...`-metoder som i `AddBaseAndStart`). Ingenting som någon har skapat eller ändrat i backoffice får skrivas över.
4. Stegen ärver `UnscopedAsyncMigrationBase`, gör allt i ett `scopeProvider.CreateCoreScope()` och anropar `Context.Complete()` sist, som i `AddBaseAndStart`.
5. När en mall skapas skickas innehållet i den befintliga vyfilen med (`EnsureTemplateAsync`), annars skrivs `.cshtml` över.
6. Steget och de ändrade filerna i `umbraco/models` pushas i samma commit.

### Arbetssätt för Claude

- **Om `umbraco/models` har ändrats men inget nytt steg finns, säg det** innan något committas. Det betyder att någon har ändrat dokumenttyper i backoffice som de andra inte får.
- **Ska ett steg återskapa något som har skapats i backoffice?** Läs databasen. Kopiera först `Umbraco.sqlite.db` (och `-shm`/`-wal` om de finns) till en tillfällig mapp och läs kopian, eftersom sajten kan vara igång. Tabeller att titta i: `cmsContentType`, `cmsPropertyType`, `cmsPropertyTypeGroup`, `cmsContentType2ContentType` (compositions), `cmsContentTypeAllowedContentType`, `umbracoDataType`, `cmsTemplate`, `cmsDocumentType`. Skriv aldrig till databasen direkt.
- Använd Umbracos tjänster (`IContentTypeService`, `IDataTypeService`, `ITemplateService`, `IContentService`, `ILanguageService`), aldrig SQL, i Umbraco-migrations.
- Förklara på svenska. Låt användaren köra `git` och `dotnet` själv, om inget annat sägs.

### Mergekonflikt i SiteMigrationPlan

Om två personer har lagt till steg samtidigt blir det en konflikt på de sista raderna. Behåll båda stegen, efter varandra: först det som redan finns på `main`, sist det nya.

Den som redan har kört sitt eget steg lokalt har sin databas på det sista steget. Då tror planen att allt är klart, och steget från `main` körs aldrig hos den personen. Lösningen är en tom databas (se nedan), eller ett tillfälligt `.To<AddXxx>("add-xxx-2")` sist. Det fungerar eftersom stegen kollar vad som redan finns.

## Ny i gruppen, eller testa med tom databas

1. Klona och kör `dotnet run`. Har du redan en databas: stoppa sajten och flytta `umbraco/Data/Umbraco.sqlite.db*` och `appsettings.Local.json` till en mapp utanför repot.
2. Öppna `https://localhost:44348`, gå igenom installationsguiden och välj SQLite.
3. **Stoppa och starta sajten en gång till.** Under installationen hoppar migrationen över sig själv. Vid omstarten ska loggen visa `Starting 'OutdoorGuide'... Done`.
4. Nu finns Base, Start, språken och Home/Hem.

## Det som inte delas

- **Texter och sidor som skrivs i backoffice** stannar i den egna databasen. Lägg det som alla behöver i en migration.
- **Media** (`wwwroot/media`).

## EF Core (senare)

EF Core-migrations är en annan sak än Umbraco-migrations. De skapar egna tabeller för data som besökare skickar in, till exempel formulärsvar, och kursen kräver EF Core för "your own data". De har en egen `DbContext` och genereras med `dotnet ef migrations add`. Se lärarens `ContactFormDbContext` i `schele/umbraco26`. Vi lägger till dem när vi behöver dem.
