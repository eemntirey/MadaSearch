namespace MadagascarBrowser.Core.Data
{
    public class BrowserSettings
    {
        public string SearchProvider { get; set; } = "google";
        public bool AdBlockEnabled { get; set; } = true;
        public bool DataSaverEnabled { get; set; }
        public int DataSaverImageThresholdKilobytes { get; set; } = 200;
        public bool RestoreSession { get; set; }

        /// <summary>
        /// Thème d'interface : seul « dark » est disponible dans cette version.
        /// </summary>
        public string Theme { get; set; } = "dark";

        /// <summary>
        /// Densité de l'interface : « comfortable » (défaut) ou « compact ».
        /// </summary>
        public string Density { get; set; } = "comfortable";

        /// <summary>
        /// Couleur d'accent : « malagasy » (défaut), « vert » ou « orange ».
        /// </summary>
        public string Accent { get; set; } = "malagasy";

        /// <summary>
        /// Remplissage automatique général (adresses, formulaires) : opt-in explicite.
        /// Les données restent dans le profil WebView2 local, sans synchronisation.
        /// </summary>
        public bool AutofillEnabled { get; set; }

        /// <summary>
        /// Enregistrement local des mots de passe : opt-in explicite.
        /// Aucun envoi hors de la machine.
        /// </summary>
        public bool PasswordAutosaveEnabled { get; set; }

        /// <summary>
        /// Domaines autorisés : le bloqueur de publicités les laisse passer.
        /// </summary>
        public System.Collections.Generic.List<string> AllowedDomains { get; set; } = new();
    }
}
