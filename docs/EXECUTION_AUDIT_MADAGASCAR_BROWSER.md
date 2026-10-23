# Rapport d’exécution — audit de MadagascarBrowser

Date : 2026-10-23  
Branche de travail : `fix/audit-madagascar-browser`  
Dernier commit de code validé par CI au moment de la rédaction : `22415efd7eb5d75a992abd7ca230394441c285a6`

## 1. Résumé

Les cinq lots de correction sont présents sur la branche d’audit. Deux corrections complémentaires ont été ajoutées pendant cette reprise :

- l’effacement WebView2 traite désormais séparément l’historique, les données de site et le cache ;
- la récupération des téléchargements ne considère plus un fichier préexistant comme terminé sur la seule base de sa taille.

Aucune fusion vers `main` n’a été effectuée.

## 2. Détail des lots

### P0.1 — Canal `WebMessageReceived`

Fichiers principaux :
- `src/MadagascarBrowser.Core/Security/NativeWebMessagePolicy.cs`
- `src/MadagascarBrowser/Shell/BrowserView.xaml.cs`
- `tests/MadagascarBrowser.Tests/NativeWebMessagePolicyTests.cs`

La politique échoue de manière fermée : origine HTTPS du seul hôte virtuel `startpage.localhost`, absence d’identifiants intégrés et de port non standard, liste blanche stricte des commandes `t`, `w`, `l`. Les messages inconnus et les charges non textuelles sont ignorés ; le journal ne contient ni URL ni contenu du message. Le pont JavaScript limite également son usage à un événement clavier utilisateur de premier niveau.

Tests ciblés : commandes autorisées, origines distantes/non sûres, URL avec identifiants, port non standard, payloads et messages inconnus.

### P0.2 — Navigation privée

Fichiers principaux :
- `src/MadagascarBrowser/Shell/PrivateProfileSession.cs`
- `src/MadagascarBrowser/Shell/WebViewEnvironment.cs`
- `src/MadagascarBrowser/Shell/TabManager.cs`
- `src/MadagascarBrowser/Shell/TabViewModel.cs`
- `src/MadagascarBrowser/MainWindow.xaml.cs`
- `src/MadagascarBrowser.Core/Security/PrivateBrowsingPolicy.cs`
- tests `PrivateProfileSessionTests.cs` et `PrivateBrowsingPolicyTests.cs`

Chaque fenêtre privée reçoit un dossier WebView2 distinct. Un verrou exclusif distingue les profils actifs des profils abandonnés ; les vues sont disposées avant la fermeture de la session de profil. Le nettoyage normal supprime le profil temporaire ; au prochain démarrage, les profils orphelins sont réessayés sans toucher aux dossiers qui restent verrouillés.

La politique privée interdit la persistance de l’historique, des favoris, des pages hors ligne, des métadonnées de téléchargement et de la session normale. Les fichiers explicitement enregistrés par l’utilisateur ne sont pas supprimés par le nettoyage du profil.

Limite : les tests automatisés couvrent les chemins de profil, le verrouillage/nettoyage et les règles de persistance. L’isolation effective de cookies/stockage par deux instances WebView2 et le nettoyage après un véritable arrêt forcé nécessitent encore la recette Windows décrite plus bas.

### P1 — Sessions et téléchargements

Fichiers principaux :
- `src/MadagascarBrowser/MainWindow.xaml.cs`
- `src/MadagascarBrowser.Core/Data/BrowserDownload.cs`
- `src/MadagascarBrowser.Core/Data/DownloadService.cs`
- `src/MadagascarBrowser/Shell/BrowserServices.cs`
- tests `SessionServiceTests.cs` et `DownloadServiceTests.cs`

La session normale est sauvegardée toutes les 15 secondes et à la fermeture normale. Les URL et l’index de l’onglet actif sont conservés ; les URL non HTTP(S) ne sont pas restaurées comme pages web.

Au démarrage, les téléchargements restés `InProgress` sont réconciliés. Un fichier n’est considéré comme complet que si sa taille correspond exactement à la taille attendue et si sa date de modification n’est pas antérieure au démarrage enregistré du téléchargement. Dans les autres cas, l’état devient `Interrupted`. Aucun fichier utilisateur n’est supprimé par cette récupération.

Tests ciblés : snapshot relu par une nouvelle instance de service, fichier partiel conservé et marqué interrompu, fichier préexistant conservé sans être faussement déclaré terminé, fichier complet simulé reconnu.

### P1 — Effacement sélectif des données

Fichiers principaux :
- `src/MadagascarBrowser/Shell/SettingsView.xaml`
- `src/MadagascarBrowser/Shell/SettingsView.xaml.cs`
- `src/MadagascarBrowser/Shell/TabManager.cs`
- `src/MadagascarBrowser/Shell/BrowserView.xaml.cs`
- `tests/MadagascarBrowser.Tests/BrowsingDataClearTests.cs`
- `tests/MadagascarBrowser.Tests/HistoryServiceTests.cs`

L’interface exige une confirmation et permet de sélectionner indépendamment :
- l’historique de navigation (historique WebView2 et historique SQLite) ;
- les cookies et données de site (catégorie WebView2 `AllSite`) ;
- le cache disque WebView2.

L’historique SQLite est relu après effacement pour vérifier qu’il est vide. L’appel asynchrone de WebView2 est attendu ; si l’opération échoue, l’interface signale un effacement incomplet. Les favoris, téléchargements et paramètres ne sont pas supprimés par ces catégories.

Tests ciblés : correspondance des catégories WebView2, sélection vide, catégories isolées, combinaison des catégories et non-régression garantissant que vider l’historique ne supprime pas les favoris.

Limite : WebView2 confirme la fin de l’opération, mais ne fournit pas de requête globale permettant de relire indépendamment toutes les données de site/cache après effacement. La vérification indépendante automatisée porte sur l’historique SQLite ; la recette manuelle doit compléter le contrôle des cookies et du cache.

## 3. Build et tests automatisés

Workflow : [`.github/workflows/dotnet.yml`](../.github/workflows/dotnet.yml) sur un runner Windows.

Commande exécutée par CI :
```powershell
dotnet restore MadagascarBrowser.sln
dotnet build MadagascarBrowser.sln --configuration Release --no-restore
dotnet test MadagascarBrowser.sln --configuration Release --no-build --logger "trx;LogFileName=test-results.trx"
```

Résultat réel sur le commit `22415efd7eb5d75a992abd7ca230394441c285a6` :
- Build Release : réussi, 0 avertissement, 0 erreur.
- Tests : 118 réussis, 0 échec, 0 ignoré.
- Run GitHub Actions : https://github.com/eemntirey/MadaSearch/actions/runs/39965989504

Le checkout local n’est pas présent dans l’environnement d’exécution de cette reprise ; le clonage direct a échoué car la résolution DNS de `github.com` est indisponible. Le build et les tests ont donc été exécutés par le runner Windows de GitHub Actions, pas localement.

## 4. Réconciliation Git

Références vérifiées :
- `main` : `2b80b96b6ceea52913079667db49d265cdd1cd67`
- `feat/design-moderne` : `357f4ed5e84383fa0a42802f69eecec74b9e8e5d`
- `fix/audit-madagascar-browser` avant les derniers correctifs : `a8662f79fbb542146f4aa5acf958733f4c5fc855`
- Correctif effacement sélectif : `8397968fc87824bd2a7351d5f86f90ed58fa19ac`
- Correctif récupération téléchargements : `22415efd7eb5d75a992abd7ca230394441c285a6`

`feat/design-moderne` est 42 commits devant `main`. La branche d’audit a également été construite à partir de `main`, et non de `feat/design-moderne`. Les deux branches ont des modifications concurrentes sur 12 fichiers. Une PR d’intégration exploratoire (#1) a été créée puis fermée sans fusion après que GitHub n’a pas confirmé une fusion propre ; elle n’a touché ni `main` ni la branche de fonctionnalité.

La branche d’audit conserve donc ses corrections et sa CI verte, mais elle ne contient pas automatiquement les 42 commits propres à `feat/design-moderne`. Il ne faut pas présenter cette branche comme une fusion intégrée de la version livrable de la fonctionnalité. La réconciliation finale des deux lignes doit se faire avec une résolution de conflits et une CI complète sur le résultat combiné.

## 5. Recette manuelle Windows/WebView2

À exécuter sur un poste Windows avec le runtime WebView2 installé. Cocher chaque ligne uniquement après observation réelle.

- [ ] Démarrage installé sous `Program Files`, page de démarrage et navigation HTTP(S).
- [ ] Site externe : tentative d’envoi de messages natifs non autorisés ; aucune commande d’interface déclenchée.
- [ ] Raccourcis `Ctrl+T`, `Ctrl+W`, `Ctrl+L` depuis la page d’accueil.
- [ ] Télécharger un fichier avec un nom fourni par `Content-Disposition`, puis annuler un téléchargement.
- [ ] Mode privé : ouvrir une page qui pose un cookie, vérifier que le profil normal ne le reçoit pas ; visiter des pages puis vérifier que l’historique normal reste inchangé.
- [ ] Fermer normalement la fenêtre privée ; vérifier que son dossier temporaire disparaît.
- [ ] Arrêter de force le processus avec un profil privé actif ; relancer le navigateur et vérifier le nettoyage du profil orphelin sans suppression de fichiers de téléchargement.
- [ ] Effacer uniquement l’historique ; vérifier que favoris, téléchargements et paramètres restent présents.
- [ ] Effacer uniquement les cookies/données de site ; vérifier le déconnexion des sites et la conservation de l’historique/favoris.
- [ ] Effacer uniquement le cache ; vérifier le rechargement des ressources sans perte des autres catégories.
- [ ] Ouvrir plusieurs onglets, laisser passer au moins 15 secondes, tuer le processus, relancer et vérifier l’ordre et l’onglet actif restaurés.
- [ ] Vérifier qu’un téléchargement partiel est affiché comme interrompu après redémarrage et qu’un fichier complet n’est jamais supprimé.
- [ ] Tester un certificat invalide et une coupure réseau.

Aucune de ces vérifications GUI n’a été exécutée par le runner CI ; elles restent explicitement à faire.

## 6. Risques restants

1. Réconciliation `feat/design-moderne` / `fix/audit-madagascar-browser` non terminée : divergence et modifications concurrentes sur 12 fichiers.
2. Recette réelle WebView2 non exécutée : isolation cookies/stockage, fermeture forcée, certificat invalide, coupure réseau et rendu GUI.
3. La vérification des données WebView2 après effacement repose sur la fin réussie de l’API ; la vérification post-opération indépendante est limitée par l’API disponible.
4. Les snapshots périodiques limitent la perte de session à la fenêtre entre deux snapshots (15 secondes), sans garantir la récupération d’une navigation en cours au moment du crash.

## 7. Fusion

**Aucune fusion vers `main` effectuée.**  
La CI Windows du commit de code cité ci-dessus est verte. Le prochain contrôle de livraison devra porter sur la branche fusionnée qui réconcilie les 42 commits de `feat/design-moderne` avec les correctifs de cette branche, puis sur la recette manuelle Windows/WebView2.
