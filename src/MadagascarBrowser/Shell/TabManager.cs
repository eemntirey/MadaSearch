using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MadagascarBrowser.Shell
{
    public class TabManager
    {
        public ObservableCollection<TabViewModel> Tabs { get; } = new();
        private TabViewModel _active;
        public TabViewModel Active { get => _active; set { _active = value; ActiveChanged?.Invoke(this, EventArgs.Empty); } }

        public event EventHandler ActiveChanged;

        public TabViewModel NewTab()
        {
            var t = new TabViewModel { Title = "Nouvel onglet", Url = "about:blank" };
            Tabs.Add(t);
            Active = t;
            return t;
        }

        public void Close(TabViewModel t)
        {
            if (t == null) return;
            var idx = Tabs.IndexOf(t);
            Tabs.Remove(t);
            if (Active == t)
            {
                if (Tabs.Any())
                {
                    Active = Tabs[Math.Max(0, idx - 1)];
                }
                else
                {
                    Active = null;
                }
            }
        }
    }
}