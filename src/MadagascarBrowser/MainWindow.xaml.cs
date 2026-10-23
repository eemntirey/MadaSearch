using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using MadagascarBrowser.Shell;

namespace MadagascarBrowser
{
    public partial class MainWindow : Window
    {
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_OLD = 19;
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWCP_ROUND = 2;

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int valueSize);

        private readonly TabManager _tabManager;
        private readonly bool _isPrivate;
        private readonly DispatcherTimer _sessionSnapshotTimer = new() { Interval = TimeSpan.FromSeconds(15) };

        public MainWindow() : this(false) { }

        public MainWindow(bool isPrivate)
        {
            InitializeComponent();
            _isPrivate = isPrivate;
            PrivateProfileSession.CleanupOrphanedProfiles();
            Title = isPrivate ? "Madagascar Browser — Navigation privée" : "Madagascar Browser";
            _tabManager = new TabManager(isPrivate);
            BrowserServices.EnsureInitialized();
            if (!_isPrivate && BrowserServices.CurrentSettings.RestoreSession)
                _tabManager.RestoreSession(BrowserServices.Sessions.Load());
            if (_tabManager.Tabs.Count == 0)
                _tabManager.NewTab();
            DataContext = _tabManager;
            SourceInitialized += OnSourceInitialized;
            PreviewKeyDown += OnPreviewKeyDown;
            Closing += OnClosing;
            Closed += (_, _) => _tabManager.Dispose();
            if (!_isPrivate)
            {
                _sessionSnapshotTimer.Tick += (_, _) => SaveSessionSnapshot();
                _sessionSnapshotTimer.Start();
            }
        }

        private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _sessionSnapshotTimer.Stop();
            if (!_isPrivate)
                SaveSessionSnapshot();
        }

        /// <summary>
        /// Persists a recoverable tab snapshot periodically, not only during normal shutdown.
        /// </summary>
        private void SaveSessionSnapshot()
        {
            if (_isPrivate)
                return;
            var urls = new List<string>();
            var activeIndex = -1;
            var currentIndex = _tabManager.Tabs.IndexOf(_tabManager.Active);
            for (var index = 0; index < _tabManager.Tabs.Count; index++)
            {
                var url = _tabManager.Tabs[index].Url;
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                    continue;

                if (index == currentIndex)
                    activeIndex = urls.Count;
                urls.Add(uri.AbsoluteUri);
            }

            try
            {
                BrowserServices.Sessions.Save(new MadagascarBrowser.Core.Data.BrowserSession(urls, activeIndex));
            }
            catch (Exception exception)
            {
                // Keep the UI responsive; log the exception type without URLs or user data.
                System.Diagnostics.Debug.WriteLine($"SessionSnapshotFailed: {exception.GetType().Name}");
            }
        }

        private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Control))
                return;

            switch (e.Key)
            {
                case System.Windows.Input.Key.T:
                    _tabManager.NewTab();
                    e.Handled = true;
                    break;
                case System.Windows.Input.Key.W:
                    if (_tabManager.Active != null) _tabManager.Close(_tabManager.Active);
                    e.Handled = true;
                    break;
                case System.Windows.Input.Key.L:
                    (_tabManager.Active?.View as BrowserView)?.FocusAddressBar();
                    e.Handled = true;
                    break;
            }
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            try
            {
                int on = 1;
                if (DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref on, sizeof(int)) != 0)
                {
                    DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_OLD, ref on, sizeof(int));
                }
                int round = DWMWCP_ROUND;
                DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref round, sizeof(int));
            }
            catch
            {
                // Windows sans DWM : la barre de titre reste en thème système.
            }
        }

        private void NewTab_Click(object sender, RoutedEventArgs e)
        {
            _tabManager.NewTab();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleMaximize();
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                try
                {
                    DragMove();
                }
                catch (InvalidOperationException)
                {
                    // Le bouton a été relâché avant DragMove : ignorer.
                }
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            ToggleMaximize();
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ToggleMaximize()
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is TabViewModel tab)
            {
                _tabManager.Close(tab);
            }
        }
    }
}
