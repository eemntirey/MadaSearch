using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MadagascarBrowser.Core.Data;
using Microsoft.Web.WebView2.Core;

namespace MadagascarBrowser.Shell
{
    public partial class DownloadsView : UserControl
    {
        private static readonly Dictionary<long, CoreWebView2DownloadOperation> ActiveOperations = new();
        private readonly DispatcherTimer _refreshTimer = new() { Interval = TimeSpan.FromSeconds(1) };

        public DownloadsView()
        {
            InitializeComponent();
            BrowserServices.EnsureInitialized();
            RefreshDownloads();
            _refreshTimer.Tick += (_, _) => RefreshDownloads();
            Loaded += (_, _) =>
            {
                RefreshDownloads();
                _refreshTimer.Start();
            };
            Unloaded += (_, _) => _refreshTimer.Stop();
        }

        public static void Register(long id, CoreWebView2DownloadOperation operation)
        {
            lock (ActiveOperations)
                ActiveOperations[id] = operation;
        }

        public static void Unregister(long id)
        {
            lock (ActiveOperations)
                ActiveOperations.Remove(id);
        }

        private async void RefreshDownloads()
        {
            var selectedId = (DownloadsList.SelectedItem as BrowserDownload)?.Id;
            DownloadsList.ItemsSource = await DatabaseAccess.ReadAsync(() => BrowserServices.Downloads.GetRecent(100));
            if (selectedId.HasValue)
                DownloadsList.SelectedItem = DownloadsList.Items.Cast<BrowserDownload>().FirstOrDefault(item => item.Id == selectedId);
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshDownloads();
        }

        private void DownloadsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DownloadsList.SelectedItem is not BrowserDownload download)
            {
                CancelButton.IsEnabled = false;
                OpenButton.IsEnabled = false;
                return;
            }

            CancelButton.IsEnabled = download.State == DownloadState.InProgress;
            OpenButton.IsEnabled = download.State == DownloadState.Completed && File.Exists(download.Path);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (DownloadsList.SelectedItem is not BrowserDownload download)
                return;

            lock (ActiveOperations)
            {
                if (ActiveOperations.TryGetValue(download.Id, out var operation))
                    operation.Cancel();
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (DownloadsList.SelectedItem is not BrowserDownload download ||
                download.State != DownloadState.Completed || !File.Exists(download.Path))
                return;

            Process.Start(new ProcessStartInfo(download.Path) { UseShellExecute = true });
        }
    }
}
