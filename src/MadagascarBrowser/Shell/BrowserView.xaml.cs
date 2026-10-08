using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Text.Json;
using Microsoft.Win32;
using MadagascarBrowser.Core.Blocking;
using MadagascarBrowser.Core.Data;
using Microsoft.Web.WebView2.Core;

namespace MadagascarBrowser.Shell
{
    public partial class BrowserView : UserControl
    {
        private const string StartPageUrl = "https://startpage.local/index.html";
        private const string ErrorPageUrl = "https://startpage.local/error.html";
        private static readonly byte[] PlaceholderGif =
            Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");

        private bool _coreReady;
        private bool _suppressSuggest;

        public BrowserView()
        {
            InitializeComponent();
            BrowserServices.EnsureInitialized();
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_coreReady) return;
            try
            {
                await WebView.EnsureCoreWebView2Async();
                _coreReady = true;
                InitCore();
            }
            catch (Exception exception)
            {
                WebView.Visibility = Visibility.Collapsed;
                WebViewError.Visibility = Visibility.Visible;
                WebViewErrorText.Text = $"Impossible d'initialiser WebView2 : {exception.Message}";
                return;
            }
        }

        private void InitCore()
        {
            var core = WebView.CoreWebView2;

            var folder = Path.Combine(AppContext.BaseDirectory, "StartPage");
            if (Directory.Exists(folder))
            {
                core.SetVirtualHostNameToFolderMapping("startpage.local", folder, CoreWebView2HostResourceAccessKind.Allow);
            }

            core.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
            core.WebResourceRequested += OnWebResourceRequested;
            core.DownloadStarting += OnDownloadStarting;
            core.NavigationStarting += OnNavigationStarting;
            core.NavigationCompleted += OnNavigationCompleted;
            core.SourceChanged += OnSourceChanged;
            core.HistoryChanged += OnHistoryChanged;
            HookAcceleratorKeys();

            var source = core.Source;
            if (string.IsNullOrWhiteSpace(source) || source == "about:blank")
            {
                var initialUrl = DataContext is TabViewModel tab &&
                    Uri.TryCreate(tab.Url, UriKind.Absolute, out var uri) &&
                    (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                    ? uri.AbsoluteUri
                    : StartPageUrl;
                core.Navigate(initialUrl);
            }

            UpdateNavButtons();
        }

        private void OnHistoryChanged(object? sender, object e)
        {
            UpdateNavButtons();
        }

        private void UpdateNavButtons()
        {
            if (!_coreReady) return;
            var core = WebView.CoreWebView2;
            BackBtn.IsEnabled = core.CanGoBack;
            ForwardBtn.IsEnabled = core.CanGoForward;
        }

        private void HookAcceleratorKeys()
        {
            // L'événement AcceleratorKeyPressed vit sur CoreWebView2Controller,
            // non exposé publiquement par le contrôle WPF : réflexion ciblée.
            try
            {
                var property = WebView.GetType().BaseType?.GetProperty(
                    "CoreWebView2Controller",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Public);
                if (property?.GetValue(WebView) is CoreWebView2Controller controller)
                {
                    controller.AcceleratorKeyPressed += OnAcceleratorKeyPressed;
                }
            }
            catch
            {
                // Sans hook clavier, les raccourcis restent gérés par la fenêtre
                // quand le focus n'est pas sur le WebView.
            }
        }

        private void OnAcceleratorKeyPressed(object? sender, CoreWebView2AcceleratorKeyPressedEventArgs e)
        {
            if (e.KeyEventKind != CoreWebView2KeyEventKind.KeyDown) return;
            if ((Interop.GetAsyncKeyState(0x11) & 0x8000) == 0) return;

            switch ((Key)e.VirtualKey)
            {
                case Key.T:
                    e.Handled = true;
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (Window.GetWindow(this)?.DataContext is TabManager tabManager)
                            tabManager.NewTab();
                    }));
                    break;
                case Key.W:
                    e.Handled = true;
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (Window.GetWindow(this)?.DataContext is TabManager tabManager &&
                            tabManager.Active != null)
                            tabManager.Close(tabManager.Active);
                    }));
                    break;
                case Key.L:
                    e.Handled = true;
                    Dispatcher.BeginInvoke(new Action(FocusAddressBar));
                    break;
            }
        }

        private static class Interop
        {
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            internal static extern short GetAsyncKeyState(int vKey);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (_coreReady && WebView.CoreWebView2.CanGoBack)
            {
                WebView.CoreWebView2.GoBack();
            }
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            if (_coreReady && WebView.CoreWebView2.CanGoForward)
            {
                WebView.CoreWebView2.GoForward();
            }
        }

        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            if (_coreReady)
            {
                WebView.CoreWebView2.Reload();
            }
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            SuggestPopup.IsOpen = false;
            MenuFeedback.Text = string.Empty;
            MenuPopup.IsOpen = true;
        }

        private void MenuNewTab_Click(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = false;
            if (Window.GetWindow(this)?.DataContext is TabManager tm)
            {
                tm.NewTab();
            }
        }

        private void MenuSettings_Click(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = false;
            if (Window.GetWindow(this)?.DataContext is TabManager tm)
                tm.OpenSettings();
        }

        private void MenuAddFavorite_Click(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = false;
            if (!Uri.TryCreate(WebView.CoreWebView2?.Source, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                MenuFeedback.Text = "Ouvrez une page Web avant de l'ajouter aux favoris.";
                MenuPopup.IsOpen = true;
                return;
            }

            BrowserServices.Favorites.Add(uri.AbsoluteUri, WebView.CoreWebView2.DocumentTitle);
        }

        private void MenuHistory_Click(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = false;
            if (Window.GetWindow(this)?.DataContext is TabManager tm)
                tm.OpenLibrary(0);
        }

        private void MenuFavorites_Click(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = false;
            if (Window.GetWindow(this)?.DataContext is TabManager tm)
                tm.OpenLibrary(1);
        }

        private async void MenuSaveOffline_Click(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = false;
            if (!_coreReady || !Uri.TryCreate(WebView.CoreWebView2.Source, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                MessageBox.Show(Window.GetWindow(this), "Ouvrez une page Web avant de l'enregistrer hors ligne.",
                    "Pages hors ligne", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var response = await WebView.CoreWebView2.CallDevToolsProtocolMethodAsync(
                    "Page.captureSnapshot", "{\"format\":\"mhtml\"}");
                using var document = JsonDocument.Parse(response);
                var mhtml = document.RootElement.GetProperty("data").GetString();
                if (string.IsNullOrEmpty(mhtml))
                    throw new InvalidOperationException("WebView2 n'a renvoyé aucun contenu.");

                BrowserServices.OfflinePages.Save(uri.AbsoluteUri, WebView.CoreWebView2.DocumentTitle, mhtml);
                MessageBox.Show(Window.GetWindow(this), "La page a été enregistrée hors ligne.",
                    "Pages hors ligne", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show(Window.GetWindow(this), $"Impossible d'enregistrer la page : {exception.Message}",
                    "Pages hors ligne", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuOfflinePages_Click(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = false;
            if (Window.GetWindow(this)?.DataContext is TabManager tm)
                tm.OpenLibrary(2);
        }

        private void MenuDownloads_Click(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = false;
            if (Window.GetWindow(this)?.DataContext is TabManager tm)
                tm.OpenDownloads();
        }

        private void MenuExportProfile_Click(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = false;
            var dialog = new SaveFileDialog
            {
                FileName = "madagascar-browser-profile.json",
                Filter = "Profil Madagascar Browser (*.json)|*.json"
            };
            if (dialog.ShowDialog(Window.GetWindow(this)) != true)
                return;

            try
            {
                BrowserServices.Profile.Export(dialog.FileName);
                MessageBox.Show(Window.GetWindow(this), "Le profil a été exporté.", "Profil",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show(Window.GetWindow(this), $"Impossible d'exporter le profil : {exception.Message}",
                    "Profil", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuImportProfile_Click(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = false;
            var dialog = new OpenFileDialog { Filter = "Profil Madagascar Browser (*.json)|*.json" };
            if (dialog.ShowDialog(Window.GetWindow(this)) != true)
                return;
            if (MessageBox.Show(Window.GetWindow(this),
                    "L'import remplacera les favoris, l'historique et les paramètres actuels. Continuer ?",
                    "Importer le profil", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            try
            {
                BrowserServices.Profile.Import(dialog.FileName);
                BrowserServices.ReloadSettings();
                MessageBox.Show(Window.GetWindow(this), "Le profil a été importé.", "Profil",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show(Window.GetWindow(this), $"Impossible d'importer le profil : {exception.Message}",
                    "Profil", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateTo(string url)
        {
            WebView.Source = Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.IsFile
                ? uri
                : new Uri(ResolveAddress(url));
        }

        private void OnDownloadStarting(object? sender, CoreWebView2DownloadStartingEventArgs e)
        {
            var operation = e.DownloadOperation;
            var fileName = Uri.TryCreate(operation.Uri, UriKind.Absolute, out var uri)
                ? Path.GetFileName(uri.LocalPath)
                : "download";
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "download";

            var dialog = new SaveFileDialog
            {
                FileName = fileName,
                Filter = "Tous les fichiers (*.*)|*.*"
            };
            var downloadsDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            if (Directory.Exists(downloadsDirectory))
                dialog.InitialDirectory = downloadsDirectory;

            if (dialog.ShowDialog(Window.GetWindow(this)) != true)
            {
                e.Cancel = true;
                e.Handled = true;
                return;
            }

            e.ResultFilePath = dialog.FileName;
            e.Handled = true;
            var download = BrowserServices.Downloads.Start(operation.Uri, e.ResultFilePath);
            DownloadsView.Register(download.Id, operation);
            operation.BytesReceivedChanged += (_, _) =>
                BrowserServices.Downloads.UpdateProgress(download.Id, operation.BytesReceived, TotalBytesToReceive(operation));
            operation.StateChanged += (_, _) =>
            {
                if (operation.State == CoreWebView2DownloadState.Completed)
                {
                    BrowserServices.Downloads.Complete(download.Id, operation.BytesReceived, TotalBytesToReceive(operation));
                    DownloadsView.Unregister(download.Id);
                }
                else if (operation.State == CoreWebView2DownloadState.Interrupted)
                {
                    if (operation.InterruptReason == CoreWebView2DownloadInterruptReason.UserCanceled)
                        BrowserServices.Downloads.Cancel(download.Id);
                    else
                        BrowserServices.Downloads.Fail(download.Id);
                    DownloadsView.Unregister(download.Id);
                }
            };
        }

        private static long TotalBytesToReceive(CoreWebView2DownloadOperation operation)
        {
            return operation.TotalBytesToReceive is ulong bytes && bytes <= long.MaxValue
                ? (long)bytes
                : 0;
        }

        private void OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (DataContext is TabViewModel tab)
            {
                tab.IsLoading = true;
                tab.Url = e.Uri;
            }
        }

        private void OnWebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            if (!Uri.TryCreate(e.Request.Uri, UriKind.Absolute, out var uri))
                return;

            if (BrowserServices.AdBlock.ShouldBlock(uri))
            {
                SetResponse(e, 403, "Bloqué", "text/plain; charset=utf-8", Array.Empty<byte>());
                RefreshStats();
                return;
            }

            var mime = MimeTypeFromUri(uri);
            if (mime == null)
                return;

            var decision = BrowserServices.DataSaver.Evaluate(uri, mime, ContentLengthOf(e));
            if (decision == DataSaverDecision.Block)
            {
                SetResponse(e, 403, "Économisé", "text/plain; charset=utf-8", Array.Empty<byte>());
            }
            else if (decision == DataSaverDecision.Placeholder)
            {
                SetResponse(e, 200, "OK", "image/gif", PlaceholderGif);
            }
        }

        private void SetResponse(CoreWebView2WebResourceRequestedEventArgs e, int status, string reason, string contentType, byte[] body)
        {
            var headers = $"Content-Type: {contentType}\r\nContent-Length: {body.Length}";
            e.Response = WebView.CoreWebView2.Environment
                .CreateWebResourceResponse(new MemoryStream(body), status, reason, headers);
        }

        private static long ContentLengthOf(CoreWebView2WebResourceRequestedEventArgs e)
        {
            if (e.Request.Headers.Contains("Content-Length") &&
                long.TryParse(e.Request.Headers.GetHeader("Content-Length"), out var length))
            {
                return length;
            }
            return 0;
        }

        private static string? MimeTypeFromUri(Uri uri)
        {
            switch (Path.GetExtension(uri.AbsolutePath).ToLowerInvariant())
            {
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".png": return "image/png";
                case ".gif": return "image/gif";
                case ".webp": return "image/webp";
                case ".svg": return "image/svg+xml";
                case ".bmp":
                case ".ico": return "image/bmp";
                case ".mp4":
                case ".m4v": return "video/mp4";
                case ".webm": return "video/webm";
                case ".ogv": return "video/ogg";
                case ".avi": return "video/x-msvideo";
                case ".mkv":
                case ".mov": return "video/x-matroska";
                default: return null;
            }
        }

        private void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            var core = WebView.CoreWebView2;
            if (DataContext is TabViewModel tab)
            {
                tab.IsLoading = false;
                if (e.IsSuccess)
                {
                    tab.Title = string.IsNullOrWhiteSpace(core.DocumentTitle) ? "Nouvel onglet" : core.DocumentTitle;
                    tab.Url = core.Source;
                }
                else if (!string.IsNullOrEmpty(core.Source) &&
                         !core.Source.StartsWith(ErrorPageUrl, StringComparison.OrdinalIgnoreCase))
                {
                    core.Navigate(ErrorPageUrl + "?u=" + Uri.EscapeDataString(core.Source));
                }
            }

            if (e.IsSuccess && IsHistoryWorthy(core.Source))
            {
                BrowserServices.History.AddVisit(core.Source, core.DocumentTitle ?? string.Empty);
            }

            RefreshStats();
            _suppressSuggest = true;
            AddressBar.Text = core.Source;
            _suppressSuggest = false;
            UpdateNavButtons();
        }

        private void OnSourceChanged(object? sender, EventArgs e)
        {
            _suppressSuggest = true;
            AddressBar.Text = WebView.CoreWebView2.Source;
            _suppressSuggest = false;
            if (DataContext is TabViewModel tab)
            {
                tab.Url = WebView.CoreWebView2.Source;
            }
        }

        private static bool IsHistoryWorthy(string url)
        {
            return !string.IsNullOrWhiteSpace(url)
                && !url.StartsWith("https://startpage.local/", StringComparison.OrdinalIgnoreCase)
                && (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                    || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
        }

        private static void RefreshStats()
        {
            BrowserStats.Instance.BlockedCount = BrowserServices.AdBlock.BlockedCount;
        }

        private void AddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseSuggestions();
                if (_coreReady && WebView.CoreWebView2 != null)
                {
                    _suppressSuggest = true;
                    AddressBar.Text = WebView.CoreWebView2.Source ?? string.Empty;
                    _suppressSuggest = false;
                }
                e.Handled = true;
                return;
            }

            if (e.Key != Key.Enter)
                return;

            var text = AddressBar.Text;
            if (string.IsNullOrWhiteSpace(text))
                return;

            var url = ResolveAddress(text);
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                CloseSuggestions();
                return;
            }

            SuggestPopup.IsOpen = false;
            WebView.Source = uri;
            e.Handled = true;
        }

        private void AddressBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressSuggest)
                return;

            var text = AddressBar.Text?.Trim() ?? string.Empty;
            if (text.Length < 2)
            {
                CloseSuggestions();
                return;
            }

            var items = BrowserServices.History.Search(text, 8);
            if (items.Count == 0)
            {
                CloseSuggestions();
                return;
            }

            SuggestList.ItemsSource = items;
            SuggestPopup.IsOpen = true;
        }

        private void SuggestList_Click(object sender, MouseButtonEventArgs e)
        {
            string? url = SuggestList.SelectedItem switch
            {
                MadagascarBrowser.Core.Data.Visit v => v.Url,
                MadagascarBrowser.Core.Data.Favorite f => f.Url,
                _ => null
            };
            if (!string.IsNullOrEmpty(url))
            {
                SuggestPopup.IsOpen = false;
                WebView.Source = new Uri(ResolveAddress(url));
                e.Handled = true;
            }
        }

        private void CloseSuggestions()
        {
            SuggestList.ItemsSource = null;
            SuggestPopup.IsOpen = false;
        }

        private static string ResolveAddress(string input)
        {
            var searchUrl = BrowserServices.CurrentSettings.SearchProvider switch
            {
                "duckduckgo" => "https://duckduckgo.com/?q=",
                "bing" => "https://www.bing.com/search?q=",
                _ => "https://www.google.mg/search?q="
            };
            return BrowserServices.Navigation.ResolveNavigation(input, searchUrl);
        }

        private void WebView2Link_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true
            });
            e.Handled = true;
        }

        public void FocusAddressBar()
        {
            AddressBar.Focus();
            AddressBar.SelectAll();
        }

        public void Dispose()
        {
            try { WebView.Dispose(); } catch { }
        }
    }
}
