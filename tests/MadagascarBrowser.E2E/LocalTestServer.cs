using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MadagascarBrowser.E2E;

/// <summary>
/// Serveur HTTP minimal (TcpListener) : évite toute réservation http.sys et
/// fournit des pages stables pour la recette, y compris un téléchargement
/// avec Content-Disposition RFC 5987.
/// </summary>
internal sealed class LocalTestServer : IDisposable
{
    private readonly TcpListener _listener;
    private readonly CancellationTokenSource _cancellation = new();
    private readonly Task _loop;

    public LocalTestServer()
    {
        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();
        Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        _loop = Task.Run(AcceptLoopAsync);
    }

    public int Port { get; }

    public string Url(string path) => $"http://127.0.0.1:{Port}{path}";

    private async Task AcceptLoopAsync()
    {
        while (!_cancellation.IsCancellationRequested)
        {
            TcpClient client;
            try
            {
                client = await _listener.AcceptTcpClientAsync(_cancellation.Token);
            }
            catch (Exception) when (_cancellation.IsCancellationRequested)
            {
                return;
            }
            catch (SocketException)
            {
                return;
            }

            _ = Task.Run(() => HandleAsync(client));
        }
    }

    private async Task HandleAsync(TcpClient client)
    {
        using (client)
        {
            try
            {
                using var stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
                var requestLine = await reader.ReadLineAsync();
                if (requestLine is null)
                    return;

                while (!string.IsNullOrEmpty(await reader.ReadLineAsync()))
                {
                }

                var target = requestLine.Split(' ', StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(1) ?? "/";
                var path = target.Split('?')[0];
                var (status, contentType, body, extraHeader) = path switch
                {
                    "/" => (200, "text/html; charset=utf-8", Page("Accueil E2E",
                        "<a id=\"deux\" href=\"/deux\">Aller à la page deux</a>"), string.Empty),
                    "/deux" => (200, "text/html; charset=utf-8", Page("Page Deux",
                        "<a id=\"accueil\" href=\"/\">Retour à l'accueil</a>"), string.Empty),
                    "/download" => (200, "text/plain; charset=utf-8", "contenu-e2e",
                        "Content-Disposition: attachment; filename=\"fallback.txt\"; filename*=UTF-8''recu-%C3%A9t%C3%A9.txt\r\n"),
                    _ => (404, "text/plain; charset=utf-8", "not found", string.Empty)
                };

                var bodyBytes = Encoding.UTF8.GetBytes(body);
                var header = $"HTTP/1.1 {status} OK\r\nContent-Type: {contentType}\r\n" +
                             $"Content-Length: {bodyBytes.Length}\r\n{extraHeader}Connection: close\r\n\r\n";
                var headerBytes = Encoding.ASCII.GetBytes(header);
                await stream.WriteAsync(headerBytes);
                await stream.WriteAsync(bodyBytes);
                await stream.FlushAsync();
            }
            catch (IOException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }

    private static string Page(string title, string content) =>
        $"<!doctype html><html><head><meta charset=\"utf-8\"><title>{title}</title></head>" +
        $"<body><h1 id=\"titre\">{title}</h1>{content}</body></html>";

    public void Dispose()
    {
        _cancellation.Cancel();
        _listener.Stop();
        try
        {
            _loop.Wait(TimeSpan.FromSeconds(2));
        }
        catch (AggregateException)
        {
        }
        _cancellation.Dispose();
    }
}
