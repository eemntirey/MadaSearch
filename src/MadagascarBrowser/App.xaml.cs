using System.Configuration;
using System.Data;
using System.Windows;
using MadagascarBrowser.Shell;

namespace MadagascarBrowser;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Les durées d'animation sont résolues au chargement des styles : on applique
        // donc la préférence système avant l'ouverture de la fenêtre.
        Motion.ApplyPreference();
        base.OnStartup(e);
    }
}
