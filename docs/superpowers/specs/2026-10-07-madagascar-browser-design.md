# Madagascar Browser Design

## 1. Architecture

Solution : MadagascarBrowser.Core (services purs, testables, sans UI), MadagascarBrowser (shell WPF + WebView2), MadagascarBrowser.Tests (xUnit). Injection par constructeur. SQLite (Microsoft.Data.Sqlite).

Structure :
- Shell : MainWindow, TabManager, TabViewModel, BrowserView
- Services : NavigationService, HistoryService, FavoritesService, OfflineCacheService, ProfileSyncService, AdBlockService, DataSaverService
- Data : browser.db, cache/, settings.json
- StartPage : startpage.html/css/js

## 2. Composants

- NavigationService : ResolveNavigation, LooksLikeUrl (détecte URL vs recherche Google)
- AdBlockService : ShouldBlock(Uri), BlockedCount, SetDomainExempt(domain) — parsing de filtre
- DataSaverService : Evaluate → Pass|Block|Placeholder (images >200 Ko → Placeholder, vidéos → Block si activé)
- History/Favorites : SQLite (WAL), add/search/recent, export/import JSON
- OfflineCache : Save/TryGet/List
- ProfileSync : Export/Import profil.json
- TabManager : NewTab, Close, Active, Tabs
- BrowserView : WebView2, barre d'adresse + suggestions, navigation, WebResourceRequested, stats blocage
- StartPage : page locale avec liens Madagascar, recherche, horloge Indian/Antananarivo

## 3. Flux

1. Saisie → NavigationService → Navigate
2. WebResourceRequested → AdBlock + DataSaver → annule/reponse
3. NavigationCompleted → maj onglet (titre/favicon) + History + cache hors-ligne
4. Suggestions → HistoryService.Search

## 4. Gestion des erreurs

- WebView2 indisponible → message + lien DL
- URL inaccessible → ErrorPage locale
- SQLite corrompue → recréation + .bak
- Filtre casse page → désactiver par domaine

## 5. Tests

xUnit sur Core : Navigation, AdBlock, History, Favorites, DataSaver, Offline, Sync. Tests en mémoire/temp files. Aucune dépendance WPF dans Core.

## Constraints
Windows seul, .NET 8, WebView2 >=153, pas de cloud, français, 100% local au démarrage.
