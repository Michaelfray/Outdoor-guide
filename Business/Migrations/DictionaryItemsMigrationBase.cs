using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace OutdoorGuide.Business.Migrations
{
    /// <summary>
    /// Skapar dictionary items under en förälder, på svenska och engelska. Nycklar som redan finns
    /// hoppas över, så texter som ändrats i backoffice skrivs aldrig över.
    /// </summary>
    public abstract class DictionaryItemsMigrationBase(
        IMigrationContext context,
        ICoreScopeProvider scopeProvider,
        IDictionaryItemService dictionaryItemService,
        ILanguageService languageService)
        : UnscopedAsyncMigrationBase(context)
    {
        protected abstract string ParentKey { get; }

        protected abstract IEnumerable<(string Key, string Swedish, string English)> Items { get; }

        protected override async Task MigrateAsync()
        {
            using (var scope = scopeProvider.CreateCoreScope())
            {
                var languages = (await languageService.GetAllAsync()).ToList();
                var swedish = languages.FirstOrDefault(x => x.IsoCode.StartsWith("sv", StringComparison.OrdinalIgnoreCase));
                var english = languages.FirstOrDefault(x => x.IsoCode.StartsWith("en", StringComparison.OrdinalIgnoreCase));

                var parent = await dictionaryItemService.GetAsync(ParentKey)
                    ?? await CreateAsync(new DictionaryItem(ParentKey));

                foreach (var (key, swedishValue, englishValue) in Items)
                {
                    if (await dictionaryItemService.ExistsAsync(key))
                    {
                        continue;
                    }

                    var translations = new List<IDictionaryTranslation>();

                    if (swedish != null)
                    {
                        translations.Add(new DictionaryTranslation(swedish, swedishValue));
                    }

                    if (english != null)
                    {
                        translations.Add(new DictionaryTranslation(english, englishValue));
                    }

                    await CreateAsync(new DictionaryItem(parent.Key, key) { Translations = translations });
                }

                scope.Complete();
            }

            // Unscoped migrations måste själva säga att de är klara, då sparas planens nya läge
            Context.Complete();
        }

        private async Task<IDictionaryItem> CreateAsync(IDictionaryItem item)
        {
            var attempt = await dictionaryItemService.CreateAsync(item, Constants.Security.SuperUserKey);

            if (!attempt.Success)
            {
                throw new InvalidOperationException($"Could not create dictionary item '{item.ItemKey}': {attempt.Status}");
            }

            return attempt.Result;
        }
    }
}