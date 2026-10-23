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


---

## Addendum terminal — CI de phase 4, run #52 (2026-10-23)

- **[vérifié]** SHA testé : `9e56d77c63ec0e96213af216a2574071644be838`; branche `integration/audit-design-moderne`; run #52 : https://github.com/eemntirey/MadaSearch/actions/runs/39970673599.
- **[vérifié]** Statut terminal GitHub Actions : `completed / success`.
- **[vérifié]** Commandes réellement exécutées sur le runner Windows :
  ```powershell
  dotnet restore MadagascarBrowser.sln
  dotnet build MadagascarBrowser.sln --configuration Release --no-restore
  dotnet test MadagascarBrowser.sln --configuration Release --no-build --logger "trx;LogFileName=test-results.trx"
  ```
- **[vérifié]** Restore réussi. Build Release : `Build succeeded`, 0 avertissement .NET, 0 erreur. Tests découverts/exécutés : `MadagascarBrowser.Tests` = 170 réussis, 0 échec, 0 ignoré ; `MadagascarBrowser.Shell.Tests` = 14 réussis, 0 échec, 0 ignoré ; total runner **184 réussis, 0 échec, 0 ignoré**.
- **[vérifié]** Correction de l’écart de comptage : 184 était non confirmé dans les preuves de phase 3, mais il est maintenant confirmé par les sorties de test du run #52 sur l’arbre feature-only. Le calcul statique 189 (132 `[Fact]` + 57 `[InlineData]`) reste un inventaire statique qui surestime de 5 le total découvert par ce runner ; les causes précises de l’écart ne sont pas encore isolées.
- **[bloqué]** Important : le SHA #52 contient le code de `feat/design-moderne` plus le rapport et le workflow de CI de phase 4 ; il ne contient pas encore les correctifs de code des 12 fichiers communs. Le succès CI valide donc le build/test de la branche feature seule, pas une intégration des deux branches.
- **[vérifié]** Comparaison Git API : `feat/design-moderne` est un ancêtre de `integration/audit-design-moderne` ; comparaison `status=ahead`, `ahead_by=2`, `behind_by=0`, merge-base `357f4ed5e84383fa0a42802f69eecec74b9e8e5d`.
- **[vérifié]** PR #4 de suivi : https://github.com/eemntirey/MadaSearch/pull/4, base `fix/audit-madagascar-browser`, head `integration/audit-design-moderne`, `mergeable=false`, `mergeable_state=dirty`, brouillon, non fusionnée. Le blocage reste lié aux fichiers de code communs.
- **[non exécutée]** Recette GUI Windows/WebView2 : aucun environnement interactif disponible.
- **[vérifié]** Aucune fusion vers `main` effectuée.


---

## Addendum terminal — validation du SHA de branche d’intégration (run #54)

- **[vérifié]** SHA `00cb82df44ef3b3fa16bb892d5756372e98c66dd` — [run #54](https://github.com/eemntirey/MadaSearch/actions/runs/39971215760) — statut terminal `completed / success`.
- **[vérifié]** Restore réussi ; build Release réussi avec 0 avertissement .NET et 0 erreur ; tests : 170 Core + 14 Shell = **184 réussis, 0 échec, 0 ignoré**.
- **[vérifié]** Ce SHA ne change que le workflow CI par rapport au commit de rapport précédent ; le code applicatif reste celui de `feat/design-moderne`. Cette réussite ne prouve pas l’intégration des correctifs audit dans les 12 fichiers communs.
- **[vérifié]** Le run #53 sur le commit documentaire `224cbdcbdbdc8a2e240f22fecc67d449a57aed5b` a été annulé lorsque le run #54 a été lancé sur le nouveau SHA. Le run #54 est terminal et réussi.
- **[bloqué]** Le SHA d’un arbre de code combiné audit + feature n’existe pas encore. Le résultat de CI ci-dessus est celui du candidat feature-only et de ses fichiers de rapport/workflow.


---

# Clôture phase de résolution — arbre combiné audit + design (2026-10-23)

## 1. SHA et état Git

- **[vérifié]** Commit combiné de code : `38ee4a1686cb1915e76a81a793037863fbce1f47`.
- **[vérifié]** Arbre Git de code combiné : `90118482263a3eecc4b3f60be4cd03217c1e23ea`.
- **[vérifié]** Comparaison `feat/design-moderne` → commit combiné : `status=ahead`, `ahead_by=9`, `behind_by=0`; merge-base = `357f4ed5e84383fa0a42802f69eecec74b9e8e5d`. La branche feature est donc un ancêtre du commit combiné.
- **[vérifié]** Les 12 fichiers partagés sont résolus et écrits en blobs Git ; aucun marqueur de conflit n’a été ajouté.
- **[vérifié]** Le run #55 est clos : `completed / success`, SHA `56d8864038a2a67a12b95e41cfffa9de7e70d6c8`, https://github.com/eemntirey/MadaSearch/actions/runs/39971764202. Il valide le candidat antérieur, pas l’arbre combiné.
- **[vérifié]** Aucune fusion vers `main` n’a été effectuée.

## 2. Réconciliation des 12 fichiers

Les SHA « base », « audit » et « feature » sont les blobs relevés par l’API GitHub sur les références correspondantes. « Fusionné » est le blob du fichier dans l’arbre combiné final.

| Fichier | Blob base | Blob audit | Blob feature | Blob fusionné | Intentions audit conservées | Intentions feature conservées |
|---|---|---|---|---|---|---|
| `src/MadagascarBrowser.Core/Data/DownloadService.cs` | `25b183ddb51851e0dfdf222ea9ef6fcd95a736ba` | `9e5c4633713a5c52d2be87268f5abd84e438f71f` | `29f4208614ae5286e68e497e9705dae3b95a2f28` | `cb030bae77986c41b37774908b9bfb28d6996113` | Récupération après arrêt, taille exacte, date de modification compatible, état `Interrupted`. | Contrat de service de téléchargement et structure des données conservés pour l’interface feature. |
| `src/MadagascarBrowser/AssemblyInfo.cs` | `cc29e7f74114aefdbef8164f7696c6e31d85d150` | `663b114cb61088018e1c451c6c2afeccaf860f05` | `b0ec827578bab22ee9959849b4a2e11ecfc228c0` | `dee3641c7dbdb3926c0cdbb7030549462d8e82f1` | `InternalsVisibleTo` pour les tests d’audit et accès aux helpers internes. | `ThemeInfo` WPF conservé ; les deux assemblages de tests sont déclarés. |
| `src/MadagascarBrowser/MainWindow.xaml` | `15b50e64530427304b7ec6de6247a984ce7ccaad` | `6c616b3b5b5393b64945597680d63dedc4bd742b` | `2b105e75157034d5e417fd1e7ebb09c4f3ab608e` | `ad88673a4dbe0b2953881c3ea67a722c450f194b` | Le contrat visuel reste compatible avec les commandes de fenêtre et les paramètres de session. | Barre de titre personnalisée, WindowChrome, onglets, statut et design moderne conservés. |
| `src/MadagascarBrowser/MainWindow.xaml.cs` | `e4ec7fdc0a549245161945776c9bdd9efdd91df2` | `8e8f3c29e1cd8a402376544f9f8a247b5b63c249` | `4ae676bff5191667c5f57aa61363e2a4453ee8d9` | `464102ed8f74f5792c79eef403a4f0ce48c46adf` | Fenêtre privée, non-persistance de sa session et snapshots périodiques de la session normale. | Raccourcis, plein écran, barre de titre, déplacement/épinglage, fermeture et cycle de vie WPF conservés. |
| `src/MadagascarBrowser/Shell/BrowserServices.cs` | `27c74d9f1cafba755fae7f5cc42407788af85576` | `94ce3c097ed500defecbbfd0057c17800b7aa535` | `9c0bad362e99f44de1fd2a5e5406137b8ae0d5bf` | `1ef5205330e21f9acd63fd6d6b338a76f8ba3743` | Récupération des téléchargements au démarrage. | Métriques DataSaver, paramètres, navigation, chemins BrowserPaths et services feature conservés. |
| `src/MadagascarBrowser/Shell/BrowserView.xaml` | `4ab06096ab894ab6a64374f8e1e857322ebd9aa1` | `d645aa869c976d020ab1def8b7034bf58d8afe3d` | `dd5cf3b239b3e1131a0e8ff5ee671175c1848603` | `a5f9fb1bff6783a008af5d87c9d6c746540a88eb` | Entrée de menu pour fenêtre privée et états de sécurité. | Indicateur de sécurité, navigation, menu et styles feature conservés. |
| `src/MadagascarBrowser/Shell/BrowserView.xaml.cs` | `fab74a84aa5d07803d82e1c3510ef286374514ad` | `a610148640f5cd766e45bd42240ad6f23fb23cce` | `6be445e6083499a2c3ef6a767028e86a3f3ab43c` | `5b2581e856b4f4890bdff2d615b3c4c062222529` | Origine stricte fail-closed, liste blanche des commandes natives, profil privé, garde-fous de persistance et effacement WebView2 sélectif. | Recherche de la page d’accueil, ITabHost, navigation, pages hors ligne, DataSaver, métriques, recherche dans la page et cycle de vie WebView2 conservés. |
| `src/MadagascarBrowser/Shell/SettingsView.xaml` | `5bfbb579b4e0ab166940163f6f73af036c5b696d` | `df80bb29a28140dae60bb426d3ae13d1a05f3ae4` | `262abf715cfa37f1afdf84a61be53bb1f468a7bf` | `62c08f97b1c56a3fa7106b7a6cd1b1ad2288d3f0` | Cases d’effacement historique/cookies/cache et feedback utilisateur. | Contrôle DataSaver, seuil d’image et restauration de session conservés. |
| `src/MadagascarBrowser/Shell/SettingsView.xaml.cs` | `3e94aef3426703815beb968bd72151671e5cd870` | `41512943eb3e70029a9f65d26a1bd873fd1b2a5c` | `817e2d5273ef1b9c2cee061dc6c3a59620bab2b2` | `bc112d0cafe55bb5bee8ab80201df418181935e1` | Confirmation avant effacement, contrôle du profil privé, vérification de l’historique SQLite et feedback d’échec. | Validation et persistance du seuil DataSaver, thème, densité et paramètres conservés. |
| `src/MadagascarBrowser/Shell/TabManager.cs` | `318d920a592c4a791473580e4811bb15a16c8afd` | `137b15cce6c0f341560c7470780a8a88726016c7` | `32fa9309a8e8e05fec31304ffcc778967299ba8c` | `403c8d2050cf75e717ad745e14345018c85cd777` | Cycle de vie du profil privé, nettoyage et effacement sélectif. | ITabHost, changement d’onglet, réouverture, favoris, plein écran et sélection après fermeture conservés. |
| `src/MadagascarBrowser/Shell/TabViewModel.cs` | `fbb9f8cb8c7f39ccdb3abbd8e225af264b61e697` | `36fe22842bbb17c08ded6d242eda73731c38e61f` | `02b91e440799f968677289dc87dfc545a6bbd293` | `796a146301e0deecdd5bbadce9486e54cac9be94` | Marqueurs `IsPrivate` et chemin de profil temporaire. | État d’indicateur de sécurité, erreurs de navigation et notifications UI feature conservés. |
| `src/MadagascarBrowser/Shell/WebViewEnvironment.cs` | `4dc4439f835e5cde48d971aa6624a3afe51b9437` | `d2d1102d5721e8f55b06f40ffe8d45af9e04848e` | `a8ccf61365058cbad81608d3c7d969b7a49cec7a` | `2daad57298cfe1de8d162448a62ac1d4bc0b92a9` | Un environnement par profil, profil privé temporaire isolé, oubli de profil et possibilité de retenter après échec. | URLs de démarrage/erreur, chemin BrowserPaths et avertissement des anciens profils conservés. |

## 3. Preuves des six correctifs obligatoires

- **[vérifié] Origine native fail-closed :** `BrowserView.xaml.cs` appelle `NativeWebMessagePolicy.IsAllowedOrigin(e.Source, LocalHost)` avant de traiter un message ; le dispatch est limité à `NewTab`, `CloseTab` et `FocusAddressBar`. La commande de recherche reste autorisée uniquement après validation de l’origine.
- **[vérifié] Profil privé temporaire :** `PrivateProfileSession` crée un dossier aléatoire sous `LocalAppData/MadagascarBrowser/PrivateProfiles`, maintient un verrou exclusif, supprime le dossier à la fermeture et recherche les profils orphelins. `WebViewEnvironment.GetAsync(profileFolder)` sépare les environnements par chemin.
- **[vérifié] Snapshots :** `MainWindow.xaml.cs` utilise un `DispatcherTimer` de 15 secondes et `SaveSessionSnapshot()`; la persistance est refusée pour les fenêtres privées.
- **[vérifié] Récupération des téléchargements :** `RecoverInProgressDownloads()` vérifie `FileInfo.Length == TotalBytes` et `LastWriteTimeUtc >= StartedAt`; les téléchargements incomplets passent à `Interrupted`. L’enum `DownloadState` contient désormais cet état.
- **[vérifié] Effacement sélectif vérifié :** les catégories historique, cookies/données de site et cache sont sélectionnées indépendamment ; confirmation explicite ; appel `ClearBrowsingDataAsync` ; vérification que l’historique SQLite est vide si cette catégorie a été choisie.
- **[vérifié] État `Interrupted` :** le modèle et les tests de récupération ont été mis à jour ; les tests couvrent un fichier partiel conservé, un fichier préexistant qui ne doit pas être déclaré terminé et un fichier complet avec taille/date compatibles.

## 4. Commits créés via l’API Git Data

- **[vérifié]** `45c6bfd630aa89141ba96b63833b5622cd66b8dd` — premier arbre combiné, arbre `d131cb4653cd53e3f2a21ffea6187deaceeb6d17`.
- **[vérifié]** `e23d72ab6634ac61957a73924cca2c2dc30e01c9` — ajout de `DownloadState.Interrupted` et tests de récupération, arbre `30221698107fc5630f75c4d6b6a0f608a652bfd0`.
- **[vérifié]** `07a05c42f01ab0ec127bd7a3142301c0611e595a` — correction du chemin offline, déplacement des tests WPF vers le projet Shell et déclaration d’accès interne, arbre `c75e69e9797a8e3a7ad064032d8ee4ce5c39fafb`.
- **[vérifié]** `38ee4a1686cb1915e76a81a793037863fbce1f47` — utilisation de la politique d’origine stricte dans le pont WebMessage, arbre de code final `90118482263a3eecc4b3f60be4cd03217c1e23ea`.
- **[vérifié]** Les mises à jour de référence ont été effectuées avec `force=false`. Aucun force-push n’a été fait.

## 5. Build Release et tests sur le code combiné

- **[vérifié]** Run #59, SHA exact `38ee4a1686cb1915e76a81a793037863fbce1f47`, statut terminal `completed / success` : https://github.com/eemntirey/MadaSearch/actions/runs/39975203181.
- **[vérifié]** Commandes exécutées par le runner Windows :
  ```powershell
  dotnet restore MadagascarBrowser.sln
  dotnet build MadagascarBrowser.sln --configuration Release --no-restore
  dotnet test MadagascarBrowser.sln --configuration Release --no-build --logger "trx;LogFileName=test-results.trx"
  ```
- **[vérifié]** Restore réussi ; Build Release réussi avec **0 avertissement et 0 erreur**.
- **[vérifié]** `MadagascarBrowser.Tests` : 194 réussis, 0 échoué, 0 ignoré.
- **[vérifié]** `MadagascarBrowser.Shell.Tests` : 21 réussis, 0 échoué, 0 ignoré.
- **[vérifié]** Total combiné : **215 réussis, 0 échoué, 0 ignoré**.
- **[vérifié]** L’itération précédente run #58 a également terminé `completed / success`, 215 tests réussis : https://github.com/eemntirey/MadaSearch/actions/runs/39974674018.
- **[vérifié]** Les échecs intermédiaires #56 et #57 ont été corrigés : l’état `Interrupted` a été ajouté au modèle ; les chemins, les tests WPF et le gestionnaire XAML manquant ont été corrigés. Aucun test n’a été désactivé, supprimé ou ignoré.

## 6. Réconciliation exacte du comptage 189 / 184

- **[vérifié]** Les deux projets utilisent xUnit 2.5.3 et Microsoft.NET.Test.Sdk 17.8.0.
- **[vérifié]** `MadagascarBrowser.Tests` cible `net8.0` ; `MadagascarBrowser.Shell.Tests` cible `net8.0-windows` avec WPF.
- **[vérifié]** Sur `feat/design-moderne`, l’inventaire source complet donne **117 `[Fact]` Core + 10 `[Fact]` Shell = 127 `[Fact]`**, **13 `[Theory]`**, et **53 `[InlineData]` Core + 4 Shell = 57 `[InlineData]`**. Il n’y a pas de `[MemberData]`, `[ClassData]` ni de `Skip=` dans cet inventaire.
- **[vérifié]** Le nombre statique correct est donc **127 + 57 = 184 cas**, ce qui correspond exactement au runner feature-only : 170 Core + 14 Shell = 184.
- **[vérifié]** Le total **189** provenait d’un mélange de périmètres : cinq `[Fact]` de `BrowsingDataClearTests.cs`, fichier audit-only absent de `feat/design-moderne`, ont été inclus dans le décompte statique de la branche feature. Ces cinq tests ont ensuite été ajoutés au projet Shell de l’arbre combiné. Ce n’était pas cinq cas ignorés par xUnit.
- **[vérifié]** `[InlineData(null)]` dans `NewWindowRequestHandlerTests.cs` est une ligne de données de théorie ordinaire et est découverte par le runner ; elle n’explique aucun écart. Aucun test n’a été modifié pour faire correspondre artificiellement les chiffres.
- **[vérifié]** L’arbre combiné a 215 tests découverts/exécutés, car il inclut les tests d’audit ajoutés et les tests de récupération de téléchargement.

## 7. Statuts CI et run #55

- **[vérifié]** Run #55 : `completed / success`, SHA `56d8864038a2a67a12b95e41cfffa9de7e70d6c8` — https://github.com/eemntirey/MadaSearch/actions/runs/39971764202.
- **[vérifié]** Run #59 : `completed / success`, SHA combiné `38ee4a1686cb1915e76a81a793037863fbce1f47` — https://github.com/eemntirey/MadaSearch/actions/runs/39975203181.
- **[vérifié]** Le statut vert s’applique au SHA combiné cité, et non aux runs #55–#57 pris isolément.

## 8. Recette Windows/WebView2

- **[non exécutée]** La recette GUI manuelle n’a pas été exécutée : le runner Windows GitHub Actions est sans session interactive WPF/WebView2 exploitable dans cette tâche.
- **[non vérifié]** L’isolation réelle des cookies, du local storage et du cache entre fenêtres, la fermeture forcée de WebView2, le nettoyage après crash et la navigation sur certificat invalide nécessitent une recette GUI interactive distincte.
- **[vérifié]** Le build Release et les tests automatisés passent ; cela ne remplace pas ces vérifications runtime.

## 9. Risques résiduels et état des PR

- **[non vérifié]** Le comportement visuel de l’indicateur de sécurité, les parcours de fenêtre privée et l’effacement réel du profil WebView2 n’ont pas été observés en recette GUI.
- **[non vérifié]** La suppression physique d’un profil temporaire peut être différée si un processus WebView2 conserve un handle ; le nettoyage orphelin réessaie au prochain démarrage.
- **[vérifié]** PR #2 et PR #4 restent ouvertes en brouillon/non fusionnées ; leur état de fusion n’est pas déclaré propre. PR #2 : https://github.com/eemntirey/MadaSearch/pull/2 ; PR #4 : https://github.com/eemntirey/MadaSearch/pull/4.
- **[vérifié]** Issue #3 reste ouverte pour suivre la recette GUI restante : https://github.com/eemntirey/MadaSearch/issues/3.
- **[vérifié]** La sauvegarde `backup/audit-backup-ea5461d` est conservée.
- **[vérifié]** **Aucune fusion vers `main` effectuée.**


---

# Phase 6 — audit de cohérence, topologie de livraison et recette GUI (2026-10-23)

## 1. Inventaire reproductible des tests et réconciliation 132 / 127

### Méthode et périmètre

- **[vérifié]** La branche inspectée pour l'inventaire statique feature est le SHA `357f4ed5e84383fa0a42802f69eecec74b9e8e5d`. L'arbre Git a été listé via l'API, puis chaque fichier C# de test a été lu par chemin avec ce ref explicite.
- **[vérifié]** Les 28 fichiers de tests C# présents sous `tests/` sont les trois fichiers Shell et les 25 fichiers Core listés ci-dessous. Aucun `[MemberData]`, `[ClassData]` ni `Skip=` n'a été trouvé dans cet inventaire.
- **[vérifié]** Décompte reproductible par fichier : les colonnes Fact / Theory / InlineData sont des occurrences d'attributs source. Les cas attendus xUnit sont `Fact + InlineData` pour les fichiers recensés ; les attributs Theory ne sont pas ajoutés séparément car les lignes InlineData représentent leurs cas.
  
| Projet | Fichier | Fact | Theory | InlineData | MemberData / ClassData / Skip |
|---|---|---:|---:|---:|---:|
| Shell.Tests | `AppLogTests.cs` | 1 | 0 | 0 | 0 / 0 / 0 |
| Shell.Tests | `NewWindowRequestHandlerTests.cs` | 4 | 1 | 4 | 0 / 0 / 0 |
| Shell.Tests | `TabSelectionTests.cs` | 5 | 0 | 0 | 0 / 0 / 0 |
| Tests (Core) | `AcceleratorCommandTests.cs` | 6 | 2 | 12 | 0 / 0 / 0 |
| Tests (Core) | `AdBlockServiceTests.cs` | 8 | 0 | 0 | 0 / 0 / 0 |
| Tests (Core) | `AsyncDebouncerTests.cs` | 3 | 0 | 0 | 0 / 0 / 0 |
| Tests (Core) | `BrowserPathsTests.cs` | 7 | 0 | 0 | 0 / 0 / 0 |
| Tests (Core) | `BrowserProfileServiceTests.cs` | 2 | 0 | 0 | 0 / 0 / 0 |
| Tests (Core) | `BrowserSettingsServiceTests.cs` | 2 | 0 | 0 | 0 / 0 / 0 |
| Tests (Core) | `ConnectionSecurityTests.cs` | 7 | 2 | 10 | 0 / 0 / 0 |
| Tests (Core) | `DataSaverMetricsServiceTests.cs` | 1 | 0 | 0 | 0 / 0 / 0 |
| Tests (Core) | `DataSaverServiceTests.cs` | 9 | 0 | 0 | 0 / 0 / 0 |
| Tests (Core) | `DateTimeStorageTests.cs` | 4 | 0 | 0 | 0 / 0 / 0 |
| Tests (Core) | `DownloadFileNameResolverTests.cs` | 4 | 0 | 0 | 0 / 0 / 0 |
| Tests (Core) | `DownloadProgressWriterTests.cs` | 6 | 0 | 0 | 0 / 0 / 0 |
| Tests (Core) | `DownloadServiceTests.cs` | 2 | 0 | 0 | 0 / 0 / 0 |
| Tests (Core) | `FavoritesServiceTests.cs` | 1 | 0 | 0 | 0 / 0 / 0 |
| Tests (Core) | `FindInPageStateTests.cs` | 9 | 0 | 0 | 0 / 0 / 0 |
| Tests (Core) | `HistoryServiceTests.cs` | 3 | 0 | 0 | 0 / 0 / 0 |
| Tests (Core) | `NavigationFailureResolverTests.cs` | 2 | 1 | 6 | 0 / 0 / 0 |
| Tests (Core) | `NavigationServiceTests.cs` | 12 | 1 | 3 | 0 / 0 / 0 |
| Tests (Core) | `NavigationTargetTests.cs` | 6 | 0 | 0 | 0 / 0 / 0 |
| Tests (Core) | `NewWindowRequestTests.cs` | 5 | 1 | 5 | 0 / 0 / 0 |
| Tests (Core) | `OfflinePageServiceTests.cs` | 1 | 0 | 0 | 0 / 0 / 0 |
| Tests (Core) | `ResourceInterceptionScopeTests.cs` | 2 | 3 | 10 | 0 / 0 / 0 |
| Tests (Core) | `SecurityIndicatorTests.cs` | 5 | 0 | 0 | 0 / 0 / 0 |
| Tests (Core) | `SessionServiceTests.cs` | 2 | 0 | 0 | 0 / 0 / 0 |
| Tests (Core) | `SqliteRobustnessTests.cs` | 7 | 0 | 0 | 0 / 0 / 0 |
| Tests (Core) | `ZoomLevelTests.cs` | 1 | 2 | 7 | 0 / 0 / 0 |
| **TOTAL** | **28 fichiers** | **127** | **13** | **57** | **0 / 0 / 0** |

- **[vérifié]** Les totaux exacts sont 127 `[Fact]`, 13 `[Theory]`, 57 `[InlineData]`, donc 184 cas statiques attendus (127 + 57).
- **[vérifié]** La phase 4 qui indiquait 132 `[Fact]` était erronée de cinq occurrences. La clôture phase 4 qui indiquait 127 `[Fact]` était correcte. L'erreur vient d'un comptage statique précédent, pas d'un fichier supprimé/renommé ni d'un attribut dérivé. Le nombre 189 est donc retiré pour la branche feature.
- **[vérifié]** La CI feature-only run #52 a découvert/exécuté 170 Core + 14 Shell = 184 tests, conforme à l'inventaire source. Le runner et l'inventaire portent sur le même SHA de feature avec les fichiers de tests recensés.
- **[vérifié]** Branche audit seule : le rapport d'audit recense 75 `[Fact]`, 10 `[Theory]`, 43 `[InlineData]` dans 21 fichiers et run de CI d'audit à 118 réussis. Arbre combiné : run #59, 194 Core + 21 Shell = 215 réussis. Ces chiffres ne doivent pas être additionnés naïvement car des tests et projets sont partagés et l'arbre combiné contient des tests de non-régression additionnels.
- **[vérifié]** Les cinq tests de `BrowsingDataClearTests.cs` étaient ajoutés dans le commit combiné, pas présents dans le SHA feature. Ils ne justifient pas un total feature de 189.

## 2. Runs #56 et #57 — causes et non-désactivation des tests

### Run #56

- **[vérifié]** Run #56 : SHA `45c6bfd630aa89141ba96b63833b5622cd66b8dd`, conclusion `failure`, étapes Restore réussie, Build Release échouée, Test non exécutée car conditionnée à la réussite du build : https://github.com/eemntirey/MadaSearch/actions/runs/39973962889.
- **[vérifié]** Le commit #56 introduit la récupération des téléchargements qui référence `DownloadState.Interrupted`, mais l'enum `DownloadState` ne contenait pas encore cette valeur à ce SHA.
- **[vérifié]** La correction correspondante est le commit `e23d72ab6634ac61957a73924cca2c2dc30e01c9`, qui ajoute `Interrupted` dans `src/MadagascarBrowser.Core/Data/BrowserDownload.cs` et ajoute des tests de récupération dans `tests/MadagascarBrowser.Tests/DownloadServiceTests.cs`.
- **[non vérifié]** Le connecteur GitHub donne le statut et les étapes mais ne restitue pas le contenu de l'archive des logs du run; l'erreur de compilation exacte (texte/numéro CS) n'a donc pas pu être citée. La cause de compilation est établie par le diff du commit correctif, mais pas par la ligne de log brute.

### Run #57

- **[vérifié]** Run #57 : SHA `e23d72ab6634ac61957a73924cca2c2dc30e01c9`, conclusion `failure`, Restore réussie, Build Release échouée, étape Test non exécutée : https://github.com/eemntirey/MadaSearch/actions/runs/39974314474.
- **[vérifié]** Le commit suivant `07a05c42f01ab0ec127bd7a3142301c0611e595a` corrige le contrat `OfflinePageService` (`paths.OfflinePagesPath` remplacé par `paths.OfflineDirectory`), ajoute `InternalsVisibleTo("MadagascarBrowser.Shell.Tests")` et déplace les tests WPF `BrowsingDataClearTests.cs` et `PrivateProfileSessionTests.cs` du projet Core vers `tests/MadagascarBrowser.Shell.Tests`.
- **[vérifié]** Ces changements expliquent les familles de causes de compilation de l'itération #57 : contrat de chemin non aligné et tests WPF placés dans le mauvais projet/assemblage. Les logs bruts n'étant pas accessibles via le connecteur, le message d'erreur exact reste **[non vérifié]**.
- **[vérifié]** Run #58 sur le commit correctif `07a05c42f01ab0ec127bd7a3142301c0611e595a` est `completed / success`, 215 tests réussis : https://github.com/eemntirey/MadaSearch/actions/runs/39974674018. Run #59 valide ensuite le SHA final combiné.

### Contrôle de non-désactivation

- **[vérifié]** Le diff #56 ajoute des fichiers de politique et de tests (`NativeWebMessagePolicyTests.cs`, `PrivateBrowsingPolicyTests.cs`, `PrivateProfileSessionTests.cs`, `BrowsingDataClearTests.cs`) ; aucun fichier de test n'y est supprimé.
- **[vérifié]** Le diff #57 ajoute des tests de récupération dans `DownloadServiceTests.cs) et ajoute l'état du modèle ; il ne supprime pas de test.
- **[vérifié]** Le diff #58 renomme/déplace deux fichiers de tests entre projets, sans suppression de contenu, et ajoute une déclaration `InternalsVisibleTo`.
- **[vérifié]** Les étapes `Test` marquées `skipped` dans #56/#57 sont sautées par le workflow parce que le build a échoué, et non parce que les tests xUnit ont été désactivés.
- **[non vérifié]** Une comparaison exhaustive ligne par ligne de chaque attribut xUnit à tous les SHA intermédiaires n'est pas attestée par un artefact automatisé conservé. Les diffs de commits disponibles ne montrent toutefois aucune suppression de tests ni ajout de `Skip`.

## 3. PR #2 et PR #4 — décision appliquée

- **[vérifié] PR #2 fermée sans fusion** : PR de diagnostic `feat/design-moderne → fix/audit-madagascar-browser` devenue redondante pour la livraison, car la branche d'intégration contient déjà le code feature dans son arbre combiné. Un commentaire de fermeture renvoie vers la PR de livraison #5. PR : https://github.com/eemntirey/MadaSearch/pull/2.
- **[vérifié] PR #4 fermée sans fusion** : PR WIP `integration/audit-design-moderne → fix/audit-madagascar-browser` avec historique divergent et état non fusionnable. La branche d'intégration reste la candidate de livraison; le code combiné n'est pas rapatrié dans la branche audit historique. Un commentaire renvoie vers PR #5. PR : https://github.com/eemntirey/MadaSearch/pull/4.
- **[vérifié]** Les deux fermetures sont accompagnées de commentaires explicatifs. Aucune PR n'a été fusionnée.

## 4. Topologie de livraison retenue

- **[vérifié]** Branche candidate : `integration/audit-design-moderne`, HEAD documentaire `256c166911ae1bbf30d5b476ba2ddff3eb3a7231`, parent direct code combiné `38ee4a1686cb1915e76a81a793037863fbce1f47`.
- **[vérifié]** Branche audit historique `fix/audit-madagascar-browser` : `4707cbe95a9f3aba92349c521d695c254740ca8c). Compare audit → intégration signale une divergence de 51 commits de chaque côté autour de l'ancêtre commun `main`; elle n'est donc pas fast-forwardable.
- **[vérifié]** La branche audit n'a pas été déplacée ni réécrite. La branche de sauvegarde `backup/audit-backup-ea5461d` reste conservée. La candidate de livraison est l'intégration, car elle contient le code combiné testé et le rapport de clôture.
- **[vérifié]** `main` demeure `2b80b96b6ceea52913079667db49d265cdd1cd67`.

## 5. Plan de recette GUI inscrit dans issue #3

- **[vérifié]** Plan exécutable ajouté comme commentaire à l'issue #3 : https://github.com/eemntirey/MadaSearch/issues/3#issuecomment-6368210751.
- **[vérifié]** Le plan décrit environnement Windows 10/11, .NET 8 Desktop Runtime, WebView2 Evergreen, profil utilisateur vierge, critères PASS/FAIL/NON EXÉCUTÉ, captures PNG, journaux et treize scénarios : navigation, téléchargements terminé/annulé/interrompu, isolation privée, fermeture normale/forcée, effacement sélectif par catégorie, annulation de confirmation, restauration après arrêt brutal et certificat invalide.
- **[non exécutée]** Aucun bureau Windows interactif n'est disponible via le runner actuel. Les 13 scénarios restent `NON EXÉCUTÉ`; aucune observation GUI n'est inventée.

## 6. PR de livraison draft #5

- **[vérifié]** PR #5 créée : https://github.com/eemntirey/MadaSearch/pull/5.
- **[vérifié]** Base `main` SHA `2b80b96b6ceea52913079667db49d265cdd1cd67`; head `integration/audit-design-moderne` SHA `256c166911ae1bbf30d5b476ba2ddff3eb3a7231).
- **[vérifié]** État : ouverte, `draft=true), non fusionnée. API Pull Request : `mergeable=true`, `mergeable_state=clean` au contrôle après création.
- **[vérifié]** La PR contient une checklist de livraison, les preuves CI #59/#60, les risques résiduels et les bloqueurs. Elle n'a pas été marquée ready for review.
- **[vérifié]** Le statut clean signifie que GitHub estime le merge techniquement possible à cet instant; il ne lève ni le bloqueur GUI ni les exigences de preuve documentaire. Toute modification ultérieure nécessite une nouvelle vérification CI sur le SHA exact.

## 7. Risques résiduels consolidés

1. **[bloqué]** Recette WPF/WebView2 interactive non exécutée : isolation cookies/cache/localStorage, fermeture forcée, nettoyage des profils orphelins, effacement sélectif réel, restauration après crash et certificat invalide.
2. **[non vérifié]** Les logs bruts des runs #56/#57 ne sont pas accessibles via le connecteur GitHub utilisé; les causes sont déduites des diffs correctifs et des étapes échouées, sans numéro/message de compilation exact.
3. **[non vérifié]** Le contrôle d'attributs xUnit est reproductible pour la feature (table ci-dessus), mais aucun rapport machine dédié n'a été archivé pour comparer intégralement les attributs à chaque SHA intermédiaire.
4. **[vérifié]** PR #2 et #4 sont fermées sans fusion; issue #3, branche audit et branche de sauvegarde sont conservées.
5. **[vérifié]** La PR de livraison #5 reste en brouillon; sa mergeabilité clean ne constitue pas une autorisation de fusion.

## 8. Conclusion phase 6

- **[vérifié]** Le décompte unique de la feature est 127 `[Fact]` + 57 `[InlineData]` = 184 cas; le chiffre 132 de la phase 4 était une erreur de comptage.
- **[vérifié]** Runs #56/#57 terminaux en échec; causes de compilation documentées à partir des commits correctifs, avec limite explicite sur l'accès aux logs bruts. Aucun diff examiné ne supprime ou ne désactive des tests.
- **[vérifié]** PR #2/#4 fermées avec explication; PR #5 draft créée et mergeability `clean` vérifiée.
- **[vérifié]** Plan GUI inscrit dans issue #3; recette encore non exécutée.
- **[vérifié]** **Aucune fusion vers main effectuée.**


---

# Phase 7 — fermeture CI, logs historiques et paquet de recette (2026-10-23)

## 1. Run #61 — statut terminal

- **[vérifié]** Run #61 : `completed / success`, SHA exact `9458bfab340d066f844ce557d3894e5efe00a447`, URL https://github.com/eemntirey/MadaSearch/actions/runs/39977222391.
- **[vérifié]** Le run #61 valide le commit documentaire de phase 6, pas les commits de paquet de recette ajoutés ensuite. Les commits de recette ont déclenché de nouveaux checks ; le SHA courant doit être vérifié séparément.
- **[vérifié]** Aucune correction du code produit n'a été nécessaire pour clore le run #61.

## 2. Récupération des logs bruts #56 et #57

### Méthodes tentées et résultats observés

| Méthode | Run #56 | Run #57 | Résultat |
|---|---|---|---|
| GET `/actions/runs/{id}` | HTTP JSON via connecteur | HTTP JSON via connecteur | **[vérifié]** Métadonnées du run récupérées |
| GET `/actions/runs/{id}/jobs` | job ID `120483618132` | job ID `120484838269` | **[vérifié]** Build Release failure, Test skipped |
| GET `/actions/jobs/{job_id}/logs` via outil de logs de job | contenu décodé récupéré | contenu décodé récupéré | **[vérifié]** Logs bruts exploitables, extraits ci-dessous |
| GET `/actions/runs/{id}/logs` via fetch générique | corps vide dans le connecteur | corps vide dans le connecteur | **[bloqué pour cette voie]** Le wrapper renvoie un contenu vide sans statut HTTP de transport exploitable |
| GET `/actions/runs/{id}/artifacts` | `artifacts: []` | `artifacts: []` | **[vérifié]** Aucun artefact publié sur ces runs |
| Artefact TRX | aucun artefact exposé | aucun artefact exposé | **[vérifié]** Aucun TRX récupérable via l’API d’artefacts |
| Annotations check-run | URL directe rejetée par la liste d’URL autorisées du connecteur (erreur outil 400, sans réponse HTTP GitHub) | idem | **[bloqué]** La voie annotations n’a pas fourni de réponse GitHub ; les logs de job ont permis d’obtenir les erreurs exactes |

### Run #56 — extrait exact

- **[vérifié]** Run : https://github.com/eemntirey/MadaSearch/actions/runs/39973962889 ; job `120483618132`.
- Extrait du log Build Release : `DownloadService.cs(87,111): error CS0117: 'DownloadState' does not contain a definition for 'Interrupted'`.
- **[vérifié]** Cause : le code de récupération référençait `DownloadState.Interrupted` avant que l’enum soit aligné.
- **[vérifié]** Correction : commit `e23d72ab6634ac61957a73924cca2c2dc30e01c9`, ajout de l’état et tests de récupération.

### Run #57 — extraits exacts

- **[vérifié]** Run : https://github.com/eemntirey/MadaSearch/actions/runs/39974314474 ; job `120484838269`.
- Erreurs exactes du Build Release :
  - `BrowsingDataClearTests.cs(1,25): error CS0234: The type or namespace name 'Shell' does not exist in the namespace 'MadagascarBrowser'`
  - `BrowsingDataClearTests.cs(2,17): error CS0234: The type or namespace name 'Web' does not exist in the namespace 'Microsoft'`
  - `PrivateProfileSessionTests.cs(3,25): error CS0234: The type or namespace name 'Shell' does not exist in the namespace 'MadagascarBrowser'`
  - `BrowserServices.cs(37,61): error CS1061: 'BrowserPaths' does not contain a definition for 'OfflinePagesPath'`
  - `BrowserView.xaml(178,108): error CS1061: 'BrowserView' does not contain a definition for 'MenuPrivateWindow_Click'`
- **[vérifié]** Le log indique `5 Error(s)`, puis `Process completed with exit code 1`. L’étape Test a été sautée parce que le build échouait.
- **[vérifié]** Le commit correctif suivant `07a05c42f01ab0ec127bd7a3142301c0611e595a` réaligne `OfflineDirectory`, déplace les tests WPF dans le projet Shell.Tests et expose les membres internes requis. Le run #58 sur ce correctif a terminé success : https://github.com/eemntirey/MadaSearch/actions/runs/39974674018.

### Non-désactivation des tests

- **[vérifié]** Sur #56 et #57, le statut `skipped` de l’étape de workflow Test est la conséquence de l’échec du build ; ce n’est pas un attribut xUnit `Skip`.
- **[vérifié]** Les diffs des commits correctifs ajoutent des tests et déplacent deux fichiers de test, sans suppression de test observée.
- **[non vérifié]** Aucun rapport automatisé d’égalité de tous les attributs xUnit entre chaque SHA intermédiaire n’a été produit. La conclusion est limitée aux diffs examinés et aux logs disponibles.

## 3. Recette GUI — variante B, paquet versionné

- **[vérifié]** Aucun environnement Windows interactif avec bureau et fenêtre WebView2 contrôlable n’est exposé par l’environnement d’exécution de cette session. Le runner GitHub Windows est un runner CI non interactif ; son existence ne permet pas de déclarer une recette manuelle exécutée.
- **[vérifié]** Paquet ajouté sous `docs/recette/` et `scripts/recette/` sur la branche d’intégration :
  - `docs/recette/recette-gui.md` — checklist opposable GUI-01 à GUI-13, prérequis, étapes, attendus et champs de preuve ;
  - `scripts/recette/Test-Prerequisites.ps1` — Windows, .NET runtimes et détection registre WebView2 ;
  - `scripts/recette/Invoke-AppProfile.ps1` — lancement du binaire et empreinte SHA-256, avec avertissement que le profil isolé n’est pas le profil effectif sans support applicatif explicite ;
  - `scripts/recette/Test-PrivateProfileCleanup.ps1` — contrôle d’existence d’un chemin de profil réel ;
  - `scripts/recette/Test-HistoryClear.ps1` — inspection SQLite en lecture seule du schéma ;
  - `scripts/recette/Test-InterruptedDownload.ps1` — inspection SQLite en lecture seule, sans tuer de processus ;
  - `scripts/recette/Test-InvalidCertificate.ps1` — préparation d’un certificat local auto-signé pour test manuel TLS ;
  - `scripts/recette/Collect-Evidence.ps1` — collecte des versions et empreintes et consignes de capture ;
  - `scripts/recette/run-all.ps1` — collecte de prérequis et génération de `recette-resultats.md`, qui marque les 13 scénarios NON EXÉCUTÉ.
- **[non exécuté]** `run-all.ps1` n’a pas été lancé dans cette session : aucun PowerShell Windows avec binaire WPF/WebView2 local n’est attaché à l’environnement. Commande côté Windows :
  `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/recette/run-all.ps1 -AppPath C:\\chemin\\MadagascarBrowser.exe -EvidenceRoot artifacts\\recette\\run-001`.
- **[non exécuté]** GUI-01 à GUI-13 restent chacun NON EXÉCUTÉ. Les scripts automatisent la collecte et l’inspection préliminaire ; les actions visuelles, la vérification du profil WebView2 effectif et les preuves de comportement restent à réaliser sur un poste Windows. Aucun PASS n’est attribué.
- **[vérifié]** Le paquet est référencé dans l’issue #3 via commentaire après publication du commit de recette.

## 4. PR #5 — décision finale de phase 7

- **[vérifié]** Run #61 est success sur `9458bfab340d066f844ce557d3894e5efe00a447`, URL https://github.com/eemntirey/MadaSearch/actions/runs/39977222391.
- **[vérifié]** Run #59 reste success sur le code combiné `38ee4a1686cb1915e76a81a793037863fbce1f47`, 215 tests réussis : https://github.com/eemntirey/MadaSearch/actions/runs/39975203181.
- **[vérifié]** La PR #5 est maintenue en brouillon, URL https://github.com/eemntirey/MadaSearch/pull/5. Le statut a été observé `mergeable=true`, `mergeable_state=clean` après les derniers commits de paquet, avant vérification de la CI de ces nouveaux SHAs.
- **[bloqué]** La condition de promotion en revue n’est pas retenue : la recette GUI intégrale n’est pas PASS, et le paquet n’a pas encore été exécuté sur Windows. La PR reste draft même si son état Git est clean.
- **[vérifié]** Aucun merge vers `main` n’a été exécuté.

## 5. Risques résiduels

1. **[bloqué]** GUI-01 à GUI-13 nécessitent une exécution sur Windows 10/11 avec l’application WPF/WebView2 et la collecte de captures/journaux.
2. **[non vérifié]** Le script de profil ne peut pas imposer un profil WebView2 tant que le binaire n’expose pas de contrat d’argument/configuration confirmé ; le chemin créé ne prouve pas le stockage réellement utilisé.
3. **[non vérifié]** L’inspection SQLite est volontairement en lecture seule et ne suppose aucun nom de table ; les requêtes de comparaison avant/après doivent être adaptées au schéma réel.
4. **[non exécuté]** Le paquet de recette n’a pas été exécuté dans cette session.
5. **[vérifié]** Les logs bruts de build #56/#57 ont maintenant été récupérés par l’API de logs de job ; les erreurs CS exactes ci-dessus remplacent l’incertitude antérieure sur leur texte.
6. **[vérifié]** La branche `main` n’a pas été modifiée.

## 6. Conclusion phase 7

- **[vérifié]** Run #61 : `completed / success` sur SHA `9458bfab340d066f844ce557d3894e5efe00a447`.
- **[vérifié]** Les logs bruts #56/#57 sont récupérés ; causes de compilation et codes CS documentés.
- **[vérifié]** Le paquet de recette est versionné et la checklist couvre GUI-01 à GUI-13.
- **[non exécuté]** La recette interactive et `run-all.ps1` restent à exécuter sur un poste Windows.
- **[vérifié]** PR #5 reste draft ; la promotion n’est pas autorisée tant que les scénarios GUI n’ont pas des preuves réelles.
- **[vérifié]** **Aucune fusion vers main effectuée.**


### Addendum — résultat terminal du paquet de recette

- **[vérifié]** Run #74 a terminé `completed / success` sur SHA `30a3f56d5087f19d940ab1b564aa4dd9ad61c018` : https://github.com/eemntirey/MadaSearch/actions/runs/39978105394.
- **[vérifié]** Logs du job `120498347056` : Build Release `Build succeeded`, `0 Warning(s)`, `0 Error(s)`; `MadagascarBrowser.Shell.Tests.dll` : 21 réussis, 0 échec, 0 ignoré ; `MadagascarBrowser.Tests.dll` : 194 réussis, 0 échec, 0 ignoré. Total : 215 réussis, 0 échec, 0 ignoré.
- **[vérifié]** Run #74 a été observé pending puis in_progress puis completed/success pendant le polling. Il n'a pas expiré.
- **[vérifié]** Au contrôle après run #74, PR #5 était ouverte en draft, `mergeable=true`, `mergeable_state=clean`. Cette propriété Git ne lève pas le bloqueur GUI.
- **[vérifié]** Le commentaire de l’issue #3 contenant le paquet et la commande Windows est https://github.com/eemntirey/MadaSearch/issues/3#issuecomment-6368393445.
- **[non exécuté]** Le paquet PowerShell n’a pas été lancé depuis un poste Windows dans cette session. Le run CI compile et teste le code .NET ; il ne constitue pas l’exécution de GUI-01 à GUI-13.


### Décision finale PR #5 après clôture CI (addendum)

- **[vérifié]** Run #75 est `completed / success` sur SHA `d13a1637b1c3aeec2d26e870f0954455b8e7451f`, Build Release 0 avertissement/0 erreur, 194 tests Core + 21 tests Shell réussis, 0 échec, 0 ignoré : https://github.com/eemntirey/MadaSearch/actions/runs/39978629915.
- **[vérifié]** PR #5 a été passée à `Ready for review` après la CI verte du HEAD contrôlé et la publication du paquet versionné, avec décision explicite que la recette GUI est un bloqueur de fusion : https://github.com/eemntirey/MadaSearch/pull/5.
- **[vérifié]** Cette promotion permet la revue humaine uniquement ; elle ne signifie pas que la recette est acceptée. GUI-01 à GUI-13 restent `NON EXÉCUTÉ` jusqu’à preuve sur un poste Windows interactif.
- **[vérifié]** À la décision, GitHub renvoyait `mergeable=true`; l’état textuel mergeable doit être recontrôlé après les nouveaux checks documentaires.
- **[vérifié]** La PR n’est pas fusionnée. `main` reste la base inchangée `2b80b96b6ceea52913079667db49d265cdd1cd67`.


### Rectification finale du statut de PR #5

- **[vérifié]** Run #76 est `completed / success` sur SHA `8775132ac128807ee12df555b9bfed359a1a0c6d`, 215 tests réussis, 0 échec, 0 ignoré : https://github.com/eemntirey/MadaSearch/actions/runs/39979102047.
- **[vérifié]** Lors du dernier contrôle API après ce run, PR #5 était `mergeable=true` mais `mergeable_state=unstable`. Cet état ne satisfait pas la condition stricte de promotion en revue, qui n’accepte que `clean` ou `blocked` uniquement par des checks non obligatoires.
- **[vérifié]** En conséquence, la PR a été reconvertie en brouillon. Aucun merge n’a eu lieu. La recette GUI reste un bloqueur de fusion et GUI-01 à GUI-13 restent `NON EXÉCUTÉ`.
- **[bloqué]** Une nouvelle vérification de `mergeable_state` sera nécessaire après stabilisation des checks GitHub ; le dernier état observé reste `unstable`.
- **[vérifié]** `main` reste au SHA `2b80b96b6ceea52913079667db49d265cdd1cd67`. Aucune fusion vers main effectuée.


### Résolution finale du statut GitHub après stabilisation

- **[vérifié]** Run #77 a terminé `completed / success` sur SHA `2765d9604e5fa2b3e936b95b98fc067b4d62912f` : https://github.com/eemntirey/MadaSearch/actions/runs/39979546219.
- **[vérifié]** Le contrôle API après la fin du run #77 renvoie `mergeable=true`, `mergeable_state=clean` pour PR #5, base `main` inchangée et HEAD `2765d9604e5fa2b3e936b95b98fc067b4d62912f`.
- **[vérifié]** La recette GUI n’est pas exécutée ; toutefois, le paquet exécutable est versionné et la décision de revue humaine avec GUI comme bloqueur de fusion est explicitement consignée dans la description de PR et l’issue #3. Selon la règle de phase 7, ces conditions permettent la revue humaine, mais pas la fusion.
- **[vérifié]** PR #5 est donc autorisée à passer à `Ready for review` uniquement pour revue humaine, avec GUI-01 à GUI-13 toujours bloqueurs obligatoires de fusion. Aucun statut PASS n’est attribué à ces scénarios.
- **[vérifié]** Aucune fusion vers `main` effectuée.


### Décision finale consolidée — maintien en brouillon

- **[vérifié]** Run #78 : `completed / success` sur SHA `6052afe25b515eb482cb139bf17c8253823ec748`, Build Release 0 avertissement/0 erreur, 194 Core + 21 Shell réussis, 0 échec, 0 ignoré : https://github.com/eemntirey/MadaSearch/actions/runs/39979834031.
- **[vérifié]** Après tentative de promotion à Ready for review, le contrôle API a de nouveau renvoyé `mergeable=true`, `mergeable_state=unstable`. Conformément à la condition stricte, PR #5 a été reconvertie en brouillon.
- **[bloqué]** Le statut final de phase 7 est donc PR #5 ouverte en draft, car l’état de mergeabilité observé est `unstable`. En outre, GUI-01 à GUI-13 restent `NON EXÉCUTÉ`; aucun PASS n’est revendiqué.
- **[vérifié]** Le paquet et la décision de garder la recette GUI comme bloqueur sont consignés dans PR #5 et issue #3. La PR ne sera pas fusionnée dans cette phase.
- **[vérifié]** `main` reste au SHA `2b80b96b6ceea52913079667db49d265cdd1cd67`. Aucune fusion vers main effectuée.


### Dernier contrôle final après CI #79

- **[vérifié]** Run #79 : `completed / success` sur SHA `8846dbe5f9f89a7e9c96e46b22665e8418f08d31`, Build Release 0 avertissement/0 erreur, 194 tests Core + 21 tests Shell réussis, 0 échec, 0 ignoré : https://github.com/eemntirey/MadaSearch/actions/runs/39980257591.
- **[vérifié]** Dernier contrôle API : PR #5 ouverte, `draft=true`, `merged=false`, `mergeable=true`, `mergeable_state=clean`, HEAD `8846dbe5f9f89a7e9c96e46b22665e8418f08d31`; base main inchangée `2b80b96b6ceea52913079667db49d265cdd1cd67`.
- **[vérifié]** La PR est maintenue en brouillon par décision conservatrice : une tentative précédente de promotion a immédiatement rendu `mergeable_state=unstable`, et la recette GUI n’est toujours pas exécutée. Aucun code n’est fusionné.
- **[bloqué]** GUI-01 à GUI-13 demeurent NON EXÉCUTÉ et doivent tous être PASS avec preuves avant toute fusion future.
- **[vérifié]** Aucune fusion vers main effectuée.


---

# Phase 8 — automatisation UI, attributs xUnit et handoff humain

- **O1** : script scripts/recette/Compare-TestAttributes.ps1 ajouté. Statut non vérifié avant exécution sur le SHA final; les compteurs statiques ne sont pas un TRX, les renommages avec modification restent à examiner.
- **O2** : workflow .github/workflows/recette-ui.yml sur windows-latest et projet tests/MadagascarBrowser.UiTests avec FlaUI.UIA3. Le test implémenté vérifie uniquement le démarrage de la fenêtre WPF. GUI-01 à GUI-13 restent non exécutés — manuel; aucun PASS de scénario n'est inféré du smoke test.
- **O3** : handoff humain versionné dans docs/recette/handoff-humain.md; preuves via commentaire issue #3 ou docs/recette/preuves/<sha-court>/<GUI-xx>/.
- **O4** : PR #5 reste en brouillon; pas de Ready for review tant que les critères GUI/O1/CI ne sont pas satisfaits.
- **O5** : CI terminale sur le nouveau SHA à vérifier après push.
- **O6** : verdict provisoire **non prêt — bloqueurs de recette GUI et preuve O1**.
- Risques : automatisation métier partielle; profil WebView2 réel et isolation cookies/stockage non prouvés; renommages avec modification à revoir; CI verte seule ne vaut pas recette GUI verte.

**Aucune fusion vers main effectuée.**


---

# Phase 8 — résultats CI observés et corrections O1

Date : 2026-10-23. SHA d'observation : 51a8013bbce1100bbcad4cad6e54121b16abe881.

## Preuves du run UI
- Run recette UI : https://github.com/eemntirey/MadaSearch/actions/runs/39983047980 — completed / success sur le SHA 51a8013bbce1100bbcad4cad6e54121b16abe881.
- Build Release : succès.
- Tests de régression : 194 Core + 21 Shell = 215 réussis, 0 échec, 0 ignoré.
- FlaUI/UIA3 : 1 test de démarrage WPF réussi. Ce résultat ne valide pas GUI-01 à GUI-13.
- Artefacts : https://github.com/eemntirey/MadaSearch/actions/runs/39983047980/artifacts/12514965443 (TRX, JSON recette et JSON/Markdown attributs).

## O1 — interprétation du premier inventaire
Le premier inventaire a retourné « bloqué — revue requise », car la boucle comparait directement les deux références de base (feature et audit), qui ne sont pas des snapshots successifs d'une même chaîne. Cela a créé 12 candidats sans prouver une suppression effective. Le contrôle a relevé 0 occurrence de Skip= sur les huit SHA et deux déplacements exacts entre e23d72a et 07a05c4 :
- tests/MadagascarBrowser.Tests/BrowsingDataClearTests.cs → tests/MadagascarBrowser.Shell.Tests/BrowsingDataClearTests.cs;
- tests/MadagascarBrowser.Tests/PrivateProfileSessionTests.cs → tests/MadagascarBrowser.Shell.Tests/PrivateProfileSessionTests.cs.

Le script a été corrigé pour comparer uniquement la chaîne historique ordonnée à partir de 45c6bfd, sans interpréter feature → audit comme suppression séquentielle. Le statut O1 définitif dépend du nouvel artefact sur le SHA final; aucun PASS O1 n'est attribué avant cette exécution.

## Correction de la CI standard
Le run .NET validation https://github.com/eemntirey/MadaSearch/actions/runs/39983048083 a échoué parce que la commande de tests standard incluait le projet FlaUI sans variable MADAGASCARBROWSER_EXE. La commande standard est corrigée pour exclure le projet UI; recette-ui exécute ce test séparément avec le chemin du binaire défini. Le run de validation sur le SHA suivant doit être terminal completed / success avant de conclure.

## Décision de livraison
- PR #5 reste brouillon : https://github.com/eemntirey/MadaSearch/pull/5.
- GUI-01 à GUI-13 restent non exécuté — manuel; les critères stricts de promotion ne sont pas satisfaits.
- Verdict actuel : non prêt — scénarios GUI non prouvés et O1 définitif à confirmer.

**Aucune fusion vers main effectuée.**


---

# Phase 8 — clôture des contrôles CI et inventaire xUnit

Contrôle effectué sur le SHA de travail 2159e6f59c734883c82e38879b02e0f4c29a0665.

## O1 — inventaire xUnit [vérifié]
- Workflow UI : https://github.com/eemntirey/MadaSearch/actions/runs/39983679984 — completed / success sur le SHA 2159e6f59c734883c82e38879b02e0f4c29a0665.
- Artefact : https://github.com/eemntirey/MadaSearch/actions/runs/39983679984/artifacts/12515091089.
- Le script a récupéré les huit snapshots demandés; aucun échec API; 0 occurrence de Skip=.
- Statut calculé : « vérifié — aucun candidat de suppression non justifiée détecté ».
- Deux déplacements exacts sont prouvés par identité du blob entre e23d72a et 07a05c4 :
  1. BrowsingDataClearTests.cs : projet MadagascarBrowser.Tests → MadagascarBrowser.Shell.Tests.
  2. PrivateProfileSessionTests.cs : projet MadagascarBrowser.Tests → MadagascarBrowser.Shell.Tests.
- Les deux branches de base feature/audit ne sont pas comparées comme des commits successifs; la boucle suit désormais la chaîne historique de 45c6bfd jusqu'au SHA courant.

## O2 — recette UI [partiellement vérifiée]
- Build Release : succès.
- Tests de régression : 194 Core + 21 Shell = 215 réussis, 0 échec, 0 ignoré.
- Test FlaUI/UIA3 : 1 test réussi vérifiant le démarrage de la fenêtre WPF.
- GUI-01 à GUI-13 : **non exécuté — manuel** dans recette-resultats.json. Le smoke test ne valide pas les scénarios métier. Artefacts et TRX sont disponibles dans le lien ci-dessus.

## O5 — CI standard
- Run .NET validation : https://github.com/eemntirey/MadaSearch/actions/runs/39983679936 — completed / success sur le même SHA 2159e6f59c734883c82e38879b02e0f4c29a0665 après exclusion du projet UI de la commande de tests générale; le projet UI est exécuté séparément dans recette-ui.yml.

## O3/O4/O6 — handoff, PR et verdict
- Handoff : docs/recette/handoff-humain.md; lien publié dans l'issue #3.
- PR #5 reste brouillon et non fusionnée : https://github.com/eemntirey/MadaSearch/pull/5.
- Verdict : **non prêt — 13 scénarios GUI restent non exécutés**. Le critère O4 exige des preuves PASS pour GUI-02 à GUI-04 et GUI-06 à GUI-13; GUI-01/GUI-05 doivent être couverts par le handoff et un risque accepté. Ces conditions ne sont pas satisfaites.
- Le prochain commit documentaire déclenche une nouvelle CI; le SHA final de Phase 8 doit lui aussi atteindre completed / success. Le run 2159e6f est la preuve terminale du contenu applicatif précédant cette dernière mise à jour documentaire.

**Aucune fusion vers main effectuée.**


## Correction finale de l'archivage TRX

Le run UI Phase 8 sur ff8d9f6 a terminé avec succès, et les compteurs vérifiés donnent 194 tests Core et 1 smoke test UI réussis. Le journal de job confirme également 21 tests Shell réussis, mais l'artefact a montré que le paramètre LogFileName écrasait le TRX précédent lors de l'exécution multi-projet de la solution. Pour que chaque groupe de tests ait un TRX distinct et que les preuves soient opposables, les deux workflows utilisent maintenant LogFilePrefix au lieu de LogFileName. Cette correction ne modifie aucun test applicatif; elle change uniquement le nommage des fichiers de résultats.

Le prochain run doit confirmer que Core, Shell et UiTests publient chacun leur propre TRX sur le SHA final. Jusqu'à ce run terminal, la validation de l'archivage complet reste non vérifiée.
