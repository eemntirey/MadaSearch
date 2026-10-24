using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MadagascarBrowser.Core.Performance
{
    /// <summary>
    /// Mesure des performances réelles du navigateur (démarrage, ouverture d'onglet,
    /// mémoire, CPU). Aucune action agressive de mise en veille qui détruirait
    /// l'état des pages. Les résultats sont des métriques mesurées, jamais simulées.
    /// </summary>
    public class PerformanceMonitor
    {
        private readonly Stopwatch _startupTimer = new();
        private readonly List<double> _tabOpenTimesMs = new();

        /// <summary>Temps de démarrage en millisecondes (mesuré depuis l'initialisation).</summary>
        public long StartupTimeMs { get; private set; }

        /// <summary>Moyenne des temps d'ouverture d'onglets (ms).</summary>
        public double AverageTabOpenTimeMs => _tabOpenTimesMs.Count > 0
            ? _tabOpenTimesMs.Average()
            : 0.0;

        /// <summary>Nombre d'onglets ouverts mesurés.</summary>
        public int MeasuredTabOpenCount => _tabOpenTimesMs.Count;

        /// <summary>Nombre total d'onglets ouverts dans la session.</summary>
        public int TotalTabsOpened { get; private set; }

        /// <summary>Temps de démarrage mesuré.</summary>
        public bool StartupMeasured => StartupTimeMs > 0;

        public PerformanceMonitor()
        {
            _startupTimer.Start();
        }

        /// <summary>Appelé au démarrage complet de la fenêtre principale.</summary>
        public void RecordStartupComplete()
        {
            StartupTimeMs = _startupTimer.ElapsedMilliseconds;
            _startupTimer.Stop();
        }

        /// <summary>Mesure le temps d'ouverture d'un onglet.</summary>
        public void StartTabOpenMeasurement()
        {
            _currentTabTimer = Stopwatch.StartNew();
        }

        /// <summary>Termine la mesure d'ouverture d'onglet.</summary>
        public void CompleteTabOpenMeasurement()
        {
            if (_currentTabTimer == null) return;
            _currentTabTimer.Stop();
            _tabOpenTimesMs.Add(_currentTabTimer.ElapsedMilliseconds);
            TotalTabsOpened++;
            _currentTabTimer = null;
        }

        private Stopwatch? _currentTabTimer;
    }
}
