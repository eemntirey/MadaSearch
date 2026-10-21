using System;

namespace MadagascarBrowser.Core.Navigation
{
    /// <summary>
    /// État de sécurité de la page affichée, matérialisé par le cadenas de la barre d'adresse.
    /// </summary>
    public enum ConnectionSecurity
    {
        /// <summary>Aucune connexion réseau : page de démarrage, about:blank, fichier local.</summary>
        None,

        /// <summary>Connexion en clair (http://).</summary>
        Insecure,

        /// <summary>Connexion chiffrée et valide (https://).</summary>
        Secure,

        /// <summary>Erreur de certificat détectée lors de la navigation.</summary>
        CertError
    }

    /// <summary>
    /// Détermine l'état de sécurité d'une page à partir de son URL et des erreurs de certificat.
    /// </summary>
    public static class ConnectionSecurityResolver
    {
        /// <summary>
        /// États d'erreur de navigation WebView2 qui correspondent à un problème de certificat.
        /// </summary>
        private static readonly string[] CertificateErrorStatuses =
        {
            "CertificateCommonNameIsIncorrect",
            "CertificateExpired",
            "ClientCertificateContainsErrors",
            "CertificateRevoked",
            "CertificateIsInvalid"
        };

        /// <summary>
        /// Indique si un état d'erreur de navigation correspond à une erreur de certificat.
        /// </summary>
        /// <param name="webErrorStatus">Nom de l'état d'erreur renvoyé par WebView2.</param>
        public static bool IsCertificateError(string? webErrorStatus)
        {
            if (string.IsNullOrWhiteSpace(webErrorStatus))
                return false;

            foreach (var status in CertificateErrorStatuses)
            {
                if (string.Equals(status, webErrorStatus, StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Résout l'état de sécurité.
        /// </summary>
        /// <param name="url">URL de la page affichée.</param>
        /// <param name="hasCertificateError">Vrai si la navigation a échoué sur une erreur de certificat.</param>
        /// <param name="localHost">Hôte virtuel de la page de démarrage (ex. startpage.local).</param>
        public static ConnectionSecurity Resolve(string? url, bool hasCertificateError, string? localHost = null)
        {
            if (hasCertificateError)
                return ConnectionSecurity.CertError;

            if (string.IsNullOrWhiteSpace(url))
                return ConnectionSecurity.None;

            if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri))
                return ConnectionSecurity.None;

            if (!string.IsNullOrEmpty(localHost) &&
                uri.Host.Equals(localHost, StringComparison.OrdinalIgnoreCase))
            {
                return ConnectionSecurity.None;
            }

            if (uri.Scheme == Uri.UriSchemeHttps)
                return ConnectionSecurity.Secure;
            if (uri.Scheme == Uri.UriSchemeHttp)
                return ConnectionSecurity.Insecure;

            return ConnectionSecurity.None;
        }
    }
}
