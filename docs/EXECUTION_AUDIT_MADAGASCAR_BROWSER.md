# Rapport d’exécution — audit de MadagascarBrowser

Date : 2026-10-23  
Branche : `fix/audit-madagascar-browser`

## 1. État et commits

- État avant cette mise à jour : `ea5461d235ddd749370baa11948242638cd744c3`.
- Commit correctif code testé précédemment : `22415efd7eb5d75a992abd7ca230394441c285a6`.
- Correctif effacement sélectif : `8397968fc87824bd2a7351d5f86f90ed58fa19ac`.
- Commit documentaire précédent : `ea5461d235ddd749370baa11948242638cd744c3`.
- Référence de sauvegarde distante créée pendant la phase 2 : `backup/audit-backup-ea5461d`, pointant sur `ea5461d235ddd749370baa11948242638cd744c3`.
- Aucun tag Git distant n’a été créé : les outils Git disponibles dans cet environnement exposent la lecture des références et la création de branches, mais pas la création d’un objet tag. La branche de sauvegarde fournit une référence distante de sécurité sans réécrire l’historique.

## 2. Cinq lots d’audit déjà présents

### P0.1 — Canal WebMessageReceived
Fichiers principaux : `src/MadagascarBrowser.Core/Security/NativeWebMessagePolicy.cs`, `src/MadagascarBrowser/Shell/BrowserView.xaml.cs`, `tests/MadagascarBrowser.Tests/NativeWebMessagePolicyTests.cs`. La politique applique une liste blanche stricte des commandes et valide l’origine ; les messages non autorisés sont rejetés. Tests de politique d’origine, de commande et de payload présents.

### P0.2 — Navigation privée
Fichiers principaux : `src/MadagascarBrowser/Shell/PrivateProfileSession.cs`, `WebViewEnvironment.cs`, `TabManager.cs`, `TabViewModel.cs`, `MainWindow.xaml.cs`, ainsi que les tests `PrivateProfileSessionTests.cs` et `PrivateBrowsingPolicyTests.cs`. Le profil WebView2 privé est distinct, temporaire et nettoyé à la fermeture ou lors de la récupération des profils orphelins. L’isolation réelle cookies/stockage entre instances WebView2 n’a pas encore été vérifiée par recette GUI.

### P1 — Sessions et téléchargements
La session normale est sauvegardée périodiquement et à la fermeture. Les téléchargements interrompus sont réconciliés au démarrage sans supprimer les fichiers utilisateur. Le correctif `22415efd7eb5d75a992abd7ca230394441c285a6` empêche de déclarer terminé un fichier préexistant uniquement parce que sa taille correspond ; date et taille sont contrôlées. Tests de non-régression ajoutés.

### P1 — Effacement sélectif
Le correctif `8397968fc87824bd2a7351d5f86f90ed58fa19ac` sépare historique, données de site/cookies et cache disque WebView2. L’historique SQLite est vérifié après effacement ; les favoris, téléchargements et paramètres ne sont pas supprimés par ces catégories. Tests dans `BrowsingDataClearTests.cs`.

### P1 — Livraison
Workflow Windows .NET présent : `.github/workflows/dotnet.yml`. La branche reste distincte de `main`. La recette GUI Windows/WebView2 demeure nécessaire pour confirmer navigation, téléchargement, mode privé, effacement, restauration après arrêt brutal et certificat invalide.

## 3. État des branches et réconciliation

Références observées par GitHub :
- `main` : `2b80b96b6ceea52913079667db49d265cdd1cd67`.
- `feat/design-moderne` : `357f4ed5e84383fa0a42802f69eecec74b9e8e5d`.
- `fix/audit-madagascar-browser` : `ea5461d235ddd749370baa11948242638cd744c3` avant cette mise à jour du rapport.
- Ancêtre commun des branches : `2b80b96b6ceea52913079667db49d265cdd1cd67`.

Le comparatif GitHub confirme que `feat/design-moderne` est exactement 42 commits devant `main`. La branche d’audit a 48 commits propres par rapport à l’ancêtre commun et ne contient pas les 42 commits de fonctionnalité. Le comparatif `feat/design-moderne` contre la branche d’audit indique une divergence (`ahead_by=42`, `behind_by=48`). Les changements se chevauchent dans les fichiers critiques, notamment `MainWindow.xaml.cs`, `BrowserView.xaml.cs`, `TabManager.cs`, `WebViewEnvironment.cs`, `DownloadService.cs`, les projets de tests et la solution.

Une PR exploratoire #1 a été créée puis fermée sans fusion parce que GitHub n’a pas confirmé une fusion propre. Aucun merge vers `main` ou vers `feat/design-moderne` n’a eu lieu.

### Blocage de réconciliation — documenté, non contourné artificiellement

La stratégie prévue est une fusion explicite avec résolution fichier par fichier, ou une branche d’intégration basée sur `feat/design-moderne` avec reprise des commits d’audit. Elle ne peut pas être exécutée de façon sûre dans cet environnement : aucun checkout local du dépôt n’est disponible et la tentative de clonage a échoué avec une erreur DNS `Could not resolve host: github.com`. Les outils GitHub accessibles ici permettent de lire des refs/fichiers et de créer des commits de contenu, mais ne fournissent pas une session Git locale avec merge/cherry-pick, résolution interactive des conflits et exécution de .NET. Fabriquer une fusion de l’extérieur en remplaçant des fichiers entiers serait risqué et contraire à l’exigence de préserver les deux intentions.

**Conclusion : la réconciliation n’est pas terminée.** Aucun commit de fusion ni cherry-pick n’a été créé. Le contrôle `git merge-base --is-ancestor origin/feat/design-moderne HEAD` ne peut donc pas être revendiqué comme réussi. Ce blocage est une limite d’environnement vérifiée, pas une fusion propre.

## 4. Build, tests et CI

Commandes réellement exécutées par le workflow Windows sur le commit de code `22415efd7eb5d75a992abd7ca230394441c285a6` :
```powershell
dotnet restore MadagascarBrowser.sln
dotnet build MadagascarBrowser.sln --configuration Release --no-restore
dotnet test MadagascarBrowser.sln --configuration Release --no-build --logger "trx;LogFileName=test-results.trx"
```

Résultat de cette exécution :
- Build Release : succès, 0 avertissement, 0 erreur.
- Tests : 118 réussis, 0 échec, 0 ignoré.
- [Run de validation du code](https://github.com/eemntirey/MadaSearch/actions/runs/39965989504).

### CI terminale du commit documentaire précédent

- SHA : `ea5461d235ddd749370baa11948242638cd744c3`.
- Run : [39966578453](https://github.com/eemntirey/MadaSearch/actions/runs/39966578453).
- État final observé via l’API GitHub : `completed`, conclusion `success`.
- Job `build-and-test` : Restore, Build Release et Test terminés avec succès.

Cette CI verte ne valide que le commit `ea5461d`, qui précède la mise à jour documentaire présente. La CI du nouveau commit de rapport doit être consultée sur son SHA propre ; aucune réussite anticipée n’est affirmée ici.

### Écart 184 / 118 tests

Le nombre de 184 tests annoncé dans un résumé antérieur ne correspond pas à l’exécution GitHub vérifiée sur `22415ef`, qui a rapporté 118 tests. Aucun test n’a été désactivé pour obtenir ce résultat. L’environnement actuel ne permet pas d’inspecter localement la solution ni de lancer un inventaire indépendant ; la différence de périmètre entre ces deux chiffres reste donc non résolue et doit être rapprochée des projets de tests et des résultats TRX lors d’une exécution locale/runner accessible.

## 5. Recette Windows/WebView2

**Non exécutée dans cette phase.** Aucun environnement GUI Windows/WebView2 interactif n’est accessible depuis l’environnement de travail courant. La CI exécute le build et les tests automatisés, mais ne simule pas une recette manuelle du navigateur.

Checklist à exécuter sur Windows avec WebView2 :
- [ ] Navigation HTTP(S), démarrage et commandes clavier.
- [ ] Téléchargement terminé, annulé et interrompu.
- [ ] Mode privé : cookie créé dans une fenêtre privée absent du profil normal ; historique normal inchangé ; cache/stockage séparé.
- [ ] Fermeture normale : suppression du profil privé temporaire.
- [ ] Arrêt forcé : nettoyage au redémarrage sans toucher aux fichiers utilisateur.
- [ ] Effacement séparé de l’historique, des cookies/données de site et du cache ; conservation des catégories non sélectionnées.
- [ ] Restauration de la session après arrêt brutal.
- [ ] Certificat invalide et coupure réseau.

Le point critique non prouvé est l’isolation effective des cookies, du stockage local et du cache dans le moteur WebView2 réel, ainsi que le nettoyage après un crash réel.

## 6. Risques résiduels et décision de livraison

1. **Intégration incomplète** : les 42 commits de `feat/design-moderne` ne sont pas intégrés dans la branche d’audit.
2. **Aucun build/test de l’état combiné** : l’état final fusionné n’existe pas encore.
3. **Recette GUI absente** : isolation privée, nettoyage après crash, certificat invalide et comportements de téléchargement doivent être vérifiés sur Windows.
4. **Comptage de tests à rapprocher** : 184 annoncés antérieurement contre 118 observés dans la CI citée.
5. **Tag de sauvegarde absent** : la branche distante `backup/audit-backup-ea5461d` est créée au même SHA comme référence de sécurité.

La branche d’audit peut être utilisée comme base de travail pour reprendre l’intégration dans un checkout Git local fonctionnel, mais elle ne doit pas être déclarée livrable combinée avec `feat/design-moderne` tant que les conflits n’ont pas été résolus et que la CI du résultat fusionné n’est pas verte.

**Aucune fusion vers `main` effectuée.**


---

# Phase 3 — rapport de clôture d’intégration

Date du contrôle : 2026-10-23. Les états ci-dessous sont qualifiés individuellement.

## A. Branche, accès et références

- **[vérifié]** Branche d’audit avant le présent ajout : `fix/audit-madagascar-browser`, SHA `9122f2f65b55b7508d46098ca16f40707bd08517`.
- **[vérifié]** `main` = `2b80b96b6ceea52913079667db49d265cdd1cd67`; `feat/design-moderne` = `357f4ed5e84383fa0a42802f69eecec74b9e8e5d`; ancêtre commun retourné par l’API Compare = `2b80b96b6ceea52913079667db49d265cdd1cd67`.
- **[vérifié]** Tentatives shell exécutées :
  - `git ls-remote https://github.com/eemntirey/MadaSearch.git` → code 128, `Could not resolve host: github.com`.
  - `gh auth status` et `gh repo view eemntirey/MadaSearch` → `gh: command not found`, code 127.
  - `curl https://api.github.com/repos/eemntirey/MadaSearch` → code 6, DNS `Could not resolve host: api.github.com`.
  - archive `https://codeload.github.com/eemntirey/MadaSearch/tar.gz/357f4ed5e84383fa0a42802f69eecec74b9e8e5d` → code 6, DNS `Could not resolve host: codeload.github.com`.
  - `git clone --depth 1 --branch feat/design-moderne https://github.com/eemntirey/MadaSearch.git /tmp/MadaSearch-phase3` → code 128, DNS.
  - même clone avec `GIT_CURL_VERBOSE=1` → DNS confirmé dans la trace.
- **[vérifié]** L’API GitHub par le connecteur reste accessible. Le shell réseau et l’outil `gh` ne sont pas disponibles.

## B. Variante d’intégration et résultat

- **[vérifié]** Variante B — PR API de diagnostic #2 : https://github.com/eemntirey/MadaSearch/pull/2, base `fix/audit-madagascar-browser`, head `feat/design-moderne`.
- **[vérifié]** GitHub indique `mergeable=false`, `mergeable_state=dirty`, 42 commits côté head, 79 fichiers modifiés. La PR est restée en brouillon ; aucune opération de fusion n’a été appelée.
- **[vérifié]** Compare API : audit a 49 commits au-delà de l’ancêtre commun à la date de ce contrôle ; `feat/design-moderne` en a 42. La branche d’audit n’est donc pas un descendant de `feat/design-moderne`.
- **[bloqué]** L’intégration n’est pas réalisée. L’API REST standard consultée ne donne pas la liste exacte des conflits de lignes ni un arbre de fusion résolu. En l’absence de checkout Git et de runner .NET, créer un arbre artificiel ou fusionner des fichiers entiers ne permettrait pas de garantir la préservation des deux côtés.
- **[vérifié]** Les trois versions (ancêtre commun/main, audit, feature) ont été récupérées via API pour les 12 fichiers touchés par les deux branches. Ce sont des **candidats de conflit** issus du chevauchement des diffs, pas une affirmation que chacun a un conflit de ligne :
  1. `src/MadagascarBrowser.Core/Data/DownloadService.cs`
  2. `src/MadagascarBrowser/AssemblyInfo.cs`
  3. `src/MadagascarBrowser/MainWindow.xaml`
  4. `src/MadagascarBrowser/MainWindow.xaml.cs`
  5. `src/MadagascarBrowser/Shell/BrowserServices.cs`
  6. `src/MadagascarBrowser/Shell/BrowserView.xaml`
  7. `src/MadagascarBrowser/Shell/BrowserView.xaml.cs`
  8. `src/MadagascarBrowser/Shell/SettingsView.xaml`
  9. `src/MadagascarBrowser/Shell/SettingsView.xaml.cs`
  10. `src/MadagascarBrowser/Shell/TabManager.cs`
  11. `src/MadagascarBrowser/Shell/TabViewModel.cs`
  12. `src/MadagascarBrowser/Shell/WebViewEnvironment.cs`
- **[vérifié]** Les écarts sont substantiels : `BrowserView.xaml.cs` fait 834 lignes dans la base, 916 sur audit et 1338 sur feature ; `TabManager.cs` fait respectivement 147, 192 et 267 lignes. Le correctif de reprise de téléchargement avec contrôle de date est présent sur audit et absent de feature ; les politiques `NativeWebMessagePolicy` et l’effacement sélectif sont également présents sur audit mais pas sur feature.
- **[bloqué]** `git merge-base --is-ancestor origin/feat/design-moderne HEAD` n’a pas pu être exécuté localement et ne peut pas être revendiqué comme réussi. L’intégration est donc **bloquée — non vérifiée**, pas partielle ni terminée.
- **[vérifié]** Issue de suivi créée : https://github.com/eemntirey/MadaSearch/issues/3.

## C. Écart 184 vs 118 — inventaire statique réel

Les fichiers de tests et les projets ont été lus via l’API GitHub. Le décompte statique porte sur les attributs xUnit dans les fichiers `.cs`, et non sur un TRX simulé.

### Branche d’audit `fix/audit-madagascar-browser`

- **[vérifié]** Un projet de test est référencé dans `MadagascarBrowser.sln` : `tests/MadagascarBrowser.Tests/MadagascarBrowser.Tests.csproj`. Le projet utilise xUnit 2.5.3 et Microsoft.NET.Test.Sdk 17.8.0.
- **[vérifié]** 21 fichiers C# de tests ; 75 attributs `[Fact]`, 10 `[Theory]`, 43 `[InlineData]`, aucun `[MemberData]` ou `[ClassData]` constaté dans les fichiers de théorie examinés.
- **[vérifié]** Calcul des cas xUnit : les `[Fact]` donnent 75 cas et les lignes `[InlineData]` ajoutent 43 cas, soit **118 cas attendus**. Ce total correspond au résultat CI déjà constaté sur le commit de code `22415ef` : 118 réussis, 0 échec, 0 ignoré, run https://github.com/eemntirey/MadaSearch/actions/runs/39965989504.

### Branche `feat/design-moderne`

- **[vérifié]** La solution contient deux projets de test : `tests/MadagascarBrowser.Tests/MadagascarBrowser.Tests.csproj` et `tests/MadagascarBrowser.Shell.Tests/MadagascarBrowser.Shell.Tests.csproj`. Les deux utilisent xUnit 2.5.3 et Microsoft.NET.Test.Sdk 17.8.0.
- **[vérifié]** 26 fichiers C# de tests ; inventaire statique : 132 `[Fact]`, 13 `[Theory]`, 57 `[InlineData]`. Aucun `[MemberData]` ou `[ClassData]` n’a été trouvé dans les fichiers de théorie inspectés.
- **[vérifié]** Total statique calculé par expansion des données : 132 cas `[Fact]` + 57 cas `[InlineData]` = **189 cas de test attendus** (sous réserve de la découverte réelle par le runner).
- **[non vérifié]** Aucun run CI/TRX de `feat/design-moderne` n’a été obtenu dans la recherche des runs Actions consultée. Les 189 cas sont un calcul statique, pas un résultat d’exécution.
- **[vérifié]** L’écart entre les 118 cas de l’audit et le total statique de 189 sur feature vient de l’ajout de tests et du second projet `MadagascarBrowser.Shell.Tests`, ainsi que de tests supplémentaires dans des fichiers partagés.
- **[non vérifié]** Le nombre de 184 tests annoncé précédemment n’est confirmé ni par le TRX de `22415ef` (118), ni par l’inventaire statique de `feat/design-moderne` (189). Sa provenance exacte n’est pas établie ; il ne faut pas le présenter comme un résultat de CI. Le total intégré réel ne sera connu qu’après fusion, build et test du commit intégré.

## D. CI du SHA précédent et du présent rapport

- **[vérifié]** Le commit documentaire précédent `9122f2f65b55b7508d46098ca16f40707bd08517` a une CI terminale `completed / success`, run #49 : https://github.com/eemntirey/MadaSearch/actions/runs/39967399505.
- **[non vérifié]** Cette CI ne couvre pas le commit de ce présent ajout documentaire ; sa CI devra être consultée sur son propre SHA après le push. Aucun build/test du code combiné n’a été exécuté.
- **[vérifié]** Aucun build Release ni test local n’a été lancé dans cet environnement, car .NET/WPF n’est pas exécutable ici et l’accès réseau Git shell est bloqué par DNS.

## E. Recette Windows/WebView2

- **[non vérifié — non exécutée]** La recette GUI Windows/WebView2 n’a pas été exécutée : aucun environnement Windows interactif avec WebView2 n’est disponible dans cette session.
- **[non vérifié]** L’isolation réelle des cookies, du stockage local et du cache privé, le nettoyage après fermeture/crash, les scénarios de certificat invalide et la restauration après arrêt brutal restent à tester manuellement sur Windows.
- **[vérifié]** Les tests automatisés antérieurs valident des politiques et services, mais ne prouvent pas à eux seuls l’isolation du moteur WebView2 réel.

## F. Étapes exactes pour débloquer

À exécuter dans un environnement Windows avec Git, .NET 8 et accès réseau fonctionnels :

```powershell
git fetch origin
git switch fix/audit-madagascar-browser
git merge --no-ff origin/feat/design-moderne
# Pour chaque conflit réel :
git show :1:<fichier>
git show :2:<fichier>
git show :3:<fichier>
git diff --check
git merge-base --is-ancestor origin/feat/design-moderne HEAD
dotnet restore MadagascarBrowser.sln
dotnet build MadagascarBrowser.sln --configuration Release --no-restore
dotnet test MadagascarBrowser.sln --configuration Release --no-build --logger "trx;LogFileName=test-results.trx"
```

Résoudre les conflits en conservant la navigation et les fonctionnalités récentes ainsi que les correctifs d’audit ; ne pas choisir aveuglément un côté. Pousser sans force, attendre le statut terminal de CI sur le SHA fusionné, puis exécuter la recette GUI.

## G. Décision de phase 3

- **[vérifié]** L’accès API GitHub a été utilisé pour comparer les branches, récupérer les trois versions des 12 fichiers candidats, créer la PR de diagnostic #2 et l’issue #3.
- **[bloqué]** L’intégration reste non réalisée en raison de la PR `mergeable_state=dirty`, de l’absence d’un checkout Git opérationnel et de l’impossibilité d’exécuter les tests .NET sur un arbre combiné.
- **[vérifié]** Aucune fusion vers `main` effectuée.


### Addendum de vérification CI — 2026-10-23, après fin du run

- **[vérifié]** Le SHA `b629946657346352a3f6f9ec389dd39af6650ede` a une CI terminale `completed / success`, run #50 : https://github.com/eemntirey/MadaSearch/actions/runs/39969133614.
- **[vérifié]** Les logs du runner Windows confirment :
  - `dotnet restore MadagascarBrowser.sln` : réussi ;
  - `dotnet build MadagascarBrowser.sln --configuration Release --no-restore` : `Build succeeded`, 0 avertissement, 0 erreur ;
  - `dotnet test MadagascarBrowser.sln --configuration Release --no-build --logger "trx;LogFileName=test-results.trx"` : 118 réussis, 0 échec, 0 ignoré.
- **[vérifié]** Cette CI valide la branche d’audit seule, sans les 42 commits de `feat/design-moderne`. Elle ne valide donc pas un arbre fusionné, puisque cet arbre n’a pas été créé.
- **[vérifié]** Le statut de la CI du commit `b629946657346352a3f6f9ec389dd39af6650ede` est terminal. Le présent ajout documentaire sera un nouveau commit et déclenchera son propre workflow ; son résultat doit être lu sur le run correspondant et ne peut pas être présumé à partir du run #50.
