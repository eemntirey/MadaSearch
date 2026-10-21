using System;
using System.Collections.Generic;
using System.IO;
using MadagascarBrowser.Core.Blocking;
using MadagascarBrowser.Core.Data;
using MadagascarBrowser.Core.Navigation;

namespace MadagascarBrowser.Shell
{
    public static class BrowserServices
    {
        private static bool _initialized;

        public static NavigationService Navigation { get; private set; } = null!;
        public static AdBlockService AdBlock { get; private set; } = null!;
        public static DataSaverService DataSaver { get; private set; } = null!;
        public static DataSaverMetricsService DataSaverMetrics { get; private set; } = null!;
        public static HistoryService History { get; private set; } = null!;
        public static FavoritesService Favorites { get; private set; } = null!;
        public static DownloadService Downloads { get; private set; } = null!;
        public static OfflinePageService OfflinePages { get; private set; } = null!;
        public static BrowserProfileService Profile { get; private set; } = null!;
        public static BrowserSettingsService SettingsService { get; private set; } = null!;
        public static SessionService Sessions { get; private set; } = null!;
        public static BrowserSettings CurrentSettings { get; private set; } = new();

        public static void EnsureInitialized()
        {
            if (_initialized) return;
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MadagascarBrowser");
            var db = new Database(Path.Combine(dir, "browser.db"));
            History = new HistoryService(db);
            DataSaverMetrics = new DataSaverMetricsService(db);
            Favorites = new FavoritesService(db);
            Downloads = new DownloadService(db);
            OfflinePages = new OfflinePageService(db, Path.Combine(dir, "offline"));
            SettingsService = new BrowserSettingsService(db);
            Profile = new BrowserProfileService(db, SettingsService);
            Sessions = new SessionService(db);
            CurrentSettings = SettingsService.Load();
            Navigation = new NavigationService();
            AdBlock = new AdBlockService();
            ApplyAllowedDomains(CurrentSettings);
            DataSaver = new DataSaverService();
            DataSaver.Enabled = CurrentSettings.DataSaverEnabled;
            ThemeManager.Apply(CurrentSettings);
            _initialized = true;
        }

        /// <summary>
        /// Recrée le bloqueur de publicités et applique les domaines autorisés
        /// par l'utilisateur via SetDomainExempt (fonction existante du Core).
        /// </summary>
        private static void ApplyAllowedDomains(BrowserSettings settings)
        {
            var adBlock = new AdBlockService { Enabled = settings.AdBlockEnabled };
            foreach (var domain in settings.AllowedDomains ?? new List<string>())
            {
                adBlock.SetDomainExempt(domain);
            }
            AdBlock = adBlock;
        }

        public static void UpdateSettings(BrowserSettings settings)
        {
            SettingsService.Save(settings);
            CurrentSettings = settings;
            ApplyAllowedDomains(settings);
            DataSaver.Enabled = settings.DataSaverEnabled;
            ThemeManager.Apply(settings);
        }

        public static void ReloadSettings()
        {
            CurrentSettings = SettingsService.Load();
            ApplyAllowedDomains(CurrentSettings);
            DataSaver.Enabled = CurrentSettings.DataSaverEnabled;
            ThemeManager.Apply(CurrentSettings);
        }
    }
}
