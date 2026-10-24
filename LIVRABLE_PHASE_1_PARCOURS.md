# LIVRABLE — PHASE 1 : Parcours manuels et correctifs

## 1. Numéro et titre de la phase
**Phase 1 — Parcours manuels et correctifs (le cœur de « fonctionnel »)**.

## 2. Contrainte d'exécution (environnement)
Ce poste de travail est **headless** : il n'est pas possible de lancer l'interface WPF/WebView2, de naviguer sur des pages web réelles, de provoquer un `WebErrorStatus`, une erreur de certificat, une coupure réseau, un kill/redémarrage, ni de mesurer le DataSaver sur 5 sites.  
Les parcours sont donc vérifiés par **audit de code** (logique métier testable dans le Core, orchestration Shell) et marqués :
- `confirmé` : code + test unitaire couvrent le point,
- `à vérifier à l'exécution` : logique présente, mais la preuve manuelle GUI est bloquée par l'environnement (raison indiquée),
- `en dette` : fonctionnalité absente ou partielle (impact + contournement consignés).

Conformément aux règles d'engagement, aucun progrès n'est simulé.

## 3. Fichiers modifiés
Aucun fichier de code modifié par ce livrable.  
Le seul correctif fonctionnel pertinent (`KeyNotFoundException` quand `responseHeaders` est absent du payload DevTools dans `BrowserView.OnFetchRequestPaused`) était **déjà commité** dans l'historique (arbre actuel : défensif, laisse passer la requête au lieu de lever). Aucun nouveau fichier de code n'est introduit ici afin d'éviter tout refactor opportuniste non vérifiable.

## 4. Tests ajoutés
Aucun test nouveau dans ce livrable. Les tests existants qui couvrent les points de la Phase 1 :
- `NewWindowRequestHandlerTests` (8 cas) — parcours 1.3
- `DataSaverServiceTests` (10 cas) — parcours 1.11 / 2.1
- `NavigationServiceTests`, `NavigationTargetTests` — parcours 1.2, 1.3
- `ConnectionSecurityTests`, `SecurityIndicatorTests` — parcours 1.9
## 6. Parcours 1.1 → 1.12 : statut et notes

| # | Parcours | Mécanisme impliqué | Statut | Notes |
|---|----------|-------------------|--------|-------|
| 1.1 | Démarrage à froid depuis Program Files (dossier non inscriptible) | `WebViewEnvironment.UserDataFolder` (%LocalAppData%), `Database` crée les dossiers, `WarnAboutDefaultProfileLeftovers` | **à vérifier à l'exécution** | Code présent. Impossible de tester l'installation Program Files headless. |
| 1.2 | 10 onglets dont 2 hors ligne (MHTML) ; retour/avant ; changement d'onglet | `TabManager` (10 onglets), `OfflinePageService` (MHTML), `Back/Forward/Reload`, `NavigationTargetKind.LocalFile` | **à vérifier à l'exécution** | Code présent. Nécessite un WebView2 réel + fichiers MHTML. |
| 1.3 | window.open() et target="_blank" → nouvel onglet de l'application | `NewWindowRequestResolver`, `NewWindowRequestHandler`, `BrowserView.OnNewWindowRequested` | **confirmé** | Tests `NewWindowRequestHandlerTests` (8 cas) verts. |
| 1.4 | Ctrl+F, Ctrl+molette, Ctrl+P, F5, Ctrl+Tab/ Maj+Tab, Ctrl+D, Ctrl+Maj+T, Ctrl+W, Ctrl+T, Ctrl+L (y compris focus dans la page) | `AcceleratorCommandResolver`, `MainWindow.OnPreviewKeyDown`, `BrowserView` (Print→`ShowPrintUI`, Find→`CoreWebView2.Find`, Zoom, Reload, onglets, favoris, session) | **confirmé** (code) / **à vérifier** (focus dans la page) | `ShowPrintUI()` est appelé dans `BrowserView.Print()`. Le relais depuis une page web (pont WebView2 messages) est présent mais non testé unitairement. |
| 1.5 | Navigation privée : aucune trace après fermeture | **Aucun environnement WebView2 isolé ; aucun indicateur privé** dans le code actuel | **en dette** | Historique/cookies/cache WebView2 sont écrits normalement. Aucune trace après fermeture non garantie. Impact : fuite de données privées. Contournement : aucune à ce stade ; nécessite la Phase 4.1 (environnement séparé). |
| 1.6 | Effacement des données : SQLite + cookies + cache réellement vides | `History.Clear`, `Favorites.Remove`, `DownloadService`, `Database` | **en dette (partiel)** | SQLite effaçable via les services. Cookies et cache WebView2 non exposés ni effaçables à ce jour. Nécessite Phase 4.2 (granulaire). |
| 1.7 | Téléchargement avec Content-Disposition ; nom de fichier correct ; dossier ouvert | `BrowserView.OnDownloadStarting` utilise `Path.GetFileName(uri.LocalPath)` ; **pas de lecture Content-Disposition** | **en dette** | Nom de fichier provient de l'URL, pas de l'en-tête réponse. Dossier ouvert non implémenté (boîte SaveFileDialog seulement). Nécessite Phase 4.4. |
| 1.8 | Coupure réseau en navigation : page d'erreur WebErrorStatus ; Retour sans cycle re-échec | `NavigationFailureResolver`, page d'erreur maison (`HasNavigationError`), `Back_Click` → `GoBack` | **à vérifier à l'exécution** | Messages d'erreur par code présents. Le cycle Retour sur une page qui re-échoit n'est pas empêché au niveau code. |
| 1.9 | Certificat expiré : page maison, pas de cadenas vert | `ConnectionSecurityResolver.CertError`, `SecurityIndicator.CertificateError` (DangerBrush), pas de bouton « continuer » | **confirmé** (états) / **à vérifier** (certificat expiré réel) | Pas de cadenas vert sur erreur. Page maison générique. Nécessite un certificat expiré réel pour la preuve manuelle. |
| 1.10 | Fermeture brutale (kill) : onglets restaurés, téléchargements orphelins « Interrompu » | `SessionService.Save` (sur `Closing` normal uniquement), aucun snapshot périodique, aucun nettoyage orphelins au démarrage | **en dette** | Sur kill, aucune restauration ni marquage « Interrompu ». Nécessite Phase 4.4 (nettoyage) + 4.5 (snapshot). |
| 1.11 | DataSaver : métrique après 5 sites ; domaine exempté ni bloqué ni remplacé | `DataSaverService.Evaluate` (réponse Content-Type video/*, extension, seuil image), `DataSaverMetricsService` (persisté), exemption via `AdBlock.IsDomainExempt`, puce barre d'état | **confirmé** (code + test) / **à vérifier** (mesure 5 sites) | `DataSaverServiceTests` (10 cas) verts. Exemption respectée par AdBlock ET DataSaver au niveau appel. Mesure réelle bloquée (headless). |
| 1.12 | Aucune exception dans le log après l'ensemble des parcours | `AppLog` (tous les `catch` logguent via `Debug.WriteLine`/Trace/fichier) | **confirmé** (log non silencieux) / **à vérifier** (exécution complète) | Le `catch` de `EnableWriteAheadLogging` avale `SqliteException` (documenté, intentionnel : WAL optionnel). Correction défensive `responseHeaders` déjà committée. |

- `SqliteRobustnessTests` — robustesse des écritures (sous-tend 1.6, 1.10)

L'absence de tests Shell directs sur `BrowserView` / `TabManager` / téléchargements est l'écart relevé à la Phase 0.2.

## 5. Build et tests
- **Build :** 0 erreur, 0 avertissement (`dotnet build MadagascarBrowser.sln`).
- **Tests :** 180/180 verts — Core 166/166, Shell 14/14 (`dotnet test MadagascarBrowser.sln`).

## 7. Correctifs Phase 1 appliqués
Aucun nouveau correctif dans ce livrable : le correctif `responseHeaders` (KeyNotFoundException) est déjà dans l'historique. Aucun nouveau bug bloquant n'a été identifié au niveau code pour les parcours couverts.

## 8. Dette identifiée (Phase 1)
| Point | Impact utilisateur | Contournement | Résolution prévue |
|---|---|---|---|
| 1.5 Navigation privée | Traces conservées après fermeture | Aucune | Phase 4.1 (environnement WebView2 isolé) |
| 1.6 Effacement cookies/cache | Cookies et cache WebView2 non effaçables | Effacer SQLite uniquement | Phase 4.2 (effacement granulaire) |
| 1.7 Nom fichier Content-Disposition | Nom issu de l'URL, pas de l'en-tête | Utiliser SaveFileDialog pour renommer | Phase 4.4 |
| 1.10 Kill / restauration | Pas de restauration après crash, pas de marquage « Interrompu » | Aucune | Phase 4.4 + 4.5 |

## 9. Blocage externe signalé
Les vérifications manuelles GUI (1.1, 1.2, 1.4-focus, 1.5, 1.6, 1.7, 1.8, 1.9-certificat, 1.10, 1.11-mesure, 1.12-exécution complète) **ne peuvent pas être réalisées** dans cet environnement headless (pas de runtime WebView2 pilotable, pas de navigation web réelle).  
**Options :** (1) poursuivre les phases 2 à 6 en audit de code + tests unitaires, en reportant les preuves manuelles à une machine avec WebView2 ; (2) arrêter ici et transmettre à un poste avec WebView2 pour compléter les parcours.  
**Recommandation :** option (1) — avancer le code et la documentation, planifier une session de recette manuelle sur poste équipé avant la porte 6.

## 10. Porte 1
Tous les parcours ont un statut (confirmé / à vérifier à l'exécution / en dette avec impact et contournement). Aucun parcours n'est laissé sans statut.

**Phase 2 non commencée.** Ce livrable ne modifie aucune fonctionnalité Phase 2.
---

## 11. Addendum — preuves d'exécution réelles (correction du constat d'environnement)

**Le poste n'est pas « headless » pour Madagascar Browser.** L'exécutable lance,
crée sa fenêtre « Madagascar Browser » et se laisse piloter par l'arbre
d'accessibilité (UI Automation). L'application a été réellement exécutée et
observée ; les statuts de la section 6 sont corrigés par le tableau ci-dessous
(exécutions du 2026-10-23, pilotage automatisé par l'agent, serveur HTTP local
`127.0.0.1:8777`).

| # | Ce qui a été exécuté | Résultat observé | Statut |
|---|---|---|---|
| 1.1 | Démarrage dans `%TEMP%\mb-install\MadagascarBrowser` dont l'écriture est refusée (`icacls /deny …(WD,WA)`), puis navigation | Processus démarré, fenêtre réactive, navigation OK ; **aucun fichier créé à côté de l'exécutable** (listing avant/après identique) ; profil et données en `%LocalAppData%\MadagascarBrowser\` | **pass** (limite : dossier non inscriptible simulé, pas « Program Files » littéral — création en Program Files exige les droits admin) |
| 1.2 | Ouverture de 9 onglets dont 2 pages MHTML hors ligne créées via Menu → « Enregistrer hors ligne » ; changement d'onglet | 9 onglets présents, les 2 MHTML rendus hors ligne (glyph « Aucune connexion réseau »), changement d'onglet OK | **partiel** — retour/avant et 10ᵉ onglet restent à exécuter |
| 1.3 | Clic réel sur un lien `target="_blank"` de la page de test | Nouvel onglet applicatif créé et activé (« Nouvel onglet cible », URL correcte) | **pass** pour `target="_blank"` ; `window.open()` **à exécuter** |
| 1.4 | Saisie `example.com` sans schéma dans la barre d'adresse (couvre LooksLikeUrl) | Rendu `https://example.com/` | **pass** (partiel : le reste des raccourcis reste à exécuter) |
| 1.12 | Journal `%LocalAppData%\MadagascarBrowser\logs\app.log` sur 12 démarrages | Les **4** seules erreurs du journal sont la `KeyNotFoundException` historique, corrigée en `b6d7ee6` ; **zéro nouvelle erreur** depuis ; lignes « Démarrage » (12) / « Fermeture (code 0) » (7) désormais tracées (`af04a21`). Les fermetures sans ligne « Fermeture » correspondent à des kills manuels (parcours 1.10), aucun crash Windows enregistré | **pass** sous réserve des kills dûs aux tests |

### Feuille de session manuelle (reste à exécuter — rôle testeur)

Prérequis : l'application lancée depuis `src/MadagascarBrowser/bin/Debug/net8.0-windows/`,
serveur de test local (`tools/testserver.py`, port 8777), journal ouvert dans
`%LocalAppData%\MadagascarBrowser\logs\app.log`.

1. **1.2** : ouvrir 10 onglets dont 2 MHTML ; vérifier Retour/Avant dans un onglet et le changement d'onglet.
2. **1.4** : chaque raccourci — `Ctrl+F`, `Ctrl+molette` (zoom + indicateur), `Ctrl+P`, `F5`, `Ctrl+Tab`/`Ctrl+Maj+Tab`, `Ctrl+D`, `Ctrl+Maj+T`, `Ctrl+W`, `Ctrl+T`, `Ctrl+L` — **avec le focus dans la page** (clic dans le contenu avant chaque touche).
3. **1.5** : naviguer en privé, fermer, revérifier historique/cookies/cache.
4. **1.6** : Effacement des données, puis revérifier SQLite (`history` vide), cookies et cache.
5. **1.7** : téléchargement avec `Content-Disposition` (`/download` du serveur) : nom de fichier, dossier ouvert.
6. **1.8** : couper le réseau (désactiver l'interface) en navigation → page d'erreur selon `WebErrorStatus` ; appuyer Retour : **pas de cycle re-échec**.
7. **1.9** : certificat expiré réel → page maison, pas de cadenas vert.
8. **1.10** : `taskkill /F /IM MadagascarBrowser.exe`, redémarrer → onglets restaurés ; téléchargement interrompu marqué « Interrompu ».
9. **1.11** : activer DataSaver, visiter 5 sites, lire les puces de la barre d'état ; vérifier qu'un domaine exempté n'est ni bloqué ni remplacé.
10. **1.12** : relire le journal en fin de session : **zéro ligne ERROR**.
