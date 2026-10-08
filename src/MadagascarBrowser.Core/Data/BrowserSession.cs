using System.Collections.Generic;

namespace MadagascarBrowser.Core.Data
{
    public record BrowserSession(IReadOnlyList<string> Urls, int ActiveTabIndex);
}