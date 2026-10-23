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


---

# Phase 4 — Réconciliation et décision d’intégration (2026-10-23)

> Cet addendum est la référence actuelle. Les valeurs de HEAD ci-dessous remplacent les anciennes valeurs historiques citées dans les sections précédentes.

## 1. Références réconciliées

- **[vérifié]** HEAD réel de `fix/audit-madagascar-browser` : `4707cbe95a9f3aba92349c521d695c254740ca8c`.
- **[vérifié]** `b629946657346352a3f6f9ec389dd39af6650ede` est un commit documentaire intermédiaire ; ce n’est pas le HEAD actuel.
- **[vérifié]** La CI du HEAD `4707cbe95a9f3aba92349c521d695c254740ca8c` est terminale `completed / success`, run #51 : https://github.com/eemntirey/MadaSearch/actions/runs/39969492371. Elle valide la branche d’audit seule, pas l’arbre combiné.
- **[vérifié]** Branche de sauvegarde exacte : `backup/audit-backup-ea5461d`, SHA `ea5461d235ddd749370baa11948242638cd744c3`. La liste actuelle des refs ne contient pas `fix/audit-backup-ea5461d`.
- **[vérifié]** `main` : `2b80b96b6ceea52913079667db49d265cdd1cd67`; `feat/design-moderne` : `357f4ed5e84383fa0a42802f69eecec74b9e8e5d`.
- **[vérifié]** Ancêtre commun retourné par Compare API : `2b80b96b6ceea52913079667db49d265cdd1cd67`.
- **[vérifié]** Compare base → audit : 51 commits, 26 fichiers modifiés ; base → feature : 42 commits, 79 fichiers modifiés. Leur intersection contient exactement les 12 chemins ci-dessous.

## 2. Cartographie des 12 fichiers communément modifiés

Les SHA ci-dessous sont les blob SHA lus via l’API pour chaque version : base / audit / feature. La classification indique la décision d’intégration, pas une affirmation que Git a produit un conflit de lignes textuel.

| Fichier | Base blob | Audit blob | Feature blob | Classe / décision |
|---|---|---|---|---|
| `src/MadagascarBrowser.Core/Data/DownloadService.cs` | `25b183ddb51851e0dfdf222ea9ef6fcd95a736ba` | `9e5c4633713a5c52d2be87268f5abd84e438f71f` | `29f4208614ae5286e68e497e9705dae3b95a2f28` | **[bloqué] conflit réel** — récupération d’interruption/date/taille audit à combiner avec les changements feature ; aucune version choisie aveuglément |
| `src/MadagascarBrowser/AssemblyInfo.cs` | `cc29e7f74114aefdbef8164f7696c6e31d85d150` | `663b114cb61088018e1c451c6c2afeccaf860f05` | `b0ec827578bab22ee9959849b4a2e11ecfc228c0` | **[bloqué] conflit réel** — les deux côtés ont modifié le fichier ; vérification de compatibilité nécessaire |
| `src/MadagascarBrowser/MainWindow.xaml` | `15b50e64530427304b7ec6de6247a984ce7ccaad` | `6c616b3b5b5393b64945597680d63dedc4bd742b` | `2b105e75157034d5e417fd1e7ebb09c4f3ab608e` | **[bloqué] conflit réel** — ajouts de design et de commandes d’audit à rapprocher |
| `src/MadagascarBrowser/MainWindow.xaml.cs` | `e4ec7fdc0a549245161945776c9bdd9efdd91df2` | `8e8f3c29e1cd8a402376544f9f8a247b5b63c249` | `4ae676bff5191667c5f57aa61363e2a4453ee8d9` | **[bloqué] conflit dur** — logique de fenêtre et cycle de vie divergent ; une fusion manuelle compilable est requise |
| `src/MadagascarBrowser/Shell/BrowserServices.cs` | `27c74d9f1cafba755fae7f5cc42407788af85576` | `94ce3c097ed500defecbbfd0057c17800b7aa535` | `9c0bad362e99f44de1fd2a5e5406137b8ae0d5bf` | **[bloqué] conflit réel** — récupération des téléchargements côté audit et services métriques/chemins côté feature |
| `src/MadagascarBrowser/Shell/BrowserView.xaml` | `4ab06096ab894ab6a64374f8e1e857322ebd9aa1` | `d645aa869c976d020ab1def8b7034bf58d8afe3d` | `dd5cf3b239b3e1131a0e8ff5ee671175c1848603` | **[bloqué] conflit réel** — états de sécurité et présentation diffèrent ; conserver les états visuels du design et les états de sécurité audit |
| `src/MadagascarBrowser/Shell/BrowserView.xaml.cs` | `fab74a84aa5d07803d82e1c3510ef286374514ad` | `a610148640f5cd766e45bd42240ad6f23fb23cce` | `6be445e6083499a2c3ef6a767028e86a3f3ab43c` | **[bloqué] conflit dur** — feature réorganise fortement l’initialisation/navigation ; l’audit ajoute validation d’origine fail-closed, profil privé, effacement et téléchargement |
| `src/MadagascarBrowser/Shell/SettingsView.xaml` | `5bfbb579b4e0ab166940163f6f73af036c5b696d` | `df80bb29a28140dae60bb426d3ae13d1a05f3ae4` | `262abf715cfa37f1afdf84a61be53bb1f468a7bf` | **[bloqué] conflit réel** — interface d’effacement sélectif audit et paramètres Data Saver/design doivent coexister |
| `src/MadagascarBrowser/Shell/SettingsView.xaml.cs` | `3e94aef3426703815beb968bd72151671e5cd870` | `41512943eb3e70029a9f65d26a1bd873fd1b2a5c` | `817e2d5273ef1b9c2cee061dc6c3a59620bab2b2` | **[bloqué] conflit réel** — handler d’effacement audit et persistance du seuil Data Saver feature |
| `src/MadagascarBrowser/Shell/TabManager.cs` | `318d920a592c4a791473580e4811bb15a16c8afd` | `137b15cce6c0f341560c7470780a8a88726016c7` | `32fa9309a8e8e05fec31304ffcc778967299ba8c` | **[bloqué] conflit dur** — gestion des onglets/fermetures feature et cycle de vie privé/effacement audit doivent être fusionnés sans perte |
| `src/MadagascarBrowser/Shell/TabViewModel.cs` | `fbb9f8cb8c7f39ccdb3abbd8e225af264b61e697` | `36fe22842bbb17c08ded6d242eda73731c38e61f` | `02b91e440799f968677289dc87dfc545a6bbd293` | **[bloqué] conflit réel** — état privé audit et propriétés/commandes de tab feature |
| `src/MadagascarBrowser/Shell/WebViewEnvironment.cs` | `4dc4439f835e5cde48d971aa6624a3afe51b9437` | `d2d1102d5721e8f55b06f40ffe8d45af9e04848e` | `a8ccf61365058cbad81608d3c7d969b7a49cec7a` | **[bloqué] conflit dur** — profil temporaire distinct audit face à la fabrique d’environnement/chemins et récupération feature |

### Correctifs audit obligatoires à ne pas perdre

- validation de l’origine de `WebMessageReceived` en mode fail-closed ;
- profil privé temporaire non persistant, isolé du profil normal ;
- snapshots périodiques de session ;
- récupération de téléchargement avec taille exacte et date de fichier compatible ;
- état `Interrupted` pour les téléchargements réellement interrompus ;
- effacement sélectif historique/cookies/cache, confirmation et vérification, sans supprimer les catégories non choisies.

Les fonctionnalités de design, navigation, onglets, recherche dans la page, métriques, chemins et services de la branche feature doivent également être conservées. Les versions sont lues et les SHA établis, mais l’assemblage de ces comportements n’est pas encore prouvé par build/test.

## 3. Stratégie d’intégration et état réel

- **[vérifié]** La branche `integration/audit-design-moderne` a été créée à partir de `feat/design-moderne` (SHA initial `357f4ed5e84383fa0a42802f69eecec74b9e8e5d`) via l’API GitHub.
- **[vérifié]** À cette étape, son arbre de code est encore celui de feature ; aucun correctif audit n’a été appliqué aux 12 fichiers communs. La branche est un point de travail, **pas un arbre combiné validé**.
- **[bloqué]** Aucun commit de fusion de code n’est créé tant que les fichiers de conflit dur ne peuvent pas être assemblés et testés sans checkout .NET fonctionnel. Ne pas appeler cette branche « fusion terminée ».
- **[vérifié]** Le commit de rapport qui accompagne cet addendum sera un commit documentaire sur la branche d’intégration ; il ne modifie pas le code produit.
- **[non vérifié]** La comparaison d’ancêtre `feat/design-moderne` → `integration/audit-design-moderne` sera confirmée après le commit documentaire ; le point de départ de la branche est bien le SHA de feature.
- **[bloqué]** Une PR vers la branche d’audit ne peut pas être déclarée propre tant que les changements de code communs ne sont pas réconciliés. La PR #2 reste ouverte et la branche de sauvegarde est conservée.

## 4. Écart des tests : 118 / 184 / 189 / total combiné

- **[vérifié] 118** : le TRX/runner CI sur le commit de code audit `22415efd7eb5d75a992abd7ca230394441c285a6` a signalé 118 réussis, 0 échec, 0 ignoré : https://github.com/eemntirey/MadaSearch/actions/runs/39965989504.
- **[non vérifié] 184** : aucune preuve TRX ou CI identifiée ne confirme ce nombre. Il est retiré comme résultat d’exécution.
- **[vérifié statiquement] 189** : sur `feat/design-moderne`, 132 `[Fact]` + 57 cas `[InlineData]` = 189 cas statiques attendus ; ce n’est pas un résultat d’exécution.
- **[bloqué] Total réel combiné** : inconnu tant que l’arbre combiné n’est pas construit et testé. Aucun chiffre n’est extrapolé en additionnant les deux inventaires.

## 5. Build, tests et CI de phase 4

- **[vérifié]** La CI terminale `completed / success` du SHA d’audit `4707cbe95a9f3aba92349c521d695c254740ca8c` est le run #51 ci-dessus ; ce n’est pas un test de l’arbre combiné.
- **[non exécuté]** `dotnet restore`, `dotnet build -c Release` et `dotnet test` sur l’arbre combiné : aucun arbre de code combiné n’a été produit, et le shell n’a pas d’accès DNS à GitHub / aucun checkout local exploitable.
- **[bloqué]** Aucun statut CI ne sera étiqueté « vert pour le code combiné » tant que la CI ne porte pas sur un SHA qui contient réellement les correctifs des deux côtés.
- **[non exécutée]** Recette GUI Windows/WebView2 : aucun environnement Windows interactif disponible. Cookies/stockage/cache privés, fermeture forcée, effacement sélectif réel, restauration après crash et certificat invalide restent à tester sur Windows.

## 6. Risques résiduels

1. Les quatre fichiers classés conflit dur (`MainWindow.xaml.cs`, `BrowserView.xaml.cs`, `TabManager.cs`, `WebViewEnvironment.cs`) pilotent le cycle de vie, les profils et les événements WebView2 ; les fusionner sans build est un risque élevé.
2. Les huit autres fichiers de l’intersection doivent être intégrés en conservant à la fois l’UI et les comportements audit.
3. Les tests du second projet feature ne valident pas encore les correctifs d’audit, et le total de tests combiné reste inconnu.
4. L’isolation réelle WebView2 et le nettoyage après arrêt brutal ne sont pas prouvés par la CI automatisée.
5. La branche de sauvegarde `backup/audit-backup-ea5461d` et l’issue #3 sont conservées.

## 7. Conclusion de phase 4

- **[vérifié]** HEAD d’audit et branche de sauvegarde réconciliés ; merge-base et 12 intersections confirmés ; trois blob SHA relevés par fichier.
- **[bloqué]** Intégration de code non terminée : conflits dur documentés, aucun arbre combiné ne peut être certifié.
- **[non vérifié]** Build/tests/CI du code combiné et recette GUI.
- **[vérifié]** Aucune fusion vers `main` effectuée.
