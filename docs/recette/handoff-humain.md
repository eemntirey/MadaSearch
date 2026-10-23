# Handoff humain — recette MadagascarBrowser

Statut initial : non exécuté. Responsable : mainteneur Windows. Tester le SHA exact du run Phase 8 sous Windows 10/11 avec .NET 8 Desktop Runtime et WebView2 Evergreen Runtime; consigner versions, SHA-256 binaire, date UTC et profil de test vierge. Déposer les preuves dans le commentaire de https://github.com/eemntirey/MadaSearch/issues/3 ou sous docs/recette/preuves/<sha-court>/<GUI-xx>/. Aucun secret ni cookie ne doit être publié.

- **GUI-01 Sécurité** : ouvrir HTTPS valide puis HTTP; vérifier que HTTP n'est jamais marqué sécurisé. Preuves : captures des deux états, log.
- **GUI-02 Téléchargement terminé** : télécharger un fichier de taille connue; vérifier présence, taille exacte, état UI. Preuves : capture, taille/SHA-256, log, JSON.
- **GUI-03 Annulation** : lancer téléchargement lent et annuler; vérifier état annulé et qu'aucun fichier partiel n'est marqué complet. Preuves : captures, listing, log, JSON.
- **GUI-04 Interruption** : lancer téléchargement lent, tuer le processus, relancer; vérifier état Interrupted en UI/DB lecture seule. Preuves : captures, logs, export DB expurgé.
- **GUI-05 Isolation privée** : créer cookie/localStorage non sensible en normal et privé; inspecter chemins/cache après fermeture. Succès : aucune donnée privée accessible au profil normal. Preuves : captures, chemins observés, export SQLite expurgé.
- **GUI-06 Nettoyage normal** : noter chemin du profil privé, fermer normalement, vérifier suppression du seul dossier temporaire. Preuves : inventaire avant/après, capture, log.
- **GUI-07 Nettoyage forcé** : tuer avec Stop-Process -Force, relancer, vérifier nettoyage du profil orphelin sans toucher au profil normal. Preuves : logs, captures, inventaire.
- **GUI-08 Historique seul** : créer historique/cookie, effacer historique; vérifier conservation cookies/cache/favoris. Preuves : capture, SQLite lecture seule, log.
- **GUI-09 Cookies seuls** : créer cookie/historique, effacer données de site; cookie absent, historique/favoris conservés. Preuves : capture, vérification locale.
- **GUI-10 Cache seul** : charger ressource, effacer cache; vérifier conservation cookies/historique/favoris. Preuves : capture, inspection cache lecture seule.
- **GUI-11 Annulation confirmation** : lancer effacement puis annuler; vérifier aucune donnée supprimée. Preuves : capture dialogue/annulation, vérification locale.
- **GUI-12 Restauration** : ouvrir plusieurs onglets, noter ordre/actif, tuer processus et relancer; vérifier restauration sans doublon ni exposition privée. Preuves : captures avant/après, log.
- **GUI-13 Certificat invalide** : ouvrir https://localhost:18443/ avec certificat auto-signé non approuvé, sans contourner l'erreur; vérifier que la connexion n'est pas marquée sûre. Preuves : capture, logs serveur/navigateur.

Chaque PASS doit référencer PNG, journal, entrée recette-resultats.json et TRX si applicable; sans preuves, garder non vérifié.