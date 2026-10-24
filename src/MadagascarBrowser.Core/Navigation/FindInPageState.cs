using System;

namespace MadagascarBrowser.Core.Navigation
{
    /// <summary>
    /// État de la recherche dans la page (Ctrl+F).
    /// La règle — recherche suivante, précédente, compteurs — vit dans le Core ;
    /// la barre de recherche ne fait que l'afficher.
    /// </summary>
    public sealed class FindInPageState
    {
        private string _query = string.Empty;
        private int _matchIndex;
        private int _matchCount;

        /// <summary>Recherche courante.</summary>
        public string Query => _query;

        /// <summary>Nombre de correspondances trouvées dans la page.</summary>
        public int MatchCount => _matchCount;

        /// <summary>
        /// Index de la correspondance affichée (1-based). Vaut 0 quand aucune
        /// correspondance n'est affichée.
        /// </summary>
        public int MatchIndex => _matchIndex;

        /// <summary>Vrai quand une recherche est en cours.</summary>
        public bool IsActive => _query.Length > 0;

        /// <summary>
        /// Lance une nouvelle recherche : la première correspondance est sélectionnée.
        /// </summary>
        /// <param name="query">Texte recherché.</param>
        public void Start(string? query)
        {
            _query = (query ?? string.Empty).Trim();
            _matchCount = 0;
            _matchIndex = 0;
        }

        /// <summary>
        /// Signale le résultat de la recherche courante : nombre de correspondances
        /// et index de celle qui est sélectionnée.
        /// </summary>
        /// <param name="matchCount">Nombre de correspondances trouvées.</param>
        /// <param name="matchIndex">Index de la correspondance sélectionnée (1-based).</param>
        public void ReportMatches(int matchCount, int matchIndex)
        {
            _matchCount = Math.Max(0, matchCount);
            if (_matchCount == 0)
            {
                _matchIndex = 0;
                return;
            }
            _matchIndex = matchIndex <= 0 ? 1 : Math.Min(matchIndex, _matchCount);
        }

        /// <summary>
        /// Calcule l'index de la correspondance suivante.
        /// </summary>
        /// <param name="forward">Vrai pour la correspondance suivante, faux pour la précédente.</param>
        public int NextIndex(bool forward)
        {
            if (_matchCount == 0)
                return 0;

            if (forward)
                return _matchIndex >= _matchCount ? 1 : _matchIndex + 1;

            return _matchIndex <= 1 ? _matchCount : _matchIndex - 1;
        }

        /// <summary>Libère la recherche.</summary>
        public void Clear()
        {
            _query = string.Empty;
            _matchCount = 0;
            _matchIndex = 0;
        }
    }
}
