using System.ComponentModel;
using MadagascarBrowser.Core.Navigation;
using MadagascarBrowser.Shell;

namespace MadagascarBrowser.Shell.Tests
{
    public class TabViewModelTests
    {
        [Fact]
        public void Defaults_AreSane()
        {
            var tab = new TabViewModel();
            Assert.Equal("Nouvel onglet", tab.Title);
            Assert.Equal("about:blank", tab.Url);
            Assert.Equal(TabKind.Page, tab.Kind);
            Assert.False(tab.IsLoading);
            Assert.Equal(ConnectionSecurity.None, tab.SecurityState);
        }

        [Fact]
        public void Title_RaisesPropertyChanged()
        {
            var tab = new TabViewModel();
            var raised = new List<string?>();
            tab.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            tab.Title = "Example Domain";

            Assert.Contains(nameof(TabViewModel.Title), raised);
            Assert.Equal("Example Domain", tab.Title);
        }

        [Fact]
        public void SecurityState_UpdatesIndicatorProperties()
        {
            var tab = new TabViewModel();
            var raised = new List<string?>();
            tab.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            tab.SecurityState = ConnectionSecurity.Secure;

            Assert.Contains(nameof(TabViewModel.SecurityState), raised);
            Assert.Contains(nameof(TabViewModel.SecurityGlyph), raised);
            Assert.Contains(nameof(TabViewModel.SecurityLabel), raised);
            Assert.Contains(nameof(TabViewModel.SecurityBrushKey), raised);
        }

        [Fact]
        public void SecurityState_SameValue_DoesNotRaise()
        {
            var tab = new TabViewModel { SecurityState = ConnectionSecurity.Secure };
            var raised = new List<string?>();
            tab.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            tab.SecurityState = ConnectionSecurity.Secure;

            Assert.Empty(raised);
        }

        [Fact]
        public void IsLoading_RaisesPropertyChanged()
        {
            var tab = new TabViewModel();
            var raised = new List<string?>();
            tab.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            tab.IsLoading = true;

            Assert.Contains(nameof(TabViewModel.IsLoading), raised);
            Assert.True(tab.IsLoading);
        }
    }
}
