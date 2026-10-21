using MadagascarBrowser.Shell;

namespace MadagascarBrowser.Shell.Tests
{
    /// <summary>
    /// Tests de la logique pure de <see cref="TabManager"/> : commutation d'onglet actif,
    /// fermeture, réutilisation des onglets de service. Les chemins qui créent un
    /// <c>BrowserView</c> (donc WebView2 + STA) sont exclus — ils relèvent des
    /// vérifications manuelles de phase.
    /// </summary>
    public class TabManagerTests
    {
        private static TabViewModel AddTab(TabManager manager, string title)
        {
            var tab = new TabViewModel { Title = title };
            manager.Tabs.Add(tab);
            return tab;
        }

        [Fact]
        public void Active_Set_RaisesActiveChangedAndPropertyChanged()
        {
            var manager = new TabManager();
            var first = AddTab(manager, "A");
            var second = AddTab(manager, "B");
            manager.Active = first;

            var activeChanged = 0;
            var propertyChanged = new List<string?>();
            manager.ActiveChanged += (_, _) => activeChanged++;
            manager.PropertyChanged += (_, e) => propertyChanged.Add(e.PropertyName);

            manager.Active = second;

            Assert.Equal(1, activeChanged);
            Assert.Contains(nameof(TabManager.Active), propertyChanged);
            Assert.Same(second, manager.Active);
        }

        [Fact]
        public void Active_SameReference_DoesNotRaise()
        {
            var manager = new TabManager();
            var tab = AddTab(manager, "A");
            manager.Active = tab;

            var activeChanged = 0;
            manager.ActiveChanged += (_, _) => activeChanged++;

            manager.Active = tab;

            Assert.Equal(0, activeChanged);
        }

        [Fact]
        public void Close_RemovesTab_AndActivatesPrevious()
        {
            var manager = new TabManager();
            var first = AddTab(manager, "A");
            var second = AddTab(manager, "B");
            var third = AddTab(manager, "C");
            manager.Active = third;

            manager.Close(third);

            Assert.Equal(2, manager.Tabs.Count);
            Assert.DoesNotContain(third, manager.Tabs);
            Assert.Same(second, manager.Active);
            Assert.Contains(first, manager.Tabs);
        }

        [Fact]
        public void Close_InactiveTab_KeepsActiveUnchanged()
        {
            var manager = new TabManager();
            var first = AddTab(manager, "A");
            var second = AddTab(manager, "B");
            var third = AddTab(manager, "C");
            manager.Active = third;

            manager.Close(second);

            Assert.Equal(2, manager.Tabs.Count);
            Assert.Same(third, manager.Active);
        }

        [Fact]
        public void Close_ActiveTabLast_DoesNotThrow_WhenOtherTabsRemain()
        {
            var manager = new TabManager();
            var first = AddTab(manager, "A");
            var second = AddTab(manager, "B");
            manager.Active = second;

            manager.Close(second);

            Assert.Single(manager.Tabs);
            Assert.Same(first, manager.Active);
        }

        [Fact]
        public void Close_Null_IsNoOp()
        {
            var manager = new TabManager();
            AddTab(manager, "A");

            manager.Close(null!);

            Assert.Single(manager.Tabs);
        }

        [Fact]
        public void Close_TabWithoutView_DoesNotThrow()
        {
            var manager = new TabManager();
            var first = AddTab(manager, "A");
            var second = AddTab(manager, "B");
            manager.Active = first;

            manager.Close(second);

            Assert.Single(manager.Tabs);
            Assert.Same(first, manager.Active);
        }

        [Fact]
        public void RestoreSession_WithOnlyInvalidUrls_DoesNotCreateTabs()
        {
            var manager = new TabManager();
            var session = new MadagascarBrowser.Core.Data.BrowserSession(
                new[] { "not-a-url", "ftp://example.com/file" }, 0);

            manager.RestoreSession(session);

            Assert.Empty(manager.Tabs);
        }

        [Fact]
        public void TabManager_ImplementsITabHost()
        {
            ITabHost host = new TabManager();
            Assert.Null(host.Active);
        }
    }
}
