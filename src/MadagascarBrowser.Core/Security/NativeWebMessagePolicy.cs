using System;
using MadagascarBrowser.Core.Navigation;

namespace MadagascarBrowser.Core.Security
{
    /// <summary>
    /// Fail-closed policy for messages that may invoke native browser commands.
    /// Only the browser-owned virtual-host origin may use the accelerator bridge.
    /// </summary>
    public static class NativeWebMessagePolicy
    {
        public static bool TryResolveCommand(
            string? source,
            string? message,
            string localHost,
            out AcceleratorCommand command)
        {
            command = AcceleratorCommand.None;

            if (!IsAllowedOrigin(source, localHost) || string.IsNullOrEmpty(message))
                return false;

            command = message switch
            {
                "t" => AcceleratorCommand.NewTab,
                "w" => AcceleratorCommand.CloseTab,
                "l" => AcceleratorCommand.FocusAddressBar,
                _ => AcceleratorCommand.None
            };

            return command != AcceleratorCommand.None;
        }

        public static bool IsAllowedOrigin(string? source, string localHost)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(localHost))
                return false;

            if (!Uri.TryCreate(source, UriKind.Absolute, out var uri))
                return false;

            return uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
                && uri.Host.Equals(localHost, StringComparison.OrdinalIgnoreCase)
                && uri.IsDefaultPort;
        }
    }
}
