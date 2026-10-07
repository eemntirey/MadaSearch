using System;
using System.Text.Encodings.Web;

namespace MadagascarBrowser.Core.Navigation
{
    public class NavigationService
    {
        public string ResolveNavigation(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            input = input.Trim();
            if (IsAbsoluteUrl(input))
            {
                return input;
            }

            if (LooksLikeUrl(input))
            {
                return "https://" + input;
            }

            var encoded = UrlEncoder.Default.Encode(input);
            return "https://www.google.com/search?q=" + encoded;
        }

        public bool LooksLikeUrl(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var s = input.Trim();
            if (IsAbsoluteUrl(s))
                return true;

            if (s.StartsWith("localhost", StringComparison.OrdinalIgnoreCase))
                return true;
            if (System.Net.IPAddress.TryParse(s, out _))
                return true;

            if (s.Contains('.'))
            {
                var parts = s.Split('.');
                if (parts.Length >= 2)
                {
                    var tld = parts[parts.Length - 1];
                    if (tld.Length >= 2 && tld.Length <= 6)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsAbsoluteUrl(string s)
        {
            return s.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || s.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }
    }
}
