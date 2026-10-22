using System;

namespace MadagascarBrowser.Shell
{
    /// <summary>
    /// Choix de l'onglet à activer après la fermeture d'un onglet.
    /// La règle est isolée du Shell pour être vérifiable par des tests.
    /// </summary>
    public static class TabSelection
    {
        /// <summary>
        /// Renvoie l'index de l'onglet à activer après la fermeture de l'onglet
        /// situé à <paramref name="removedIndex"/>.
        /// L'onglet de gauche est préféré ; le premier onglet restant est utilisé
        /// s'il n'y en a pas à gauche.
        /// </summary>
        /// <param name="removedIndex">Index de l'onglet fermé.</param>
        /// <param name="remainingCount">Nombre d'onglets restants.</param>
        /// <returns>Index de l'onglet à activer, ou -1 s'il ne reste aucun onglet.</returns>
        public static int NextIndex(int removedIndex, int remainingCount)
        {
            if (remainingCount <= 0)
                return -1;
            return Math.Clamp(removedIndex - 1, 0, remainingCount - 1);
        }
    }
}
