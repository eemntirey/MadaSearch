using System;
using System.Reflection;
using System.Windows;
using MadagascarBrowser.Core.Navigation;

namespace MadagascarBrowser.Shell
{
    /// <summary>
    /// Version de l'application et état du runtime WebView2.
    /// Aucune fausse promesse : un runtime figé ancien est explicitement signalé.
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            var version = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                ?? "inconnue";
            AppVersionText.Text = version;

            var runtime = WebViewRuntimeService.GetRuntimeInfo();
            WebViewVersionText.Text = runtime.Version == null
                ? "Version indisponible"
                : $"{runtime.Version} — {runtime.ChannelLabel}";
            RuntimeStatusText.Text = runtime.Status == WebViewRuntimeStatus.Outdated
                ? $"État : {runtime.StatusLabel} (minimum attendu : {runtime.MinimumSupportedVersion}). " +
                  "Mettez à jour le runtime pour recevoir les correctifs de sécurité."
                : $"État : {runtime.StatusLabel}.";
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
