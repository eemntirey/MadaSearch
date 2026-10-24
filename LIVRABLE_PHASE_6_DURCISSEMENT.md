# LIVRABLE — PHASE 6.1 : Nettoyage (nullabilité, code mort, reformatage)

## 1. Numéro et titre de la phase
**Phase 6.1 — Durcissement : avertissements de nullabilité, code mort, reformatage.**
(Point attribué explicitement par l'utilisateur avant les portes 6.2 à 6.4.)

## 2. Fichiers modifiés
- `src/MadagascarBrowser/MainWindow.xaml.cs` — `OnSourceInitialized(object? sender, …)` : signature alignée sur le délégue `EventHandler` (CS8622).
- `tests/MadagascarBrowser.Shell.Tests/NewWindowRequestHandlerTests.cs` — `RecordingTabHost.NewTab(string? url)` conforme à `ITabHost.NewTab(string? url = null)`, l'URL absente traitée comme vide (CS8767).
- `src/MadagascarBrowser.Core/Data/HistoryService.cs` — 62 corrections d'espaces (indentation), aucune logique touchée.
- `src/MadagascarBrowser/AssemblyInfo.cs` — coupure des attributs `ThemeInfo` sur des lignes lisibles (9 corrections).

## 3. Tests ajoutés
Aucun : les corrections sont de signature nullable et d'espaces, sans comportement modifié. La non-régression est démontrée par les 184 tests existants (tous verts après chaque commit).

## 4. Build et tests
- **Build :** 0 erreur, **0 avertissement** (`dotnet build MadagascarBrowser.sln --no-incremental` complet avant et après).
- **Tests :** 184/184 verts — Core 170/170, Shell 14/14.

## 5. Vérifications manuelles
Aucune nécessaire : corrections statiques ( compilateur + analyseur ). Les preuves sont le build complet et les tests.
- `dotnet format style --diagnostics IDE0051 IDE0005` : **0 finding** (aucun membre privé inutilisé, aucun using inutile).
- `dotnet format whitespace --verify-no-changes` : **0 finding** après correction (71 avant, sur les 2 seuls fichiers concernés).

## 6. Statuts des points
| Point | Statut | Détail |
|---|---|---|
| Avertissements CS8622/CS8767 | **confirmé** | 0 avertissement en build complet non incrémental |
| Code mort | **confirmé** | Analyseur Roslyn IDE0051/IDE0005 : rien. Un scan heuristique a listé des suspects (API de Core appelées par le Shell/XAML, handlers XAML, tests) — tous faux positifs examinés, **rien de supprimable sans risque** ; la phase 4 en cours référence certaines de ces API. |
| Reformatage des fichiers illisibles | **confirmé** | Les 2 fichiers signalés par l'analyseur ont été reformatés ; diff `-w` vide (espaces uniquement). |
| CS8612/CS8618/CS8625 (mention de l'ordre) | **confirmé** | Ces codes n'existaient plus dans le code ; seuls CS8622/CS8767 subsistaient, traités ci-dessus. |

## 7. Dette restante (hors périmètre de 6.1)
Aucune nouvelle dette introduite. Les dettes connues des phases 1 à 5 restent documentées dans leurs livrables respectifs (`LIVRABLE_PHASE_1_PARCOURS.md`, `docs/LIVRABLE.md`).

## 8. Portes
**Les phases 6.2 à 6.4 ne sont pas déclarées closes par ce livrable.** 6.2 (journal fichier + exceptions globales) a été livrée plus tôt dans la mission (commits `9dbccb5`, `af04a21`) et utilisée comme outil de diagnostic pendant les phases 1 et 4. 6.3 (rejouer tous les parcours sur l'état final) et 6.4 (livrable final, dette exhaustive) restent à faire — 6.3 dépend de la session de recette manuelle de l'utilisateur (feuille dans `LIVRABLE_PHASE_1_PARCOURS.md`).
