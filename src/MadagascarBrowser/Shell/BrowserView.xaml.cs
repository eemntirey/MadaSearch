using System.Windows.Controls;
using System.Windows.Input;

namespace MadagascarBrowser.Shell
{
    public partial class BrowserView : UserControl
    {
        public BrowserView()
        {
            InitializeComponent();
        }

        private void AddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrWhiteSpace(AddressBar.Text))
                {
                    WebView.Source = new System.Uri(AddressBar.Text.StartsWith("http") ? AddressBar.Text : "https://" + AddressBar.Text);
                }
            }
        }
    }
}