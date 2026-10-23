using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using MadagascarBrowser.Shell;
using MadagascarBrowser.Core.Security;

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
        private bool _isPinMode;
        private Point _lastMousePosition;
        private bool _isDragging;

        public MainWindow() : this(false) { }

        public MainWindow(bool isPrivate)
        {
            InitializeComponent();
            _isPrivate = isPrivate;
            PrivateProfileSession.CleanupOrphanedProfiles();
            Title = isPrivate ? "Madagascar Browser — Navigation privée" : "Madagascar Browser";
            _tabManager = new TabManager(isPrivate);
            BrowserServices.EnsureInitialized();
            if (PrivateBrowsingPolicy.ShouldPersistSession(_isPrivate) && BrowserServices.CurrentSettings.RestoreSession)
                _tabManager.RestoreSession(BrowserServices.Sessions.Load());
            if (_tabManager.Tabs.Count == 0)
                _tabManager.NewTab();
            DataContext = _tabManager;
            _tabManager.FullScreenChanged += OnFullScreenChanged;
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
            if (PrivateBrowsingPolicy.ShouldPersistSession(_isPrivate))
                SaveSessionSnapshot();
        }

        /// <summary>
        /// Persists a recoverable tab snapshot periodically, not only during normal shutdown.
        /// </summary>
        private void SaveSessionSnapshot()
        {
            if (!PrivateBrowsingPolicy.ShouldPersistSession(_isPrivate))
                return;
            var urls = new List<string>();
            var activeIndex = -1;
            var currentIndex = _tabManager.Active is { } activeTab ? _tabManager.Tabs.IndexOf(activeTab) : -1;
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
            // Échap est traité par la vue (barre de recherche, plein écran) : la
            // fenêtre ne doit pas l'intercepter avant elle.
            if (e.Key == System.Windows.Input.Key.Escape)
                return;

            var ctrl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            var alt = (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt;
            var shift = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;

            var command = MadagascarBrowser.Core.Navigation.AcceleratorCommandResolver.Resolve(
                e.Key.ToString(), ctrl, alt, shift);

            if (command == MadagascarBrowser.Core.Navigation.AcceleratorCommand.None)
                return;

            _tabManager.Execute(command);
            e.Handled = true;
        }

        /// <summary>
        /// Une page passe en plein écran : la barre d'onglets et la barre d'état
        /// sont masquées pour laisser toute la place à la page.
        /// </summary>
        private void OnFullScreenChanged(object? sender, bool isFullScreen)
        {
            TabStripBorder.Visibility = isFullScreen ? Visibility.Collapsed : Visibility.Visible;
            StatusBarBorder.Visibility = isFullScreen ? Visibility.Collapsed : Visibility.Visible;
        }

        private void OnSourceInitialized(object? sender, EventArgs e)
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
                AppLog.Warning("Le gestionnaire de fenêtres DWM a refusé la demande de thème.");
            }
        }

        private void NewTab_Click(object sender, RoutedEventArgs e)
        {
            _tabManager.NewTab();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Un clic qui démarre sur un bouton (min/max/fermer) ne doit pas lancer le
            // déplacement de fenêtre : DragMove() capture la souris et le bouton ne reçoit
            // jamais le MouseUp, donc le Click n'est jamais déclenché.
            if (IsInsideButton(e.OriginalSource as DependencyObject))
                return;

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
                catch (InvalidOperationException exception)
                {
                    // Le bouton a été relâché avant DragMove : ignorer.
                    AppLog.Warning("Déplacement de la fenêtre impossible.", exception);
                }
            }
        }

        private static bool IsInsideButton(DependencyObject? source)
        {
            while (source != null)
            {
                if (source is ButtonBase)
                    return true;

                source = source switch
                {
                    Visual visual => VisualTreeHelper.GetParent(visual),
                    Visual3D visual3D => VisualTreeHelper.GetParent(visual3D),
                    _ => null
                };
            }

            return false;
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

        private void PinBtn_Click(object sender, RoutedEventArgs e)
        {
            _isPinMode = !_isPinMode;
            UpdatePinButtonState();
            UpdateTabStripVisibility();
        }

        private void UpdatePinButtonState()
        {
            if (_isPinMode)
            {
                // Mode épinglé - le bouton montre qu'on est en mode détachable
                PinBtn.Content = ""; // glyphe alternatif ou on garde ⛓
                // Tooltip géré par le style
            }
            else
            {
                PinBtn.Content = "⛓";
            }
        }

        private void UpdateTabStripVisibility()
        {
            // En mode épinglé, les onglets restent visibles dans leur position normale
            // En mode détachable, on peut les faire glisser
            if (_isPinMode)
            {
                // Mode épinglé - onglets fixes, Z-index normal
                TabStripBorder.SetValue(Canvas.ZIndexProperty, 10);
                TabStripBorder.Cursor = Cursors.Arrow;
            }
            else
            {
                // Mode détachable - onglets flottants avec Z-index élevé
                TabStripBorder.SetValue(Canvas.ZIndexProperty, 100);
                TabStripBorder.Cursor = Cursors.SizeAll;
            }
        }

        private void TabStripBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isPinMode)
                return; // En mode épinglé, ignorer les clics sur les onglets

            _isDragging = true;
            _lastMousePosition = e.GetPosition(this);
            TabCaptureMouse(e);
        }

        private void TabStripBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            TabStripBorder.ReleaseMouseCapture();
        }

        private void TabStripBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || _isPinMode)
                return;

            var currentPosition = e.GetPosition(this);
            var delta = currentPosition - _lastMousePosition;

            // Déplacement vertical seulement pour l'instant (en-dessus de la barre de titre)
            var newTop = Canvas.GetTop(TabStripBorder) + delta.Y;
            var newLeft = Canvas.GetLeft(TabStripBorder) + delta.X;

            // Garder dans les bounds de la fenêtre
            var maxTop = ActualHeight - TabStripBorder.ActualHeight - 40; // laisser de la place pour la barre de titre
            var maxLeft = ActualWidth - TabStripBorder.ActualWidth - 16;

            newTop = Math.Max(0, Math.Min(maxTop, newTop));
            newLeft = Math.Max(0, Math.Min(maxLeft, newLeft));

            Canvas.SetTop(TabStripBorder, newTop);
            Canvas.SetLeft(TabStripBorder, newLeft);

            _lastMousePosition = currentPosition;
        }

        private void TabCaptureMouse(MouseEventArgs e)
        {
            // Capturer la souris sur l'élément pour recevoir tous les événements de déplacement
            e.Handled = true;
            TabStripBorder.CaptureMouse();
        }
    }
}
