# Recette manuelle et opposable — MadagascarBrowser / MadaSearch

**Statut initial : NON EXÉCUTÉ.** Ne cochez PASS que sur observation réelle. Une commande réussie ne prouve pas le comportement graphique.

## Fiche de session
- SHA testé :
- Date/heure ISO 8601 :
- Windows édition/version/build :
- .NET Desktop Runtime :
- WebView2 Runtime :
- Binaire testé (chemin + SHA-256) :
- Profil de test :
- Opérateur :
- Dossier de preuves :
- Résultat global : NON EXÉCUTÉ

Pour chaque scénario, renseigner observé, PASS/FAIL/BLOQUÉ/NON EXÉCUTÉ, SHA, versions, capture, journal et défaut éventuel. Ne jamais inclure cookies, mots de passe ou données personnelles.

## Préparation commune
1. Utiliser Windows 10/11 de test, .NET 8 Desktop Runtime, WebView2 Evergreen Runtime et un compte de test.
2. Fermer les instances. Ne jamais utiliser le profil quotidien.
3. Créer un dossier de preuves artifacts/recette/<date>-<sha-court>.
4. Lancer scripts/recette/run-all.ps1 -AppPath <chemin-vers-exe> -EvidenceRoot <dossier-preuves>.
5. Enregistrer versions, SHA, journaux et captures PNG montrant l’action et le résultat.

## GUI-01 — Navigation et indicateur de sécurité
Prérequis : application et profil vierge. Étapes : ouvrir un site HTTPS valide, naviguer vers un second site, revenir/avancer, puis ouvrir un site HTTP. Attendu : navigation cohérente et indicateur de sécurité non trompeur.
Observé : ___ | Statut : NON EXÉCUTÉ | SHA : ___ | Versions : ___ | Preuves : ___

## GUI-02 — Téléchargement terminé
Étapes : télécharger un fichier de test inoffensif, attendre la fin, comparer taille/date à la source, inspecter la liste. Attendu : état terminé, fichier complet, taille exacte et date cohérente.
Observé : ___ | Statut : NON EXÉCUTÉ | SHA : ___ | Versions : ___ | Preuves : ___

## GUI-03 — Téléchargement annulé
Étapes : commencer un téléchargement volumineux de test et l’annuler depuis l’interface. Attendu : état annulé explicite, aucun état terminé mensonger, comportement du fichier partiel documenté.
Observé : ___ | Statut : NON EXÉCUTÉ | SHA : ___ | Versions : ___ | Preuves : ___

## GUI-04 — Téléchargement interrompu et récupération
Étapes : démarrer un téléchargement puis terminer le processus pendant le transfert ; relancer et consulter les téléchargements. Attendu : état Interrupted, jamais présenté comme terminé.
Observé : ___ | Statut : NON EXÉCUTÉ | SHA : ___ | Versions : ___ | Preuves : ___

## GUI-05 — Isolation privée
Étapes : créer un marqueur cookie/localStorage non sensible en session normale ; ouvrir une fenêtre privée et vérifier son absence ; créer un marqueur privé, fermer la fenêtre et vérifier qu’il n’est pas dans le profil normal. Attendu : cookies, cache et stockage isolés.
Observé : ___ | Statut : NON EXÉCUTÉ | SHA : ___ | Versions : ___ | Preuves : ___

## GUI-06 — Nettoyage privé après fermeture normale
Étapes : ouvrir session privée, noter le répertoire temporaire réel, fermer normalement toutes les fenêtres privées et inspecter le disque. Attendu : profil temporaire supprimé selon le contrat.
Observé : ___ | Statut : NON EXÉCUTÉ | SHA : ___ | Versions : ___ | Preuves : ___

## GUI-07 — Nettoyage après fermeture forcée / profil orphelin
Étapes : ouvrir session privée, relever son répertoire, terminer le processus, relancer et vérifier le nettoyage sans toucher au profil normal. Attendu : aucun profil privé orphelin réutilisé.
Observé : ___ | Statut : NON EXÉCUTÉ | SHA : ___ | Versions : ___ | Preuves : ___

## GUI-08 — Effacement de l’historique seul
Étapes : créer une entrée, effacer uniquement l’historique, vérifier l’historique et la conservation cookies/cache/favoris. Attendu : seule la catégorie demandée est effacée.
Observé : ___ | Statut : NON EXÉCUTÉ | SHA : ___ | Versions : ___ | Preuves : ___

## GUI-09 — Effacement cookies et données de site seuls
Étapes : créer un cookie de test et une entrée d’historique, effacer uniquement cookies/données de site, vérifier chaque catégorie. Attendu : cookies supprimés, historique et favoris conservés.
Observé : ___ | Statut : NON EXÉCUTÉ | SHA : ___ | Versions : ___ | Preuves : ___

## GUI-10 — Effacement du cache seul
Étapes : charger une ressource, effacer uniquement le cache, contrôler cookies, historique et favoris. Attendu : seule la catégorie ciblée est effacée.
Observé : ___ | Statut : NON EXÉCUTÉ | SHA : ___ | Versions : ___ | Preuves : ___

## GUI-11 — Annulation de confirmation
Étapes : sélectionner une catégorie d’effacement puis annuler la confirmation ; vérifier toutes les catégories. Attendu : aucune donnée supprimée.
Observé : ___ | Statut : NON EXÉCUTÉ | SHA : ___ | Versions : ___ | Preuves : ___

## GUI-12 — Restauration après arrêt brutal
Étapes : ouvrir plusieurs onglets non sensibles, noter les URL, terminer le processus sans fermeture normale, relancer. Attendu : restauration conforme, sans corruption, doublons ni exposition d’une session privée.
Observé : ___ | Statut : NON EXÉCUTÉ | SHA : ___ | Versions : ___ | Preuves : ___

## GUI-13 — Certificat invalide
Prérequis : script Test-InvalidCertificate.ps1 et port libre. Étapes : démarrer le serveur TLS local, naviguer vers https://localhost:18443/, ne jamais contourner l’avertissement. Attendu : certificat invalide détecté, aucune présentation comme connexion sûre.
Observé : ___ | Statut : NON EXÉCUTÉ | SHA : ___ | Versions : ___ | Preuves : ___

## Critères de clôture
PASS seulement après observation et preuve. FAIL si l’attendu est contredit. BLOQUÉ si un prérequis empêche le test. NON EXÉCUTÉ si aucune observation n’a eu lieu. La recette complète exige GUI-01 à GUI-13 tous PASS et les preuves archivées.