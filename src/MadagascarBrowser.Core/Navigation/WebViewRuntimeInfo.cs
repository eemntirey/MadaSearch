using System;

namespace MadagascarBrowser.Core.Navigation
{
    /// <summary>
    /// Canal de distribution du runtime WebView2.
    /// </summary>
    public enum WebViewRuntimeChannel
    {
        Unknown,
        Evergreen,
        Fixed
    }

    /// <summary>
    /// État de sécurité du runtime WebView2.
    /// </summary>
    public enum WebViewRuntimeStatus
    {
        Unknown,
        UpToDate,
        Outdated
    }

    /// <summary>
    /// Héritage assumé : le moteur (TLS, réseau, JavaScript, CSS) appartient à
    /// Microsoft WebView2. L'application ne le remplace pas ; elle expose son
    /// état pour que l'utilisateur sache s'il reçoit les correctifs de sécurité.
    /// </summary>
    public sealed record WebViewRuntimeInfo
    {
        /// <summary>
        /// Version minimale considérée comme maintenue pour cette version de
        /// l'application. En dessous, un runtime figé (Fixed Version) est signalé.
        /// </summary>
        public const string DefaultMinimumSupportedVersion = "130.0.0.0";

        public string? Version { get; init; }

        public WebViewRuntimeChannel Channel { get; init; } = WebViewRuntimeChannel.Unknown;

        public string MinimumSupportedVersion { get; init; } = DefaultMinimumSupportedVersion;

        /// <summary>
        /// Vrai uniquement pour un runtime figé plus ancien que la version minimale.
        /// Un runtime Evergreen est mis à jour par Microsoft : il n'est jamais
        /// signalé comme obsolète par l'application.
        /// </summary>
        public WebViewRuntimeStatus Status
        {
            get
            {
                if (Channel == WebViewRuntimeChannel.Unknown)
                    return WebViewRuntimeStatus.Unknown;

                var version = ParseVersion(Version);
                if (version == null)
                    return WebViewRuntimeStatus.Unknown;

                if (Channel == WebViewRuntimeChannel.Evergreen)
                    return WebViewRuntimeStatus.UpToDate;

                var minimum = ParseVersion(MinimumSupportedVersion) ?? new System.Version(0, 0, 0, 0);
                return version < minimum
                    ? WebViewRuntimeStatus.Outdated
                    : WebViewRuntimeStatus.UpToDate;
            }
        }

        /// <summary>Vrai quand le bandeau de sécurité doit être affiché.</summary>
        public bool RequiresSecurityNotice => Status == WebViewRuntimeStatus.Outdated;

        public string ChannelLabel => Channel switch
        {
            WebViewRuntimeChannel.Evergreen => "Evergreen (mise à jour automatique)",
            WebViewRuntimeChannel.Fixed => "Fixed Version (version figée)",
            _ => "Inconnu"
        };

        public string StatusLabel => Status switch
        {
            WebViewRuntimeStatus.UpToDate => "À jour",
            WebViewRuntimeStatus.Outdated => "Obsolète",
            _ => "Non vérifié"
        };

        /// <summary>
        /// Construit l'état du runtime à partir de la version renvoyée par WebView2
        /// et du mode de déploiement détecté.
        /// </summary>
        /// <param name="versionString">Version brute, par exemple « 141.0.3537.85 ».</param>
        /// <param name="fixedVersionDeployment">Vrai si un runtime figé est utilisé.</param>
        public static WebViewRuntimeInfo Detect(string? versionString, bool fixedVersionDeployment)
        {
            var channel = fixedVersionDeployment
                ? WebViewRuntimeChannel.Fixed
                : string.IsNullOrWhiteSpace(versionString)
                    ? WebViewRuntimeChannel.Unknown
                    : WebViewRuntimeChannel.Evergreen;

            return new WebViewRuntimeInfo
            {
                Version = string.IsNullOrWhiteSpace(versionString) ? null : versionString.Trim(),
                Channel = channel
            };
        }

        /// <summary>
        /// Extrait la partie numérique d'une version WebView2.
        /// Les suffixes de canal (« beta », « (Official build) ») sont ignorés.
        /// </summary>
        public static System.Version? ParseVersion(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            var text = raw.Trim();
            var length = 0;
            while (length < text.Length && (char.IsDigit(text[length]) || text[length] == '.'))
                length++;

            var numeric = text[..length].TrimEnd('.');
            if (numeric.Length == 0)
                return null;

            if (System.Version.TryParse(numeric, out var version))
                return version;

            // « 120 » n'est pas accepté par Version.TryParse : compléter en 120.0.
            if (int.TryParse(numeric, out var major))
                return new System.Version(major, 0);

            return null;
        }
    }
}
