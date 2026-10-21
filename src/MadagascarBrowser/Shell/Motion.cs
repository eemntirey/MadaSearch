using System;
using System.Windows;

namespace MadagascarBrowser.Shell
{
    /// <summary>
    /// Règles de mouvement de l'interface. Les durées sont pilotées par les jetons
    /// MotionFast / MotionMedium / MotionSlow : elles passent à zéro quand l'utilisateur
    /// a désactivé les animations du système.
    /// </summary>
    public static class Motion
    {
        /// <summary>
        /// Applique la préférence système d'animations. À appeler au démarrage,
        /// avant la création de la fenêtre (les styles résolvent les durées au chargement).
        /// </summary>
        public static void ApplyPreference()
        {
            if (SystemParameters.ClientAreaAnimation)
                return;

            var resources = Application.Current?.Resources;
            if (resources == null) return;

            resources["MotionFast"] = new Duration(TimeSpan.Zero);
            resources["MotionMedium"] = new Duration(TimeSpan.Zero);
            resources["MotionSlow"] = new Duration(TimeSpan.Zero);
        }
    }
}
