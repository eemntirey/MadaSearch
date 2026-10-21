using System;

namespace MadagascarBrowser.Core.Navigation
{
    /// <summary>
    /// Représentation visuelle de l'état de sécurité d'une page : glyphe affiché
    /// dans la barre d'adresse, libellé accessible et clé du jeton de couleur.
    /// </summary>
    public sealed record SecurityIndicator(string Glyph, string Label, string BrushKey)
    {
        /// <summary>État sans connexion réseau (page de démarrage, about:blank, fichier local).</summary>
        public static SecurityIndicator None { get; } = new(
            "\uE774",
            "Aucune connexion réseau",
            "FaintBrush");

        /// <summary>Connexion chiffrée et valide.</summary>
        public static SecurityIndicator Secure { get; } = new(
            "\uE72E",
            "Connexion chiffrée (HTTPS)",
            "SecureBrush");

        /// <summary>Connexion en clair.</summary>
        public static SecurityIndicator Insecure { get; } = new(
            "\uE7BA",
            "Connexion non chiffrée (HTTP)",
            "WarnBrush");

        /// <summary>Erreur de certificat : la page affichée n'est pas fiable.</summary>
        public static SecurityIndicator CertificateError { get; } = new(
            "\uEA39",
            "Erreur de certificat",
            "DangerBrush");
    }

    /// <summary>
    /// Associe un état de sécurité à sa représentation visuelle.
    /// La règle vit dans le Core : l'interface ne fait que l'afficher.
    /// </summary>
    public static class SecurityIndicatorResolver
    {
        /// <summary>Renvoie l'indicateur correspondant à l'état de sécurité.</summary>
        public static SecurityIndicator Resolve(ConnectionSecurity state)
        {
            return state switch
            {
                ConnectionSecurity.Secure => SecurityIndicator.Secure,
                ConnectionSecurity.Insecure => SecurityIndicator.Insecure,
                ConnectionSecurity.CertError => SecurityIndicator.CertificateError,
                _ => SecurityIndicator.None
            };
        }
    }
}
