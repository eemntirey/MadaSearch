using System;
using System.Diagnostics;

namespace MadagascarBrowser.Shell
{
    /// <summary>
    /// Journalisation de l'application.
    /// Chaque message part dans la sortie de débogage et dans la trace .NET.
    /// Un sinks fichier peut s'y brancher : rien n'est alors perdu au redémarrage.
    /// </summary>
    internal static class AppLog
    {
        private static readonly object Sync = new();
        private static Action<string>? _fileSink;

        /// <summary>Branche un récepteur supplémentaire (journalisation dans un fichier).</summary>
        public static void UseSink(Action<string> sink)
        {
            lock (Sync)
            {
                _fileSink = sink;
            }
        }

        public static void Info(string message)
        {
            Write("INFO", message, null);
        }

        public static void Warning(string message)
        {
            Write("WARN", message, null);
        }

        public static void Warning(string message, Exception exception)
        {
            Write("WARN", message, exception);
        }

        public static void Error(string message)
        {
            Write("ERROR", message, null);
        }

        public static void Error(string message, Exception exception)
        {
            Write("ERROR", message, exception);
        }

        private static void Write(string level, string message, Exception? exception)
        {
            var line = exception == null ? $"{level} {message}" : $"{level} {message} | {exception}";

            Debug.WriteLine(line);
            switch (level)
            {
                case "ERROR":
                    Trace.TraceError(line);
                    break;
                case "WARN":
                    Trace.TraceWarning(line);
                    break;
                default:
                    Trace.TraceInformation(line);
                    break;
            }

            Action<string>? sink;
            lock (Sync)
            {
                sink = _fileSink;
            }

            if (sink == null)
                return;

            try
            {
                sink(line);
            }
            catch (Exception sinkException)
            {
                // Un journal défaillant ne doit jamais empêcher l'application de tourner.
                Trace.TraceError($"Écriture du journal impossible : {sinkException}");
            }
        }
    }
}
