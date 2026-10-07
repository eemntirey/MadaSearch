using System.Windows;
using MadagascarBrowser.Shell;

namespace MadagascarBrowser
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var tm = new TabManager();
            tm.NewTab();
            DataContext = tm;
        }
    }
}