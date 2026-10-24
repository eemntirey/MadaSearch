using System;
using System.Threading;
using System.Threading.Tasks;

namespace MadagascarBrowser.Core.Data
{
    /// <summary>
    /// Retarde l'exécution d'une action et regroupe les appels rapprochés :
    /// seule la dernière action demandée est exécutée, après le délai de repos.
    /// Utilisé pour la requête de suggestions de la barre d'adresse.
    /// </summary>
    public sealed class AsyncDebouncer : IDisposable
    {
        /// <summary>Délai de repos par défaut pour les suggestions d'adresse.</summary>
        public static readonly TimeSpan DefaultDelay = TimeSpan.FromMilliseconds(180);

        private readonly TimeSpan _delay;
        private readonly Func<CancellationToken, Task> _action;
        private readonly object _lock = new();
        private CancellationTokenSource? _pending;
        private bool _disposed;

        public AsyncDebouncer(Func<CancellationToken, Task> action, TimeSpan? delay = null)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _delay = delay ?? DefaultDelay;
        }

        /// <summary>Nombre d'exécutions réelles de l'action (utile pour les tests).</summary>
        public int ExecutionCount { get; private set; }

        /// <summary>
        /// Demande l'exécution de l'action après le délai de repos. Les demandes
        /// intermédiaires sont annulées.
        /// </summary>
        public void Debounce()
        {
            CancellationTokenSource? previous;
            CancellationTokenSource current;

            lock (_lock)
            {
                if (_disposed)
                    return;

                previous = _pending;
                current = new CancellationTokenSource();
                _pending = current;
            }

            previous?.Cancel();
            previous?.Dispose();

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(_delay, current.Token).ConfigureAwait(false);
                    if (current.IsCancellationRequested)
                        return;

                    lock (_lock)
                    {
                        if (_disposed || current.IsCancellationRequested)
                            return;

                        ExecutionCount++;
                    }

                    await _action(current.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Demande annulée par une demande plus récente : rien à faire.
                }
            });
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _disposed = true;
                _pending?.Cancel();
                _pending?.Dispose();
                _pending = null;
            }
        }
    }
}
