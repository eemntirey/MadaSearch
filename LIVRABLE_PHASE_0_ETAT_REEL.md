# LIVRABLE — PHASE 0 : Verrouillage de l'état réel

## 0. Numéro et titre de la phase
**Phase 0 — Verrouillage de l'état réel** (avant toute nouveauté).

## 1. Fichiers modifiés
Aucun. Cette phase est une phase de vérification et de constat, pas de modification de code.

## 2. Tests ajoutés
Aucun. Les tests de preuve existent déjà dans le dépôt (voir section 4).

## 3. Build et tests
- **Build :** 0 erreur, 0 avertissement (confirmé par `dotnet build MadagascarBrowser.sln`).
- **Tests :** 180/180 verts
  - Core (`MadagascarBrowser.Tests.dll`) : 166/166 verts.
  - Shell (`MadagascarBrowser.Shell.Tests.dll`) : 14/14 verts.

## 4. Vérifications manuelles (état réel des 5 bloquants Phase 0)

| Bloquant | Preuve par code | Preuve par test | Statut |
|---|---|---|---|
| **userDataFolder** (profil WebView2 hors Program Files) | `WebViewEnvironment.UserDataFolder` = `BrowserPaths.Default.WebViewProfilePath` (%LocalAppData%\\MadagascarBrowser\\WebView2) ; `WarnAboutDefaultProfileLeftovers` signale les profils par défaut restés à côté de l'exe | `BrowserPathsTests` (WebViewProfilePath, FindDefaultProfileLeftovers) | Confirmé (code + test). Vérification manuelle d'installation dans Program Files **à vérifier à l'exécution** (headless). |
| **Cadenas dynamique** | `SecurityIndicator` (None/Secure/Insecure/CertificateError) + `SecurityIndicatorResolver` ; `TabViewModel.SecurityState` pilote le glyphe | `ConnectionSecurityTests` + `SecurityIndicatorTests` | Confirmé (code + test). |
| **NewWindowRequested** → nouvel onglet de l'application | `NewWindowRequestResolver` + `NewWindowRequestHandler` ; `BrowserView.OnNewWindowRequested` raccorde l'événement WebView2 | `NewWindowRequestTests` + `NewWindowRequestHandlerTests` | Confirmé (code + test). |
| **LooksLikeUrl + MHTML** | `NavigationService.LooksLikeUrl` ; `NavigationTargetResolver` (LocalFile pour .mhtml / chemin local) | `NavigationServiceTests` + `NavigationTargetTests` | Confirmé (code + test). |
| **SQLite hors thread UI** | `DatabaseAccess.ReadAsync/WriteAsync` (Task.Run) ; `SqliteRetryPolicy` ; WAL + busy_timeout dans `Database` | `SqliteRobustnessTests` (réentrance, verrous, busy_timeout) | Confirmé (code + test). |

## 5. État de la branche et du commit
- Branche : `feat/design-moderne`
- Commit HEAD : `3d00ffa` ("feat(shell): recherche, zoom, impression, plein écran et raccourcis (phase 3)"), successeur de `8a92218`.
- Working tree : propre (aucune modification non commitée).
- Le dépôt est dans un état connu et compris : **PAS d'état inconnu → pas de STOP**.

## 6. Écart avec ce qui est annoncé (Phase 0.2 — couverture Shell)
Les 14 tests Shell couvrent :
- `NewWindowRequestHandler` (gestion fenêtre popup),
- `AppLog` (journal fichier),
- `TabSelection` (choix d'onglet après fermeture).

Ils ne couvrent pas directement : `BrowserView` (navigation, téléchargement, certificat, recherche dans la page), `TabManager` (cycle de vie des onglets, restauration de session), `DownloadsView`, ni l'initialisation `BrowserServices.EnsureInitialized`.

**Statut :** les tests Shell couvrent des adaptateurs/gestionnaires, pas le Shell de navigation lui-même. Cet écart est consigné ; il n'est pas bloquant pour la porte 0 mais figure comme « à vérifier à l'exécution » pour les parcours 1.x.

## 7. Statut par point (Phase 0)
- 0.1 Bloquants clos : **confirmé** (preuves code + test ci-dessus).
- 0.2 Couverture des 14 tests Shell : **à vérifier à l'exécution** (écart listé ci-dessus ; pas de couverture directe de BrowserView/TabManager/Downloads).
- 0.3 Commit attendu présent : **confirmé** (HEAD `3d00ffa`, tree compris).
- 0.4 Consignation état réel : **ce document**.

## 8. Porte 0
- Build vert : oui.
- Tests verts : 180/180 (166 Core + 14 Shell).
- État Git propre et compris : oui.
- Liste des écarts écrite : oui (section 6).

**Phase 1 non commencée.** La porte 0 est franchie ; aucune modification de fonctionnalité Phase 1 n'est introduite par ce livrable.