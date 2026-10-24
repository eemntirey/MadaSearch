using System;
using System.Collections.Generic;

namespace MadagascarBrowser.Core.Startup
{
    /// <summary>
    /// Options de démarrage de l'application. Elles sont volontairement minimales :
    /// une fenêtre privée et une racine de données isolée (recette GUI).
    /// </summary>
    public sealed record CommandLineOptions(bool IsPrivate, string? DataRoot)
    {
        public static CommandLineOptions Parse(IReadOnlyList<string>? args)
        {
            var isPrivate = false;
            string? dataRoot = null;

            if (args == null)
                return new CommandLineOptions(false, null);

            for (var index = 0; index < args.Count; index++)
            {
                var argument = args[index];
                if (string.Equals(argument, "--private", StringComparison.OrdinalIgnoreCase))
                {
                    isPrivate = true;
                    continue;
                }

                if (string.Equals(argument, "--data-root", StringComparison.OrdinalIgnoreCase))
                {
                    if (index + 1 < args.Count && !string.IsNullOrWhiteSpace(args[index + 1]))
                    {
                        dataRoot = args[index + 1];
                        index++;
                    }
                    continue;
                }

                const string dataRootPrefix = "--data-root=";
                if (argument.StartsWith(dataRootPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    var value = argument[dataRootPrefix.Length..].Trim();
                    if (value.Length > 0)
                        dataRoot = value;
                }
            }

            return new CommandLineOptions(isPrivate, dataRoot);
        }
    }
}
