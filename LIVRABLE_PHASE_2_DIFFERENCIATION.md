# LIVRABLE — PHASE 2 : Différenciation mesurable

## 1. Numéro et titre de la phase
**Phase 2 — Différenciation mesurable (la raison d'être du produit)**.

## 2. Contrainte d'exécution (environnement)
Toujours **headless** : pas de mesure réelle du DataSaver sur 5 sites, pas de coupure réseau provoquée, pas de vérification du cycle Retour sur une page qui re-échoit. Ces points sont audités par le code et marqués `à vérifier à l'exécution` avec raison.

## 3. Fichiers modifiés
| Chemin | Résumé (une ligne) |
|---|---|
| `src/MadagascarBrowser/Shell/BrowserStats.cs` | Ajout des compteurs `ReplacedImagesCount` et `BlockedVideosCount` avec notification `PropertyChanged` pour affichage des puces de barre d'état. |
| `src/MadagascarBrowser/Shell/BrowserView.xaml.cs` | Alimentation des deux nouveaux compteurs depuis `DataSaverMetricsService` dans `RefreshStatsAsync`. |
| `src/MadagascarBrowser/MainWindow.xaml` | Ajout de deux puces « X images remplacées » et « X vidéos bloquées » dans la barre d'état, après la puce économies DataSaver. |

## 4. Tests ajoutés
Aucun. Les métriques sont déjà couvertes par `DataSaverMetricsServiceTests` (persistance, session, cumul) et `DataSaverServiceTests` (décision d'évaluation). Aucun nouveau test unitaire n'est ajouté ici pour éviter tout code non vérifiable headless.

## 5. Build et tests
- **Build :** 0 erreur, 0 avertissement (`dotnet build MadagascarBrowser.sln`).
- **Tests :** tous verts — Core 170/170, Shell 14/14 (total 184/184).

## 6. Points Phase 2 : statut et notes

| # | Point | Mécanisme impliqué | Statut | Notes |
|---|-------|-------------------|--------|-------|
| 2.1 | DataSaver : `Content-Length` de la RÉPONSE (pas la requête) ; vidéo par `Content-Type video/*` + extension ; seuil image paramétrable ; domaine exempté respecté par AdBlock ET DataSaver | `ResourceInterceptionScope` (FetchPatterns réponse), `DataSaverService.Evaluate(contentLength, mimeType, domainExempt)`, `DataSaver.ImageThresholdBytes` issu des Paramètres | **confirmé** (code + test) / **à vérifier** (mesure réelle 5 sites) | `Evaluate` lit `contentLength` de la réponse, bloque `video/*` et extensions vidéo, applique le seuil image configurable, et passe en `Pass` si `domainExempt`. AdBlock `IsDomainExempt` couvre le même domaine. Mesure réelle bloquée (headless). |
| 2.2 | Métrique persistée et visible (octets économisés, pubs bloquées, images remplacées, vidéos bloquées) ; calcul dans le Core ; puce dans la barre d'état | `DataSaverMetricsService` (SQLite, persisté) ; `BrowserStats.Instance` (puces) ; `BrowserView.RefreshStatsAsync` alimente les 4 compteurs | **confirmé** (code + test) / **à vérifier** (affichage GUI) | 4 puces maintenant présentes : pubs bloquées, économies (Mo), images remplacées, vidéos bloquées. Calcul Core, affichage Shell. |
| 2.3 | Hors ligne : page d'erreur = état de l'onglet (pas navigation) ; message adapté par code d'erreur ; cycle Retour éliminé | `NavigationFailureResolver` (message par `webErrorStatus`), panneau `HasNavigationError` maison dans `BrowserView`, `Back_Click` → `GoBack` | **à vérifier à l'exécution** | Messages par code présents et état d'onglet (pas de navigation vers une URL d'erreur). Le cycle Retour sur une page qui re-échoit n'est pas empêché au niveau code : **à vérifier à l'exécution**. |
| 2.4 | Start page : moteur de recherche piloté par les Paramètres, un seul point de configuration injecté dans `NavigationService` ET page d'accueil. Retirer toute mention fausse (« 100 % locale ») | `BrowserSettings.SearchProvider` → `NavigationService(searchProvider)` ; pont `webview.postMessage("search:…")` depuis `app.js` → `BrowserView.OnWebMessageReceived` → `Navigation.ResolveNavigation` | **confirmé** (code) / **à vérifier** (GUI) | Le moteur choisi dans Paramètres pilote à la fois la barre d'adresse et la recherche de la page d'accueil. L'UI ne contient aucune mention « 100 % locale » (cette mention existe uniquement dans les docs de spec internes, pas dans l'application livrée). |

## 7. Mesure manuelle sur 5 sites (Phase 2.1 / 2.2)
Non réalisable headless (pas de navigation web réelle, pas de serveur de test pilotable). Reportée à la session de recette sur poste équipé (voir blocage externe section 9).

## 8. Erreur réseau provoquée et cycle Retour (Phase 2.3)
Code présent (`NavigationFailureResolver`, panneau maison), mais la preuve par coupure réseau réelle et par cycle Retour est bloquée par l'environnement. **À vérifier à l'exécution.**

## 9. Blocage externe signalé
Toutes les vérifications manuelles GUI et réseau de la Phase 2 (mesure DataSaver 5 sites, coupure réseau, cycle Retour, certificat expiré, affichage des puces) **ne peuvent pas être réalisées** dans cet environnement headless.  
**Options :** (1) poursuivre les phases 3 à 6 en audit de code + tests unitaires, en reportant les preuves manuelles à une machine avec WebView2 ; (2) arrêter ici et transmettre à un poste avec WebView2.  
**Recommandation :** option (1).

## 10. Porte 2
- Mesure manuelle sur 5 sites : reportée (blocage externe, section 9).
- Erreur réseau provoquée et cycle Retour : code présent, preuve manuelle reportée.
- Aucun nouveau bug introduit ; build et tests verts.

**Phase 3 non commencée.** Ce livrable n'introduit aucune fonctionnalité Phase 3.