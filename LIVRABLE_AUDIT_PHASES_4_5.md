# AUDIT — Phases 4 et 5 (état au code, hors travail en cours)

Audit de lecture seule réalisé le 2026-10-23 pour situer l'écart entre l'ordre de
mission et l'état réel du code. Ne déclare **aucune porte close** : les portes 4.x
et 5.x exigent des preuves manuelles qui restent au testeur.

## Phase 4 — Fonctionnalités avancées niveau 2

| Point | État au code | Statut | Impact / contournement |
|---|---|---|---|
| 4.1 Navigation privée | Aucun environnement WebView2 isolé ; `IsPasswordAutosaveEnabled` / `IsGeneralAutofillEnabled` explicitement à `false` | **absent** | Traces (historique/cookies/cache) conservées ; contournement : aucun. Dette portée par le parcours 1.5. |
| 4.2 Effacement granulaire | Historique SQLite effaçable (Bibliothèque, avec confirmation) ; cookies et cache WebView2 non exposés ni effaçables | **partiel** | Cookies/cache non effaçables ; contournement : effacer la base + le profil %LocalAppData% à la main. Dette portée par 1.6. |
| 4.3 Erreurs de certificat | Rejet **explicite** `e.Action = Cancel` (commit `b48f620`) ; page maison via `NavigationCompleted` (`IsBuiltInErrorPageEnabled = false`) ; glyph CertError rouge testé | **fait** (code) | Preuve sur certificat réel (`expired.badssl.com`) à faire à la recette. |
| 4.4 Téléchargements | **En cours hors de ce dépôt committé** (session parallèle) : `DownloadFileNameResolver` (Content-Disposition, UTF-8, anti-traversal) + 4 tests verts ; fix UI-thread du `DownloadOperation` dans l'arbre de travail | **en cours** | Nettoyage des `InProgress` orphelins et « dossier ouvert » non vus — à réclamer avec le commit final. |
| 4.5 Session | Sauvegarde sur fermeture **normale uniquement** (`Closing`) + réglage « Restaurer les onglets » (actuellement `False`) ; **aucun snapshot périodique** | **absent** | Après kill, onglets perdus → parcours 1.10 en dette. Contournement : fermeture normale. |
| 4.6 Onglets | Défilement horizontal **désactivé** (`HorizontalScrollBarVisibility="Disabled"`), favicon absent, indicateur de chargement partiel (LoadBar + `IsLoading`), pas de bouton Stop identifié (Reload seul) | **partiel** | Au-delà de 10-12 onglets, navigation difficile ; contournement : fermer des onglets. |
| 4.7 Autofill / mots de passe | Volontairement désactivé | **non fait, conforme à la consigne** | La consigne n'autorise l'autofill qu'après 4.1 ; 4.1 étant absent, le désactivation est la bonne réponse. Documenté. |

Bug observé (parcour 1.2) non corrigé, rattaché à 4.5 : un onglet MHTML ré-ouvert
affiche le **nom de fichier brut** (`<guid>.mhtml`) au lieu du titre de la page —
les pages MHTML anciennes (sauvegardées avant) avaient un titre correct.

## Phase 5 — Design (spec « Design moderne »)

| Point | État au code | Statut |
|---|---|---|
| 5.1 Chrome unifié | `WindowChrome CaptionHeight="34"` aligné sur les boutons (`CaptionButton` 34 px) ; barre de commande unifiée ; onglets flottants (`TabStripItem`, coins 9 px) ; barre d'état en puces (pubs bloquées, octets, images remplacées, vidéos bloquées, onglets) ; SecurityGlyph unifié via `SecurityIndicatorResolver` du Core (4 états testés) | **conforme au code** |
| 5.2 Vues en cartes + Apparence | Section « Apparence » dans Paramètres (Densité, Accent) ; champs `BrowserSettings` `Theme`/`Density`/`Accent` persistés par `SaveValue` (additif, sans migration de schéma) ; `Theme` sans interface (mode clair hors périmètre) | **conforme au code** ; rendu « cartes » des 3 vues à vérifier à l'exécution |
| 5.3 Start page | `style.css` présent, **26 SVG inline**, chemins relatifs (`href="style.css"`), carte de raccourcis malgaches | **conforme au code** |
| 5.4 Mouvement & a11y | `MotionFast` 120 ms / `MotionMedium` 180 ms / `MotionSlow` 220 ms dans `Tokens.xaml` ; neutralisation via `SystemParameters.ClientAreaAnimation` appliquée avant création de fenêtre ; anneau de focus `FocusRing` ; `AutomationProperties.Name` sur les barres principales (MainWindow 7, BrowserView 13, SettingsView 1) | **conforme au code** ; couverture a11y exhaustive (Bibliothèque, Téléchargements, popups) et **vérification visuelle des 4 états du cadenas** à faire à la recette |

## Portes
Aucune porte n'est déclarée close par cet audit. Ce qui manque pour les fermer :
- 4.4 : le commit du travail en cours (session parallèle) + ses points non vus (orphelins, dossier).
- 4.x manquants (4.1, 4.2, 4.5, 4.6) : décision produit de les réaliser ou de les porter en dette finale.
- 5.x : vérification visuelle (rendu cartes, cadenas, animations) par le testeur.
