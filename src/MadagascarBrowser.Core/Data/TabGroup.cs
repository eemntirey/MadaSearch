namespace MadagascarBrowser.Core.Data
{
    /// <summary>
    /// Groupe d'onglets nommés et colorés, permettant l'organisation
    /// des espaces de travail sans isolation forcée des cookies.
    /// Toute isolation réelle de profil doit utiliser le mécanisme
    /// InPrivate existant.
    /// </summary>
    public class TabGroup
    {
        /// <summary>Identifiant unique du groupe.</summary>
        public int Id { get; set; }

        /// <summary>Nom donné par l'utilisateur.</summary>
        public string Name { get; set; } = "Nouveau groupe";

        /// <summary>Couleur d'accent du groupe (hex).</summary>
        public string ColorHex { get; set; } = "#FF4438"; // rouge malgache par défaut

        /// <summary>Vrai si le groupe est réduit.</summary>
        public bool IsCollapsed { get; set; }

        /// <summary>Liste des URLs des onglets appartenant au groupe.</summary>
        public System.Collections.Generic.List<string> TabUrls { get; } = new();

        /// <summary>Ordre d'affichage dans la liste des groupes.</summary>
        public int OrderIndex { get; set; }
    }
}
