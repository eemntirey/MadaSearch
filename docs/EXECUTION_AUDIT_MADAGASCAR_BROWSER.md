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
