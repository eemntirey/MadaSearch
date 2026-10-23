using System;
using System.Diagnostics;
using System.IO;
using MadagascarBrowser.Core.Paths;

namespace MadagascarBrowser.Shell
{
    /// <summary>
    /// Journalisation de l'application.
    /// Chaque message part dans la sortie de débogage et dans la trace .NET.
    /// Un sink fichier est activé par défaut dans %LocalAppData%\MadagascarBrowser\logs\app.log
    /// et, si nécessaire, il fait une rotation simple sans bloquer l'application.
    /// </summary>
    public static class AppLog
    {
        private const int MaxLogFiles = 5;
        private const long MaxLogSizeBytes = 1024L * 1024L;

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

        /// <summary>Active la journalisation fichier locale dans le dossier d'application.</summary>
        public static void EnableFileSink(string? filePath = null)
        {
            var path = string.IsNullOrWhiteSpace(filePath) ? BrowserPaths.Default.LogFilePath : filePath;
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            UseSink(line => AppendLine(path, line));
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

        private static void AppendLine(string path, string line)
        {
            lock (Sync)
            {
                RotateIfNeeded(path);
                using var writer = File.AppendText(path);
                writer.WriteLine($"{DateTimeOffset.Now:O} {line}");
            }
        }

        private static void RotateIfNeeded(string path)
        {
            if (!File.Exists(path))
                return;

            if (new FileInfo(path).Length < MaxLogSizeBytes)
                return;

            for (var index = MaxLogFiles - 1; index >= 1; index--)
            {
                var previous = path + "." + index;
                var next = path + "." + (index + 1);
                if (File.Exists(next))
                    File.Delete(next);
                if (File.Exists(previous))
                    File.Move(previous, next);
            }

            var oldestArchive = path + ".1";
            if (File.Exists(oldestArchive))
                File.Delete(oldestArchive);

            File.Move(path, path + ".1");
        }
    }
}
