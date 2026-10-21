# Browser Shell Integration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Brancher la shell WPF (BrowserView/MainWindow) sur les services Core existants, ajouter la page d'accueil locale Madagascar et les suggestions d'historique — le navigateur devient réellement utilisable.

**Architecture:** Services partagés via `BrowserServices` (statique, initialisé par la shell) ; chaque `TabViewModel` possède sa vue `BrowserView` en cache (`View`) pour préserver l'état WebView2 au changement d'onglet ; page d'accueil servie via `SetVirtualHostNameToFolderMapping("startpage.local", …)` ; historique/blocage alimentés par événements WebView2.

**Tech Stack:** .NET 8, WPF, Microsoft.Web.WebView2 1.0.2849.39, Microsoft.Data.Sqlite, xUnit

**Spec:** docs/superpowers/specs/2026-10-07-madagascar-browser-design.md

## Global Constraints
- Windows, .NET 8, WebView2 >=153
- Pas de cloud, français, 100% local au démarrage
- Couleur imposée : fond sombre `#0E0C0A` issu du mélange des couleurs du drapeau malgache (blanc `#FFFFFF`, rouge `#FC3D32`, vert `#007E3A`), accents tricolores, thème appliqué dans MainWindow/BrowserView
- Code Core = testable sans WPF ; tests xUnit sur Core uniquement
- Commits par task (voir Global Constraints du plan mère)

---

### Task 1 : Hygiène git

**Files:**
- Create: `.gitignore`

- [x] **Step 1:** Créer `.gitignore` avec : `bin/`, `obj/`, `.vs/`, `*.user`
- [ ] **Step 2:** `git rm -r --cached` sur les artefacts bin/obj déjà trackés (281 fichiers), stage uniquement
- [ ] **Step 3:** Vérifier `git status` : les bin/obj ne réapparaissent plus comme modifiés après un build

Note : Step 2-3 attendent la décision de commit de l'utilisateur (le déstage n'a d'effet qu'au commit).

---

### Task 2 : BrowserView branchée aux services Core

**Files:**
- Create: `src/MadagascarBrowser/Shell/BrowserServices.cs`
- Create: `src/MadagascarBrowser/Shell/BrowserStats.cs`
- Modify: `src/MadagascarBrowser/Shell/BrowserView.xaml` (barre d'adresse dans un Grid)
- Modify: `src/MadagascarBrowser/Shell/BrowserView.xaml.cs` (tout le wiring)
- Modify: `src/MadagascarBrowser/Shell/TabManager.cs:15-21` (NewTab crée la vue, Url = startpage)
- Modify: `src/MadagascarBrowser/MainWindow.xaml:21-26` (ContentTemplate → `Content="{Binding View}"`)
- Modify: `src/MadagascarBrowser/MainWindow.xaml:27-29` (StatusBar : stats blocage)

**Interfaces:**
- Produces: `BrowserServices.EnsureInitialized()`, `BrowserServices.Navigation : NavigationService`, `.AdBlock : AdBlockService`, `.DataSaver : DataSaverService`, `.History : HistoryService`, `.Favorites : FavoritesService`
- Produces: `BrowserStats.Instance.BlockedCount : int` (INPC) — binding StatusBar
- Consomme: `NavigationService.ResolveNavigation(string) : string`, `AdBlockService.ShouldBlock(Uri) : bool`, `.BlockedCount`, `DataSaverService.Evaluate(Uri, string, long) : DataSaverDecision`, `HistoryService.AddVisit(url, title)`, `TabViewModel.View`

- [x] **Step 1:** Écrire `BrowserServices` (init paresseuse : Database `%LocalAppData%\MadagascarBrowser\browser.db`, History, Favorites, Navigation, AdBlock, DataSaver)
- [x] **Step 2:** Écrire `BrowserStats` (singleton INPC, `BlockedCount`)
- [x] **Step 3:** `TabManager.NewTab` : `BrowserServices.EnsureInitialized()`, `t.View = new BrowserView()`, Url = `https://startpage.local/`
- [x] **Step 4:** MainWindow : `Content="{Binding View}"`, StatusBar stats liées à `BrowserStats.Instance.BlockedCount`
- [x] **Step 5:** `BrowserView.xaml.cs` : au `Loaded` → `EnsureCoreWebView2Async` ; échec → message + lien téléchargement WebView2 Runtime ; succès → `AddWebResourceRequestedFilter("*", All)`, hooks `WebResourceRequested` (AdBlock → réponse 403 ; DataSaver → Block=403, Placeholder=GIF 1×1 base64 `R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7`), `NavigationStarting` (IsLoading), `NavigationCompleted` (titre onglet via `DocumentTitle`, `History.AddVisit`, maj `BrowserStats`), `SourceChanged` (maj AddressBar + `TabViewModel.Url`), mapping `startpage.local` + navigation d'accueil si source vide
- [x] **Step 6:** `AddressBar_KeyDown` → `NavigationService.ResolveNavigation(text)` → `WebView.Source`
- [x] **Step 7:** Compiler : `"$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe" build MadagascarBrowser.sln` → 0 erreur

### Task 3 : StartPage locale + page d'erreur

**Files:**
- Create: `src/MadagascarBrowser/StartPage/index.html`
- Create: `src/MadagascarBrowser/StartPage/style.css`
- Create: `src/MadagascarBrowser/StartPage/app.js`
- Create: `src/MadagascarBrowser/StartPage/error.html`
- Modify: `src/MadagascarBrowser/MadagascarBrowser.csproj` (Content CopyToOutputDirectory)

**Interfaces:**
- Consomme: mapping virtuel `https://startpage.local/*` posé en Task 2
- Produits: `index.html` (titre, recherche Google .mg, liens Madagascar, horloge `Indian/Antananarivo`), `error.html?u=<url>` (message français + retour accueil)

- [x] **Step 1:** `index.html` + `style.css` + `app.js` : fond `#0A0D38`, recherche → `https://www.google.mg/search?q=`, grille de liens (Google, Facebook, YouTube, Gmail, mg.wikipedia, WhatsApp Web, journalmadagascar, madagascar-tribune, lexpress, midi-madagascar, tvm, la1ère), horloge/date `Intl` avec `timeZone: 'Indian/Antananarivo'`, mise à jour 1 s
- [x] **Step 2:** `error.html` : message « Page inaccessible », affiche `u` du query string, bouton « Retour à l'accueil »
- [x] **Step 3:** csproj : `<Content Include="StartPage\**\*"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></Content>`
- [x] **Step 4:** `BrowserView` : `NavigationCompleted` échec → `Navigate("https://startpage.local/error.html?u=…")` ; exclure `startpage.local` de l'historique
- [x] **Step 5:** Compiler → 0 erreur

### Task 4 : Suggestions d'historique dans la barre d'adresse

**Files:**
- Modify: `src/MadagascarBrowser/Shell/BrowserView.xaml` (Popup + ListBox)
- Modify: `src/MadagascarBrowser/Shell/BrowserView.xaml.cs` (TextChanged, clic suggestion)

**Interfaces:**
- Consomme: `HistoryService.Search(string, int) : List<Visit>` ; `Visit.Url`, `Visit.Title`
- Produits: popup suggestions (max 8), clic → navigation ; Escape ferme ; Enter = navigation du texte tapé

- [x] **Step 1:** XAML : `Popup` en bas de l'`AddressBar` (Placement=Bottom, StaysOpen=False) + `ListBox` avec template Titre/Url
- [x] **Step 2:** Code : `TextChanged` (≥2 caractères) → `History.Search` → ItemsSource + ouverture si résultats ; clic sur item → `ResolveNavigation`/URL directe → navigation + fermeture
- [x] **Step 3:** Compiler → 0 erreur

### Task 5 : Tests + vérification applicative

- [x] **Step 1:** `"$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe" test MadagascarBrowser.sln` → tous verts
- [x] **Step 2:** Fermer l'instance en cours, rebuild, relancer l'exécutable
- [x] **Step 3:** Vérif UIA : onglet « Page d'accueil »/titre HTML, horloge visible dans l'arbre, navigation URL tape-entrée, suggestion d'historique après 1ère navigation, stats « pubs bloquées » dans la StatusBar

## Hors périmètre (plan suivant)
- OfflineCacheService, ProfileSyncService (spec §2)
- UI Favoris/Historique (menu, panneau)
- Toggle DataSaver dans l'UI
