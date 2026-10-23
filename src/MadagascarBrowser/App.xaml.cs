using System;
using System.Threading.Tasks;
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
        AppLog.EnableFileSink();

        // Aucune exception ne doit disparaître : tout ce qui échappe au dispatcheur,
        // à un processus tâche ou au domaine d'application est tracé dans le journal
        // AVANT que le processus ne se termine.
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        AppLog.Info("Démarrage de l'application.");
        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Trace post-mortem : si le processus disparaît sans exception, la dernière
        // ligne du journal indique si la fermeture était attendue ou non.
        AppLog.Info($"Fermeture de l'application (code {e.ApplicationExitCode}).");
        base.OnExit(e);
    }

    private static void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        AppLog.Error("Exception non interceptée dans le dispatcheur de l'interface.", e.Exception);
        // Non absorbée : l'état de l'interface est inconnu après une telle exception.
        // Le journal contient néanmoins la pile complète avant la fin du processus.
    }

    private static void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        AppLog.Error("Exception non interceptée dans le domaine d'application.",
            e.ExceptionObject as Exception ?? new Exception(e.ExceptionObject?.ToString() ?? "exception inconnue"));
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        AppLog.Error("Exception d'une tâche non observée.", e.Exception);
        e.SetObserved();
    }
}
