using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MadagascarBrowser.Core.Blocking
{
    public class AdBlockService
    {
        private readonly HashSet<string> _domains = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> _domainSuffixes = new();
        private readonly HashSet<string> _exempt = new(StringComparer.OrdinalIgnoreCase);
        private int _blocked;

        public AdBlockService(string? filterListPath = null)
        {
            LoadFilters(filterListPath);
        }

        public bool Enabled { get; set; } = true;
        public int BlockedCount => _blocked;

        public void SetDomainExempt(string domain)
        {
            if (!string.IsNullOrWhiteSpace(domain))
                _exempt.Add(domain.Trim());
        }

        public bool ShouldBlock(Uri url)
        {
            if (!Enabled || url == null)
                return false;
            var host = url.Host.ToLowerInvariant();
            if (_exempt.Contains(host))
                return false;
            if (_domains.Contains(host))
            {
                _blocked++;
                return true;
            }
            foreach (var d in _domainSuffixes)
            {
                if (host.EndsWith(d, StringComparison.OrdinalIgnoreCase))
                {
                    _blocked++;
                    return true;
                }
            }
            return false;
        }

        private void LoadFilters(string? path)
        {
            string? text = null;
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                text = File.ReadAllText(path);
            }
            else
            {
                var asm = Assembly.GetExecutingAssembly();
                using var s = asm.GetManifestResourceStream("MadagascarBrowser.Core.Blocking.FilterList.txt");
                if (s != null)
                {
                    using var r = new StreamReader(s);
                    text = r.ReadToEnd();
                }
            }
            if (string.IsNullOrEmpty(text))
                return;
            foreach (var line in text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var l = line.Trim();
                if (l.StartsWith("!") || l.StartsWith("[") || l.StartsWith("#"))
                    continue;
                if (l.StartsWith("||") && l.EndsWith("^"))
                {
                    var d = l.Substring(2, l.Length - 3);
                    if (!string.IsNullOrWhiteSpace(d))
                    {
                        _domainSuffixes.Add("." + d.ToLowerInvariant());
                        _domains.Add(d.ToLowerInvariant());
                    }
                    continue;
                }
                if (!l.Contains(" ") && !l.Contains("/") && l.Contains("."))
                {
                    _domains.Add(l.ToLowerInvariant());
                    _domainSuffixes.Add("." + l.ToLowerInvariant());
                }
            }
        }
    }
}