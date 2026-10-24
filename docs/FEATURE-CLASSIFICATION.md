# Classification des fonctionnalités : dette, plateforme ou hors périmètre

Règle de lecture :

- **(A) Dette produit** : manque de Madagascar Browser, corrigeable dans ce dépôt.
- **(B) Limite de plateforme** : imposée par WebView2/Chromium/Windows ; le produit
  ne peut pas la lever sans réimplémenter le moteur, ce qui est exclu.
- **(C) Hors périmètre niche** : fonctionnalité volontairement absente du produit
  (compte, synchronisation, écosystème d'extensions).

Une (A) n'est jamais requalifiée en (B) pour justifier un report. Les preuves
renvoient au code, aux tests et aux captures.

| # | Fonctionnalité | Classe | Décision | Preuve |
|---|---|---|---|---|
| D1 | Navigation privée | (A) | **Levée** : profil `InPrivate`, badge, pas de session/historique | `TabManager`, `BrowserView.EnsureCoreWebView2Async`, `TabManagerPrivateTests`, E2E `PrivateWindow_IsExplicitlyMarkedAndOpenedFromMenu` |
| D2 | Effacement cookies/cache/historique | (A) | **Levée** : dialogue granulaire, `ClearBrowsingDataAsync`, historique SQLite effacé | `ClearBrowsingDataWindow`, `BrowsingDataSelectionTests`, E2E `ClearBrowsingData_DialogClearsSelectedCategories` |
| D3 | `Content-Disposition` / nom de téléchargement | (A) | **Levée** : `DownloadStarting`, `filename`/`filename*`, RFC 5987, extensions doubles | `DownloadFileNameResolver`, `DownloadFileNameResolverTests`, E2E `Download_ContentDispositionFileNameIsPreserved` |
| D4 | Cycle Retour / page en erreur | (A) | **Levée** : `GoBack`/`CanGoBack`, page d'erreur = état d'onglet | `NavigationService`, `NavigationServiceTests`, E2E `BackAndForward_NavigateThroughHistoryWithoutStaleContent` |
| D5 | Outils de développement | (A) | **Exposés en Debug**, absents du build public | `DevToolsPolicy`, `BuildInfo`, `DevToolsPolicyTests`, E2E `DevTools_ExposedOnlyInDebugBuild` |
| D6 | Runtime WebView2 obsolète | (A) | **Levée** : version réelle + bandeau si Fixed Version < 130.0.0.0 | `WebViewRuntimeInfo`, `WebViewRuntimeService`, `WebViewRuntimeInfoTests`, E2E `AboutWindow_ReportsRealRuntimeInsteadOfPretending` |
| D7 | Autofill / mots de passe | (A partielle) | **Opt-in local**, désactivé par défaut ; support partiel selon le moteur | `BrowserSettings`, `BrowserSettingsServiceTests`, `SettingsView` |
| D8 | Mise à jour automatique de l'application | (A) | **Dette ouverte** : distribution manuelle, pas encore Velopack/MSIX | Absence d'updater dans `src/` |
| D9 | Mise à jour des listes de blocage | (A) | **Dette ouverte** : listes embarquées, pas de canal de mise à jour | `AdBlockService` |
| B1 | TLS, réseau, JavaScript, CSS, rendu | (B) | Assumé : moteur Microsoft WebView2, jamais réimplémenté | Architecture, aucune couche réseau propre |
| B2 | Traduction de pages | (B) | Non exposé par l'API WebView2 ; hors périmètre niche | Pas d'API publique de traduction |
| B3 | Galerie d'extensions | (B) | WebView2 n'expose pas la galerie Chrome/Edge | Pas de gestion d'extensions |
| B4 | Mises à jour de sécurité du moteur | (B) | Evergreen mis à jour par Microsoft ; Fixed Version signalé s'il est ancien | `WebViewRuntimeInfoTests` |
| C1 | Compte et synchronisation | (C) | Refusé : le produit ne crée aucun compte, ne synchronise rien | Aucun service de compte |
| C2 | Écosystème d'extensions | (C) | Refusé : hors du besoin DataSaver/3G | Aucun store |
| C3 | Moteur de rendu maison | (C) | Refusé : réimplémenter Chromium n'apporte rien au niche | Choix d'architecture |

## Ce que la recette GUI prouve réellement

- Les dettes D1–D4 et D6 sont exercées sur l'exécutable réel, pas seulement en
  unitaire.
- D5 est prouvée en Debug ; le build public cache le menu et ignore F12.
- D7 est couvert par les tests de persistance ; la bascule GUI est visible dans
  Paramètres (cases opt-in).
- D8 et D9 restent explicitement ouvertes : elles ne sont pas maquillées en
  limites de plateforme.

## Points non revendiqués

- La traduction n'est pas simulée.
- Aucun proxy TLS maison, aucun moteur réseau parallèle.
- Aucun « mode économie » fictif : les compteurs viennent des réponses réellement
  interceptées.
