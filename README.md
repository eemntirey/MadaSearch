# Madagascar Browser

Navigateur de niche pour Madagascar, construit sur **.NET 8 / WPF** et le moteur
**Microsoft WebView2**. Objectif : consommer moins de données sur réseau mobile
(3G/4G), bloquer la publicité localement et rester utilisable sans compte.

Ce dépôt contient l'application, ses tests unitaires et une suite de recette GUI
(FlaUI) qui pilote réellement l'exécutable.

## Ce que fait le produit

- **DataSaver mesuré** : décision sur le `Content-Length` de la réponse, seuil
  image réglable, blocage vidéo, exceptions par domaine.
- **Blocage publicitaire local** : aucune liste envoyée à un serveur, aucun compte.
- **Navigation privée** : profil WebView2 `InPrivate`, badge explicite, aucune
  écriture de session ni d'historique SQLite.
- **Effacement granulaire** : cookies, cache, historique, stockage local et mots
  de passe via `ClearBrowsingDataAsync`.
- **Téléchargements** : le nom de fichier vient de `Content-Disposition`
  (`filename*` RFC 5987 décodé), pas de l'URL.
- **Retour / suivant fiables** : le cycle d'historique ne rejoue pas une page
  d'erreur.
- **Autofill et mots de passe locaux, opt-in** : désactivés par défaut, jamais
  synchronisés.
- **Outils de développement (F12)** : exposés uniquement dans les builds Debug.
- **Honnêteté runtime** : version WebView2 réelle affichée ; un runtime *Fixed
  Version* plus ancien que `130.0.0.0` déclenche un bandeau de sécurité.

## Démarrage

Prérequis : Windows 10/11, SDK .NET 8, runtime WebView2 Evergreen.

```powershell
dotnet build MadagascarBrowser.sln
dotnet run --project src/MadagascarBrowser
```

Options de démarrage :

```powershell
# Fenêtre privée dès le lancement
dotnet run --project src/MadagascarBrowser -- --private

# Racine de données isolée (recette, profil jetable)
dotnet run --project src/MadagascarBrowser -- --data-root "C:\Temp\profil-test"
```

## Tests

```powershell
# Unitaires + Shell (225 tests)
dotnet test tests/MadagascarBrowser.Tests/MadagascarBrowser.Tests.csproj
dotnet test tests/MadagascarBrowser.Shell.Tests/MadagascarBrowser.Shell.Tests.csproj

# Recette GUI réelle (7 scénarios, poste Windows interactif requis)
dotnet test tests/MadagascarBrowser.E2E/MadagascarBrowser.E2E.csproj
```

La recette E2E :

- lance le vrai exécutable avec un profil temporaire (`--data-root`) ;
- navigue vers un serveur HTTP local ;
- vérifie le titre affiché, le retour/avant, la fenêtre privée, l'effacement,
  le téléchargement `content-disposition`, la fenêtre À propos et les DevTools ;
- écrit ses captures dans `docs/captures/e2e/`.

Elle ne tourne **pas** dans une CI headless : FlaUI et WebView2 exigent une
session de bureau Windows réelle. Les tests unitaires restent, eux, exécutables
partout.

## Limites de plateforme vs dettes

Distinction stricte : une **limite de plateforme** est imposée par WebView2 ou
Windows ; une **dette** est un manque de l'application, corrigeable ici. Une
dette n'est jamais présentée comme une limite de plateforme.

| Sujet | Nature | État |
|---|---|---|
| TLS, réseau, JavaScript, CSS, rendu | Limite de plateforme : moteur WebView2/Chromium hérité | Assumé, non réimplémenté |
| Mises à jour de sécurité du moteur | Limite de plateforme : Evergreen est mis à jour par Microsoft | Détecté et affiché ; un runtime figé obsolète est signalé |
| Traduction de pages | Limite de plateforme : fonctionnalité interne du moteur, non exposée par l'API WebView2 | Hors périmètre niche |
| Galerie d'extensions | Limite de plateforme : WebView2 n'expose pas la galerie d'extensions de Chrome/Edge | Hors périmètre |
| Synchronisation de compte | Hors périmètre produit : le navigateur ne crée aucun compte | Non prévu |
| Mise à jour automatique de l'application | Dette produit (pas une limite) | **Ouverte** : distribution manuelle ; Velopack/MSIX à planifier |
| Navigation privée | Dette produit | **Levée** : `IsInPrivateModeEnabled`, badge, aucune session enregistrée |
| Effacement cookies/cache/historique | Dette produit | **Levée** : dialogue granulaire + `ClearBrowsingDataAsync` |
| Nom de fichier téléchargé | Dette produit | **Levée** : `DownloadStarting` + RFC 5987 |
| Retour sur page en erreur | Dette produit | **Levée** : `GoBack`/`CanGoBack`, couvert par la recette E2E |
| Détection d'un runtime obsolète | Dette produit | **Levée** : `WebViewRuntimeInfo` + bandeau + fenêtre À propos |
| DevTools | Décision produit | Disponibles en Debug uniquement, jamais en build public |
| Autofill / mots de passe | Support partiel de plateforme + choix produit | Opt-in local, désactivé par défaut |
| Mise à jour des listes de blocage | Dette produit | **Ouverte** : listes embarquées, pas encore de mise à jour distante |

## Structure

- `src/MadagascarBrowser.Core` : logique testable (navigation, DataSaver, AdBlock,
  paramètres, téléchargements, runtime).
- `src/MadagascarBrowser` : coquille WPF/WebView2.
- `tests/MadagascarBrowser.Tests` : tests unitaires Core.
- `tests/MadagascarBrowser.Shell.Tests` : tests de la coquille sans GUI.
- `tests/MadagascarBrowser.E2E` : recette GUI FlaUI.
- `docs/FEATURE-CLASSIFICATION.md` : tableau (A) dette / (B) plateforme / (C) hors périmètre.
- `docs/RELEASE-NOTES-2026-10-24.md` : note de version.
- `docs/E2E-RECETTE.md` : mode d'emploi de la recette.

## Vie privée

Aucune télémétrie, aucun compte, aucune synchronisation. Les données de
navigation restent dans le profil local ; la navigation privée n'écrit ni
session ni historique. L'autofill et l'enregistrement des mots de passe sont
désactivés par défaut et restent locaux lorsqu'ils sont activés.
