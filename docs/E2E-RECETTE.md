# Recette GUI (FlaUI)

La suite `tests/MadagascarBrowser.E2E` pilote l'exécutable réel avec UI
Automation. Elle vérifie ce qu'un test unitaire ne peut pas prouver : le rendu
WebView2, les fenêtres, le menu et les téléchargements.

## Prérequis

- Windows 10/11 avec session de bureau interactive.
- Runtime WebView2 Evergreen installé.
- SDK .NET 8.
- Application compilée dans la configuration testée (`Debug` par défaut).

## Exécution

```powershell
dotnet build MadagascarBrowser.sln
dotnet test tests/MadagascarBrowser.E2E/MadagascarBrowser.E2E.csproj
```

ou :

```powershell
powershell -ExecutionPolicy Bypass -File scripts/run-e2e.ps1
```

Chaque test lance une instance isolée avec `--data-root` dans `%TEMP%`, puis
supprime ce profil. Les captures sont écrites dans `docs/captures/e2e/`.

## Scénarios

| Test | Ce qui est prouvé | Capture |
|---|---|---|
| `Navigation_LoadsPageAndReflectsTitleInStatusBar` | WebView2 charge une page locale, le titre remonte dans la barre d'état | `01-navigation-accueil.png` |
| `BackAndForward_NavigateThroughHistoryWithoutStaleContent` | Retour puis avant sur deux pages, sans contenu périmé | `02-historique-retour-avant.png` |
| `PrivateWindow_IsExplicitlyMarkedAndOpenedFromMenu` | Le menu ouvre une vraie fenêtre privée, badge et titre visibles | `05-fenetre-privee.png` |
| `ClearBrowsingData_DialogClearsSelectedCategories` | Cases par défaut, confirmation, retour « effacées » | `06-effacement-donnees.png` |
| `Download_ContentDispositionFileNameIsPreserved` | Nom RFC 5987 `recu-été.txt` et contenu du fichier, nom affiché dans la vue Téléchargements | `03-download-content-disposition.png` |
| `AboutWindow_ReportsRealRuntimeInsteadOfPretending` | Version application, version runtime, état réel | `07-a-propos-runtime.png` |
| `DevTools_AreAvailableInDebugBuild` | Menu DevTools présent et fenêtre DevTools ouverte via F12 | `04-devtools-ouverts.png` |

## Ce que la suite ne fait pas

- Elle ne tourne pas en CI headless : FlaUI exige un bureau Windows.
- Elle ne remplace pas une mesure DataSaver sur cinq sites réels (réseau mobile).
- Elle n'installe pas un runtime WebView2 ancien : le bandeau runtime est couvert
  par les tests unitaires `WebViewRuntimeInfoTests`, pas par une manipulation
  système destructrice.
