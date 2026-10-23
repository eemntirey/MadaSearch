# Livrable — Madagascar Browser vers l'état livrable

Mission en phases (ordre imposé). Chaque phase : fichiers modifiés, tests ajoutés,
build, tests, vérifications manuelles, statut par point, et confirmation explicite
que la phase suivante n'est pas commencée.

Définition de « livré » : voir l'ordre de mission (6 critères opposables).
« Zéro bug » = zéro bug **connu** au moment de livrer, tout bug connu restant
listé dans la section Dette avec impact et contournement.

Banc de vérification manuelle utilisé pendant toute la mission :
- Application lancée depuis `src/MadagascarBrowser/bin/Debug/net8.0-windows/`.
- Pilotage par l'arbre d'accessibilité (UI Automation) : lecture des titres d'onglets,
  de l'indicateur de sécurité, des puces de barre d'état, de la barre d'adresse ;
  clics et raccourcis clavier injectés sur la fenêtre.
- Serveur HTTP local déterministe (pages de test, `target=_blank`, `window.open`,
  téléchargement `Content-Disposition`, 404/500, images DataSaver).
- Journal applicatif : `%LocalAppData%\MadagascarBrowser\logs\app.log`.

---

## PHASE 0 — VERROUILLAGE DE L'ÉTAT RÉEL

### 0.1 Les 5 blockers de la phase 0 précédente

| Blocker | Preuve par test | Vérification manuelle | Statut |
|---|---|---|---|
| userDataFolder | `BrowserPathsTests`, profil unifié sur `BrowserPaths.Default.WebViewProfilePath` (commit ca31727) | Après lancement : `EBWebView` absent de `bin\Debug\net8.0-windows`, profil présent dans `%LocalAppData%\MadagascarBrowser\WebView2\EBWebView`, données (browser.db, logs, offline) dans `%LocalAppData%\MadagascarBrowser\` | **confirmé** |
| Cadenas dynamique | `ConnectionSecurityTests` (7 cas), `SecurityIndicatorTests` (3 cas) | Navigation réelle : HTTPS → « Connexion chiffrée (HTTPS) » ; HTTP (127.0.0.1:8777) → « Connexion non chiffrée (HTTP) » ; `file://` (MHTML) → « Aucune connexion réseau ». État « certificat expiré » à vérifier au parcour 1.9 | **confirmé** (états HTTPS/HTTP/fichier) ; état certificat **à vérifier à l'exécution** (parcour 1.9) |
| NewWindowRequested | `NewWindowRequestTests` (Core), `NewWindowRequestHandlerTests` (8 cas Shell) | Clic réel sur lien `target="_blank"` de la page de test → nouvel onglet de l'application créé et activé (« Nouvel onglet cible », URL correcte) | **confirmé** |
| LooksLikeUrl + MHTML | `NavigationServiceTests` (4 cas LooksLikeUrl), `OfflinePageServiceTests` | Saisie `example.com` sans schéma → `https://example.com/` rendue ; saisie du chemin absolu `…\offline\….mhtml` → `file:///…mhtml` rendu hors ligne, glyph « Aucune connexion réseau » | **confirmé** |
| SQLite hors thread UI | `SqliteRobustnessTests`, `DatabaseAccess.WriteAsync` | Navigation enchaînée + écritures d'historique simultanées (56 lignes, dernières = navigations du test) : interface réactive, aucun verrou ni exception SQLite dans le journal | **confirmé** |

### 0.2 Couverture des 14 tests Shell (répartition réelle)

Couvert (3 fichiers, 14 tests) :
- `NewWindowRequestHandlerTests` (8) : ouverture d'onglet sur URI réelle, URI vide → onglet vide,
  échec d'ouverture signalé (non avalé), refus d'hôte nul — avec un `RecordingTabHost` (adaptateur).
- `TabSelectionTests` (5) : logique `NextIndex` de sélection d'onglet après fermeture.
- `AppLogTests` (1) : écriture du journal fichier local (rotation/dossier).

**Non couvert** : `BrowserView` (code-behind : recherche, zoom, raccourcis, téléchargements,
erreurs de certificat, interception DataSaver), `TabManager` (NewTab/Close/Execute),
`MainWindow` (accélérateurs fenêtre), `WebViewEnvironment`, `Downloads`. Ces zones ne sont
vérifiables que manuellement : c'est exactement le trou que la phase 1 comble.

### 0.3 État Git

- Dépôt propre après mission : les modifications non committées présentes au démarrage
  (travail Shell de la phase 3, journal fichier 6.2, correctifs) ont été relues intégralement,
  jugées cohérentes, et découpées en 4 commits :
  `4fb6eca` (course d'annulation AsyncDebouncer), `ca31727` (profil unifié),
  `9dbccb5` (journal fichier 6.2), `3d00ffa` (recherche/zoom/impression/plein écran/raccourcis, phase 3).
- Chaîne de commits récente lue : phases 2.1→2.4 et 3 (core) déjà en historique (`8a9955d`…).
  L'état du dépôt est **connu et compris** → pas de STOP.

### 0.4 État réel vs annoncé — écarts

| # | Annoncé | État réel observé | Traitement |
|---|---|---|---|
| É1 | 1 avertissement de nullabilité | **2 avertissements distincts** en build complet : `CS8622` (`MainWindow.OnSourceInitialized`), `CS8767` (`RecordingTabHost.NewTab` dans les tests) | Dette planifiée phase 6.1 |
| É2 | « aucun bug connu » supposé après phase 0 | **Exception récurrente détectée au journal** : `KeyNotFoundException` en `BrowserView.OnFetchRequestPaused` ligne 741 (`root.GetProperty("responseHeaders")` absent du payload DevTools) — 4 occurrences dès la première navigation, interceptée et loggée (non silencieuse), mais interception DataSaver en erreur sur certaines réponses | **Correctif phase 1** (parcour 1.12 / 2.1) |
| É3 | 180 tests verts = fonctionnel | Confirmé : 166 Core + 14 Shell verts, **mais aucun parcours manuel documenté** avant cette mission | Comblé par les phases 1 à 4 |
| É4 | Travail phase 3 / 6.2 « supposé » committé | Était **non committé** au démarrage (13 fichiers modifiés + 1 test) | Committé en 4 commits (0.3) |

### Build / tests (constat de porte 0)

- Build : **0 erreur** (oui). Avertissements : 2 distincts (voir É1).
- Tests : **180/180 verts** — Core 166/166, Shell 14/14.

### Vérifications manuelles (réalisées pour la porte 0)

1. Lancement, profil, données en `%LocalAppData%` — **pass**.
2. Glyph de sécurité HTTPS / HTTP / fichier — **pass**.
3. `target="_blank"` → nouvel onglet applicatif — **pass**.
4. `example.com` sans schéma + ouverture d'un `.mhtml` absolu — **pass**.
5. Réactivité SQLite sous charge de navigation — **pass**.
6. Journal fichier présent, exceptions visibles (É2) — **pass** (le log prouve son utilité).

### Statuts des points 0.1 → 0.4 : « confirmé », « à vérifier à l'exécution » (état certificat,
parcours complets), « en dette » (É1 → 6.1).

**Confirmation explicite : la phase 1 n'était PAS commencée à la clôture de cette section ;
elle démarre au paragraphe suivant.**
