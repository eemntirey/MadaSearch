using System;
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
            Favorites = new FavoritesService(db);
            Downloads = new DownloadService(db);
            OfflinePages = new OfflinePageService(db, Path.Combine(dir, "offline"));
            SettingsService = new BrowserSettingsService(db);
            Profile = new BrowserProfileService(db, SettingsService);
            Sessions = new SessionService(db);
            CurrentSettings = SettingsService.Load();
            Navigation = new NavigationService();
            AdBlock = new AdBlockService();
            AdBlock.Enabled = CurrentSettings.AdBlockEnabled;
            DataSaver = new DataSaverService();
            DataSaver.Enabled = CurrentSettings.DataSaverEnabled;
            _initialized = true;
        }

        public static void UpdateSettings(BrowserSettings settings)
        {
            SettingsService.Save(settings);
            CurrentSettings = settings;
            AdBlock.Enabled = settings.AdBlockEnabled;
            DataSaver.Enabled = settings.DataSaverEnabled;
        }

        public static void ReloadSettings()
        {
            CurrentSettings = SettingsService.Load();
            AdBlock.Enabled = CurrentSettings.AdBlockEnabled;
            DataSaver.Enabled = CurrentSettings.DataSaverEnabled;
        }
    }
}
