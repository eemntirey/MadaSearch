namespace MadagascarBrowser.Core.Navigation;

public sealed record NavigationFailureInfo(string Title, string Message);

public static class NavigationFailureResolver
{
    public static NavigationFailureInfo Resolve(string webErrorStatus)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(webErrorStatus);

        return webErrorStatus switch
        {
            "HostNameNotResolved" => new(
                "Site introuvable",
                "L’adresse du site n’a pas pu être résolue. Vérifiez l’adresse et réessayez."),
            "CannotConnect" or "ServerUnreachable" => new(
                "Serveur inaccessible",
                "Le serveur n’a pas accepté la connexion. Réessayez plus tard."),
            "Timeout" => new(
                "Délai de réponse dépassé",
                "Le site a mis trop de temps à répondre."),
            "ConnectionAborted" or "ConnectionReset" or "Disconnected" => new(
                "Connexion interrompue",
                "La connexion a été interrompue avant la fin du chargement."),
            "CertificateCommonNameIsIncorrect" or "CertificateExpired" or "CertificateIsInvalid" or "CertificateRevoked" =>
                new("Certificat non valide", "Le certificat de sécurité du site n’a pas pu être vérifié."),
            "ErrorHttpInvalidServerResponse" => new(
                "Réponse du site invalide",
                "Le serveur a renvoyé une réponse que le navigateur ne peut pas traiter."),
            "OperationCanceled" => new(
                "Chargement interrompu",
                "Le chargement de cette page a été interrompu."),
            _ => new(
                "Page inaccessible",
                $"Le chargement a échoué ({webErrorStatus}).")
        };
    }
}
