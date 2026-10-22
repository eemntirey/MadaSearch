using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Text.Json;
using System.Threading;
using Microsoft.Win32;
using MadagascarBrowser.Core.Blocking;
using MadagascarBrowser.Core.Data;
using MadagascarBrowser.Core.Navigation;
using Microsoft.Web.WebView2.Core;

namespace MadagascarBrowser.Shell
{
    public partial class BrowserView : UserControl
    {
        private const string StartPageUrl = WebViewEnvironment.StartPageUrl;
        private const string ErrorPageUrl = WebViewEnvironment.ErrorPageUrl;
        private const string LocalHost = WebViewEnvironment.LocalHost;
        private static readonly byte[] PlaceholderGif =
            Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");
        private static readonly SemaphoreSlim StatsRefreshLock = new(1, 1);

        private readonly ITabHost _tabHost;
        private readonly NewWindowRequestHandler _newWindowHandler;
        private bool _coreReady;
        private bool _suppressSuggest;
        private bool _certificateError;
        private CoreWebView2DevToolsProtocolEventReceiver? _fetchRequestPausedReceiver;
        private AsyncDebouncer? _suggestDebouncer;
        private readonly TaskCompletionSource<CoreWebView2> _coreReadySource =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>
        /// Termine quand le WebView2 est initialisé : permet de raccrocher une nouvelle
        /// fenêtre demandée par une page à l'onglet vide correspondant.
        /// </summary>
        public Task<CoreWebView2> CoreReady => _coreReadySource.Task;

        /// <summary>
        /// Crée une vue de navigation. L'hôte d'onglets est injecté : la vue ne
        /// dépend plus du DataContext de la fenêtre.
        /// </summary>
        /// <param name="tabHost">Hôte d'onglets qui portera les onglets ouverts.</param>
        public BrowserView(ITabHost tabHost)
        {
            _tabHost = tabHost ?? throw new ArgumentNullException(nameof(tabHost));
            _newWindowHandler = new NewWindowRequestHandler(tabHost);
            InitializeComponent();
            BrowserServices.EnsureInitialized();
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_coreReady) return;
            try
            {
                // Environnement explicite : le profil utilisateur WebView2 vit dans
                // %LocalAppData%\MadagascarBrowser\WebView2, jamais à côté de l'exe.
                var environment = await WebViewEnvironment.GetAsync();
                await WebView.EnsureCoreWebView2Async(environment);
                _coreReady = true;
                await InitCoreAsync();
                await RefreshStatsAsync();
                _coreReadySource.TrySetResult(WebView.CoreWebView2);
            }
            catch (Exception exception)
            {
                AppLog.Error("Impossible d'initialiser WebView2.", exception);
                WebView.Visibility = Visibility.Collapsed;
                WebViewError.Visibility = Visibility.Visible;
                WebViewErrorText.Text = $"Impossible d'initialiser WebView2 : {exception.Message}";
                _coreReadySource.TrySetException(exception);
                return;
            }
        }

        private async Task InitCoreAsync()
        {
            var core = WebView.CoreWebView2;
            ConfigureSettings(core.Settings);

            var folder = Path.Combine(AppContext.BaseDirectory, "StartPage");
            if (Directory.Exists(folder))
            {
                core.SetVirtualHostNameToFolderMapping(LocalHost, folder, CoreWebView2HostResourceAccessKind.DenyCors);
            }

            foreach (var pattern in ResourceInterceptionScope.UrlPatterns)
            {
                core.AddWebResourceRequestedFilter(pattern, CoreWebView2WebResourceContext.All);
            }
            core.WebResourceRequested += OnWebResourceRequested;
            core.DownloadStarting += OnDownloadStarting;
            core.NavigationStarting += OnNavigationStarting;
            core.NavigationCompleted += OnNavigationCompleted;
            core.SourceChanged += OnSourceChanged;
            core.HistoryChanged += OnHistoryChanged;
            core.NewWindowRequested += OnNewWindowRequested;
            core.ContainsFullScreenElementChanged += OnContainsFullScreenElementChanged;
            core.ServerCertificateErrorDetected += OnServerCertificateErrorDetected;
            core.WebMessageReceived += OnWebMessageReceived;
            HookAcceleratorKeys();
            await EnableDataSaverInterceptionAsync(core);

            var source = core.Source;
            if (string.IsNullOrWhiteSpace(source) || source == "about:blank")
            {
                var tab = DataContext as TabViewModel;
                var kind = NavigationTargetResolver.Resolve(tab?.Url, out var target, out _);
                switch (kind)
                {
                    case NavigationTargetKind.LocalFile:
                        // Page hors ligne : navigation directe sur le fichier, sans le filtre http/https.
                        core.Navigate(target);
                        break;
                    case NavigationTargetKind.Blank:
                        core.Navigate(NewWindowRequestResolver.BlankUrl);
                        break;
                    case NavigationTargetKind.Web:
                        core.Navigate(target);
                        break;
                    default:
                        core.Navigate(StartPageUrl);
                        break;
                }
            }

            UpdateNavButtons();
        }

        /// <summary>
        /// Audit des réglages WebView2. Chaque choix est documenté : on n'active que ce qui
        /// est utile à Madagascar Browser, on neutralise ce qui exposerait l'utilisateur.
        /// </summary>
        private static void ConfigureSettings(CoreWebView2Settings settings)
        {
            // Zoom Ctrl+molette et Ctrl++/Ctrl+- : activé (navigation en 3G/4G fréquente).
            settings.IsZoomControlEnabled = true;

            // Accélérateurs navigateur (Ctrl+F recherche, Ctrl+P impression, F5, Ctrl+molette) :
            // laissés actifs, l'utilisateur garde les raccourcis habituels d'un navigateur.
            settings.AreBrowserAcceleratorKeysEnabled = true;

            // Menu contextuel natif (copier/coller, lien dans un nouvel onglet) : activé.
            settings.AreDefaultContextMenusEnabled = true;

            // Pas d'enregistrement automatique des mots de passe ni d'autofill Chromium :
            // MadaSearch ne gère pas de coffre-fort, ces fonctions resteraient
            // inaccessibles et stockeraient des secrets sans contrôle de l'utilisateur.
            settings.IsPasswordAutosaveEnabled = false;
            settings.IsGeneralAutofillEnabled = false;

            // Barre d'état WebView2 : désactivée, l'application affiche la sienne.
            settings.IsStatusBarEnabled = false;

            // Page d'erreur WebView2 : désactivée, Madagascar Browser affiche sa page
            // d'erreur maison (startpage.localhost/error.html).
            settings.IsBuiltInErrorPageEnabled = false;

            // Scripts : activés (aucun site ne fonctionne sans JavaScript).
            settings.IsScriptEnabled = true;

            // Messages Web : activés, nécessaire pour les pages hors ligne et l'hôte virtuel.
            settings.IsWebMessageEnabled = true;

            // DevTools : activés pour le diagnostic, sans impact sur l'utilisateur final.
            settings.AreDevToolsEnabled = true;

            // Diapositives au doigt : désactivées, la navigation tactile n'a pas lieu d'être.
            settings.IsSwipeNavigationEnabled = false;
        }

        /// <summary>
        /// Erreur de certificat détectée par WebView2 : c'est le signal le plus fiable
        /// (la navigation est annulée et l'état d'erreur de navigation peut être
        /// générique). L'action par défaut de WebView2 est conservée : le certificat
        /// n'est pas accepté, la navigation échoue.
        /// </summary>
        private void OnServerCertificateErrorDetected(object? sender, CoreWebView2ServerCertificateErrorDetectedEventArgs e)
        {
            _certificateError = true;
        }

        /// <summary>
        /// Une page passe en plein écran : la barre d'outils de l'onglet est masquée
        /// pour laisser la page occuper tout l'espace.
        /// </summary>
        private void OnContainsFullScreenElementChanged(object? sender, object e)
        {
            var isFullScreen = WebView.CoreWebView2?.ContainsFullScreenElement ?? false;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (DataContext is TabViewModel tab)
                    tab.IsFullScreen = isFullScreen;
            }));
        }

        private void OnHistoryChanged(object? sender, object e)
        {
            UpdateNavButtons();
        }

        /// <summary>
        /// Une page demande l'ouverture d'une fenêtre : la fenêtre WebView2 par défaut est
        /// annulée et l'URI est ouverte dans un nouvel onglet de l'application.
        /// </summary>
        private void OnNewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;

            var result = _newWindowHandler.Handle(e.Uri);
            switch (result.Outcome)
            {
                case NewWindowOutcome.OpenedInNewTab:
                    return;

                case NewWindowOutcome.BlankTabOpened:
                    // window.open sans URL : on laisse la page appelante piloter
                    // le contenu de l'onglet vide via l'API WebView2.
                    _ = OpenBlankWindowAsync(e, result.Tab);
                    return;

                default:
                    AppLog.Error($"Demande de nouvelle fenêtre impossible ({result.Error}) : {e.Uri}");
                    return;
            }
        }

        private async Task OpenBlankWindowAsync(CoreWebView2NewWindowRequestedEventArgs e, TabViewModel? tab)
        {
            var deferral = e.GetDeferral();
            try
            {
                if (tab?.View is BrowserView view)
                {
                    // Timeout : l'onglet vide reste utilisable même si WebView2 ne démarre pas.
                    var ready = await Task.WhenAny(view.CoreReady, Task.Delay(TimeSpan.FromSeconds(5)));
                    if (ready == view.CoreReady)
                    {
                        e.NewWindow = await view.CoreReady;
                    }
                    else
                    {
                        AppLog.Warning(
                            "Le WebView2 de l'onglet vide n'a pas démarré à temps : la page appelante perd la main.");
                    }
                }
                else
                {
                    AppLog.Warning("Onglet vide absent : la page appelante perd la main.");
                }
            }
            catch (Exception exception)
            {
                // Onglet vide sans rattachement API : la page appelante perd la main.
                AppLog.Error("Rattachement de la fenêtre vide impossible.", exception);
            }
            finally
            {
                deferral.Complete();
            }
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
            // L'événement AcceleratorKeyPressed du CoreWebView2Controller n'est pas
            // exposé par le contrôle WPF (Microsoft.Web.WebView2.Wpf.WebView2 ne
            // publie pas CoreWebView2Controller), et aucune version du SDK ne le
            // rend accessible : le seul accès possible passait par la réflexion
            // ci-dessous, qui échouait silencieusement.
            // Les raccourcis applicatifs (Ctrl+T / Ctrl+W / Ctrl+L) sont donc
            // interceptés dans la page via AddScriptToExecuteOnDocumentCreatedAsync
            // puis relayés par WebMessageReceived (API publique CoreWebView2).
            _ = WebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(AcceleratorBridgeScript);
        }

        /// <summary>
        /// Script injecté dans chaque page : relaie les raccourcis applicatifs
        /// (Ctrl+T / Ctrl+W / Ctrl+L) vers l'hôte via le canal WebMessage.
        /// </summary>
        private const string AcceleratorBridgeScript = @"
(function () {
  if (window.__madagascarAcceleratorBridge) { return; }
  window.__madagascarAcceleratorBridge = true;
  document.addEventListener('keydown', function (event) {
    if (!event.ctrlKey || event.altKey || event.metaKey) { return; }
    var key = (event.key || '').toLowerCase();
    if (key !== 't' && key !== 'w' && key !== 'l') { return; }
    try {
      window.chrome.webview.postMessage(key);
    } catch (error) {
      /* canal de messages indisponible */
    }
  }, true);
})();";

        /// <summary>
        /// Réception d'un raccourci applicatif relayé par la page.
        /// </summary>
        private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string? key;
            try
            {
                key = e.TryGetWebMessageAsString();
            }
            catch (ArgumentException exception)
            {
                // Message non textuel : la commande est ignoree, mais le journal garde la trace.
                AppLog.Warning("Message Web non textuel reçu.", exception);
                return;
            }

            var command = AcceleratorCommandResolver.Resolve(key, ctrl: true, alt: false, shift: false);
            switch (command)
            {
                case AcceleratorCommand.NewTab:
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _tabHost.NewTab(StartPageUrl);
                    }));
                    break;
                case AcceleratorCommand.CloseTab:
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _tabHost.CloseActiveTab();
                    }));
                    break;
                case AcceleratorCommand.FocusAddressBar:
                    Dispatcher.BeginInvoke(new Action(FocusAddressBar));
                    break;
            }
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
            _tabHost.NewTab(StartPageUrl);
        }

        private void MenuSettings_Click(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = false;
            _tabHost.OpenSettings();
        }

        private async void MenuAddFavorite_Click(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = false;
            if (!Uri.TryCreate(WebView.CoreWebView2?.Source, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                MenuFeedback.Text = "Ouvrez une page Web avant de l'ajouter aux favoris.";
                MenuPopup.IsOpen = true;
                return;
            }

            var url = uri.AbsoluteUri;
            var title = WebView.CoreWebView2.DocumentTitle;
            await DatabaseAccess.WriteAsync(() => BrowserServices.Favorites.Add(url, title));
        }

        private void MenuHistory_Click(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = false;
            _tabHost.OpenLibrary(0);
        }

        private void MenuFavorites_Click(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = false;
            _tabHost.OpenLibrary(1);
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

                var pageUrl = uri.AbsoluteUri;
                var pageTitle = WebView.CoreWebView2.DocumentTitle;
                await DatabaseAccess.WriteAsync(() =>
                    BrowserServices.OfflinePages.Save(pageUrl, pageTitle, mhtml));
                MessageBox.Show(Window.GetWindow(this), "La page a été enregistrée hors ligne.",
                    "Pages hors ligne", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception exception)
            {
                AppLog.Error("Impossible d'enregistrer la page hors ligne.", exception);
                MessageBox.Show(Window.GetWindow(this), $"Impossible d'enregistrer la page : {exception.Message}",
                    "Pages hors ligne", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuOfflinePages_Click(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = false;
            _tabHost.OpenLibrary(2);
        }

        private void MenuDownloads_Click(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = false;
            _tabHost.OpenDownloads();
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
                AppLog.Error("Impossible d'exporter le profil.", exception);
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
                AppLog.Error("Impossible d'importer le profil.", exception);
                MessageBox.Show(Window.GetWindow(this), $"Impossible d'importer le profil : {exception.Message}",
                    "Profil", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void OnDownloadStarting(object? sender, CoreWebView2DownloadStartingEventArgs e)
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
            var download = await DatabaseAccess.WriteAsync(() =>
                BrowserServices.Downloads.Start(operation.Uri, e.ResultFilePath));
            DownloadsView.Register(download.Id, operation);

            // L'avancement n'est plus écrit à chaque événement : l'écriture est
            // limitée à un intervalle de 500 ms ou à un pas de 5 %, et la dernière
            // écriture a lieu à la fin du téléchargement.
            var progressWriter = new DownloadProgressWriter();
            operation.BytesReceivedChanged += (_, _) =>
            {
                var received = operation.BytesReceived;
                var total = TotalBytesToReceive(operation);
                if (!progressWriter.ShouldPersist(received, total, DateTime.UtcNow))
                    return;

                _ = DatabaseAccess.WriteAsync(() =>
                    BrowserServices.Downloads.UpdateProgress(download.Id, received, total));
            };
            operation.StateChanged += (_, _) =>
            {
                if (operation.State == CoreWebView2DownloadState.Completed)
                {
                    var received = operation.BytesReceived;
                    var total = TotalBytesToReceive(operation);
                    _ = DatabaseAccess.WriteAsync(() =>
                        BrowserServices.Downloads.Complete(download.Id, received, total));
                    DownloadsView.Unregister(download.Id);
                }
                else if (operation.State == CoreWebView2DownloadState.Interrupted)
                {
                    if (operation.InterruptReason == CoreWebView2DownloadInterruptReason.UserCanceled)
                        _ = DatabaseAccess.WriteAsync(() => BrowserServices.Downloads.Cancel(download.Id));
                    else
                        _ = DatabaseAccess.WriteAsync(() => BrowserServices.Downloads.Fail(download.Id));
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
            // La page d'erreur interne ne doit pas effacer une erreur de certificat déjà signalée.
            if (!e.Uri.StartsWith(ErrorPageUrl, StringComparison.OrdinalIgnoreCase))
                _certificateError = false;

            if (DataContext is TabViewModel tab)
            {
                tab.IsLoading = true;
                tab.Url = e.Uri;
                tab.SecurityState = ConnectionSecurityResolver.Resolve(e.Uri, _certificateError, LocalHost);
            }
        }

        private void OnWebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            if (!Uri.TryCreate(e.Request.Uri, UriKind.Absolute, out var uri))
                return;

            if (BrowserServices.AdBlock.ShouldBlock(uri))
            {
                SetResponse(e, 403, "Bloqué", "text/plain; charset=utf-8", Array.Empty<byte>());
                _ = RecordMetricAndRefreshAsync(BrowserServices.DataSaverMetrics.RecordAdBlocked);
            }
        }

        private void SetResponse(CoreWebView2WebResourceRequestedEventArgs e, int status, string reason, string contentType, byte[] body)
        {
            var headers = $"Content-Type: {contentType}\r\nContent-Length: {body.Length}";
            e.Response = WebView.CoreWebView2.Environment
                .CreateWebResourceResponse(new MemoryStream(body), status, reason, headers);
        }

        private async Task EnableDataSaverInterceptionAsync(CoreWebView2 core)
        {
            _fetchRequestPausedReceiver = core.GetDevToolsProtocolEventReceiver("Fetch.requestPaused");
            _fetchRequestPausedReceiver.DevToolsProtocolEventReceived += OnFetchRequestPaused;
            try
            {
                var patterns = string.Join(",", ResourceInterceptionScope.FetchPatterns.Select(
                    pattern => $"{{\"urlPattern\":\"{pattern}\",\"requestStage\":\"Response\"}}"));
                await core.CallDevToolsProtocolMethodAsync(
                    "Fetch.enable",
                    $"{{\"patterns\":[{patterns}]}}");
            }
            catch (Exception exception)
            {
                AppLog.Error("Impossible d'activer l'interception des réponses pour l'économiseur de données.", exception);
                _fetchRequestPausedReceiver.DevToolsProtocolEventReceived -= OnFetchRequestPaused;
                _fetchRequestPausedReceiver = null;
            }
        }

        private async void OnFetchRequestPaused(
            object? sender,
            CoreWebView2DevToolsProtocolEventReceivedEventArgs e)
        {
            string? requestId = null;
            try
            {
                using var document = JsonDocument.Parse(e.ParameterObjectAsJson);
                var root = document.RootElement;
                requestId = root.GetProperty("requestId").GetString();
                if (string.IsNullOrEmpty(requestId))
                    throw new InvalidOperationException("La réponse interceptée ne contient pas d'identifiant de requête.");

                var url = root.GetProperty("request").GetProperty("url").GetString();
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                {
                    await ContinuePausedRequestAsync(requestId);
                    return;
                }

                if (BrowserServices.AdBlock.IsDomainExempt(uri))
                {
                    await ContinuePausedRequestAsync(requestId);
                    return;
                }

                var responseHeaders = root.GetProperty("responseHeaders").EnumerateArray()
                    .Select(header => new
                    {
                        Name = header.GetProperty("name").GetString() ?? string.Empty,
                        Value = header.GetProperty("value").GetString() ?? string.Empty
                    })
                    .ToList();
                var contentType = responseHeaders
                    .FirstOrDefault(header => header.Name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                    ?.Value.Split(';', 2)[0].Trim() ?? string.Empty;
                var contentLengthHeader = responseHeaders
                    .FirstOrDefault(header => header.Name.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                    ?.Value;
                long? contentLength = long.TryParse(contentLengthHeader, out var length) && length >= 0
                    ? length
                    : null;
                var statusCode = root.TryGetProperty("responseStatusCode", out var status)
                    ? status.GetInt32()
                    : 0;

                if (statusCode != 200)
                {
                    await ContinuePausedRequestAsync(requestId);
                    return;
                }

                var decision = BrowserServices.DataSaver.Evaluate(uri, contentType, contentLength);
                if (decision == DataSaverDecision.Block)
                {
                    await WebView.CoreWebView2.CallDevToolsProtocolMethodAsync(
                        "Fetch.failRequest",
                        JsonSerializer.Serialize(new { requestId, errorReason = "BlockedByClient" }));
                    _ = RecordMetricAndRefreshAsync(
                        () => BrowserServices.DataSaverMetrics.RecordVideoBlocked(contentLength ?? 0));
                    return;
                }

                if (decision == DataSaverDecision.Placeholder && contentLength.HasValue)
                {
                    var replacementHeaders = responseHeaders
                        .Where(header => !IsPayloadHeader(header.Name))
                        .Select(header => new Dictionary<string, string>
                        {
                            ["name"] = header.Name,
                            ["value"] = header.Value
                        })
                        .ToList();
                    replacementHeaders.Add(new Dictionary<string, string> { ["name"] = "Content-Type", ["value"] = "image/gif" });
                    replacementHeaders.Add(new Dictionary<string, string> { ["name"] = "Content-Length", ["value"] = PlaceholderGif.Length.ToString() });

                    await WebView.CoreWebView2.CallDevToolsProtocolMethodAsync(
                        "Fetch.fulfillRequest",
                        JsonSerializer.Serialize(new
                        {
                            requestId,
                            responseCode = statusCode,
                            responseHeaders = replacementHeaders,
                            body = Convert.ToBase64String(PlaceholderGif)
                        }));
                    _ = RecordMetricAndRefreshAsync(
                        () => BrowserServices.DataSaverMetrics.RecordImageReplaced(contentLength.Value));
                    return;
                }

                await ContinuePausedRequestAsync(requestId);
            }
            catch (Exception exception)
            {
                AppLog.Error("Erreur pendant l'interception d'une réponse Web.", exception);
                if (!string.IsNullOrEmpty(requestId))
                {
                    try
                    {
                        await ContinuePausedRequestAsync(requestId);
                    }
                    catch (Exception continuationException)
                    {
                        AppLog.Error("Impossible de reprendre la réponse Web interceptée.", continuationException);
                    }
                }
            }
        }

        private static bool IsPayloadHeader(string name)
        {
            return name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)
                || name.Equals("Content-Length", StringComparison.OrdinalIgnoreCase)
                || name.Equals("Content-Encoding", StringComparison.OrdinalIgnoreCase)
                || name.Equals("Content-Range", StringComparison.OrdinalIgnoreCase)
                || name.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase)
                || name.Equals("Content-MD5", StringComparison.OrdinalIgnoreCase);
        }

        private Task<string> ContinuePausedRequestAsync(string requestId)
        {
            return WebView.CoreWebView2.CallDevToolsProtocolMethodAsync(
                "Fetch.continueRequest",
                JsonSerializer.Serialize(new { requestId }));
        }

        private void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            var core = WebView.CoreWebView2;
            if (!e.IsSuccess && ConnectionSecurityResolver.IsCertificateError(e.WebErrorStatus.ToString()))
                _certificateError = true;
            if (DataContext is TabViewModel tab)
            {
                tab.IsLoading = false;
                tab.SecurityState = ConnectionSecurityResolver.Resolve(core.Source, _certificateError, LocalHost);
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
                var url = core.Source;
                var title = core.DocumentTitle ?? string.Empty;
                _ = DatabaseAccess.WriteAsync(() => BrowserServices.History.AddVisit(url, title));
            }

            _ = RefreshStatsAsync();
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
                && !url.StartsWith("https://" + WebViewEnvironment.LocalHost + "/", StringComparison.OrdinalIgnoreCase)
                && (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                    || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
        }

        private static async Task RefreshStatsAsync()
        {
            var lockTaken = false;
            try
            {
                await StatsRefreshLock.WaitAsync();
                lockTaken = true;
                var metrics = await DatabaseAccess.ReadAsync(BrowserServices.DataSaverMetrics.GetSnapshot);
                var culture = System.Globalization.CultureInfo.CurrentCulture;
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    BrowserStats.Instance.BlockedCount = metrics.SessionBlockedAds;
                    BrowserStats.Instance.SavedDataText =
                        $"↓ {(metrics.SessionSavedBytes / (1024d * 1024d)).ToString("N1", culture)} Mo économisés";
                    BrowserStats.Instance.SavedDataTooltip =
                        $"Cette session : {metrics.SessionSavedBytes.ToString("N0", culture)} octets. " +
                        $"Cumul : {metrics.TotalSavedBytes.ToString("N0", culture)} octets.";
                });
            }
            catch (Exception exception)
            {
                AppLog.Error("Impossible de lire les métriques du navigateur.", exception);
            }
            finally
            {
                if (lockTaken)
                    StatsRefreshLock.Release();
            }
        }

        private static async Task RecordMetricAndRefreshAsync(Action record)
        {
            try
            {
                await DatabaseAccess.WriteAsync(record);
            }
            catch (Exception exception)
            {
                AppLog.Error("Impossible d'enregistrer une métrique du navigateur.", exception);
                return;
            }

            await RefreshStatsAsync();
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
                _suggestDebouncer?.Dispose();
                _suggestDebouncer = null;
                return;
            }

            // Requête SQLite différée et exécutée hors du thread UI.
            _suggestDebouncer?.Dispose();
            _suggestDebouncer = new AsyncDebouncer(async token =>
            {
                var items = await DatabaseAccess.ReadAsync(() => BrowserServices.History.Search(text, 8));
                if (token.IsCancellationRequested)
                    return;

                await Dispatcher.InvokeAsync(() =>
                {
                    if (items.Count == 0)
                    {
                        CloseSuggestions();
                        return;
                    }

                    SuggestList.ItemsSource = items;
                    SuggestPopup.IsOpen = true;
                });
            });
            _suggestDebouncer.Debounce();
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
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri)
                {
                    UseShellExecute = true
                });
            }
            catch (Exception exception)
            {
                // Lien externe (protocole applicatif) : l'ouverture peut échouer.
                AppLog.Warning($"Ouverture du lien externe '{e.Uri}' impossible.", exception);
            }
            e.Handled = true;
        }

        public void FocusAddressBar()
        {
            AddressBar.Focus();
            AddressBar.SelectAll();
        }

        public void Dispose()
        {
            _suggestDebouncer?.Dispose();
            _suggestDebouncer = null;
            if (_fetchRequestPausedReceiver != null)
            {
                _fetchRequestPausedReceiver.DevToolsProtocolEventReceived -= OnFetchRequestPaused;
                _fetchRequestPausedReceiver = null;
            }
            try
            {
                WebView.Dispose();
            }
            catch (Exception exception)
            {
                AppLog.Warning("Liberation du WebView2 incomplete.", exception);
            }
        }
    }
}
