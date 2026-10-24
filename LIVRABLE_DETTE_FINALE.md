# Registre de dette — Madagascar Browser (brouillon évolutif pour la porte 6)

Chaque entrée : impact utilisateur + contournement. Finalisé à la porte 6.4, qui
exigera que la section ne contienne **que** du hors-périmètre ou du report décidé.

## Dette active

| # | Point | Impact utilisateur | Contournement | Résolution |
|---|---|---|---|---|
| D1 | Navigation privée absente (1.5 / 4.1) | Historique, cookies et cache conservés après navigation « normale » | Aucun | Décision produit : implémenter (environnement WebView2 isolé) ou dette finale assumée |
| D2 | Cookies et cache non effaçables (1.6 / 4.2) | Données de navigation non purgeables depuis l'interface | Suppression manuelle du dossier `%LocalAppData%\MadagascarBrowser\WebView2` | 4.2 ou dette |
| D3 | Sauvegarde de session sur fermeture normale uniquement (1.10 / 4.5) | Après un plantage/kill, les onglets de la session perdue sont perdus | Fermer proprement ; activer « Restaurer les onglets au démarrage » (réglage actuellement désactivé) | 4.5 (snapshot périodique) ou dette |
| D4 | Onglets : défilement horizontal désactivé, pas de favicon, pas de bouton Stop (4.6) | Au-delà de ~10 onglets, navigation difficile ; pas de repère visuel de chargement par onglet | Fermer des onglets ; Reload sert aussi d'indicateur global | 4.6 ou dette |
| D5 | Autofill / mots de passe non activés (4.7) | Aucune saisie automatique | Saisie manuelle | Volontaire : conditionné à 4.1 (absente) — conforme à la consigne, non une dette de mise en œuvre |
| D6 | Titre d'onglet MHTML = nom de fichier brut (`<guid>.mhtml`) (observé parcours 1.2) | Onglet hors ligne peu lisible | Lire le contenu de l'onglet | Rattaché à 4.5 (restauration des pages hors ligne) |
| D7 | Fonctionnalité hors plan : bouton Épingler + déplacement de la barre d'onglets (commit `8a0dc6a`) | Aucun (ajout non demandé) | — | À confirmer (garder) ou retirer avant livraison : l'ordre de mission interdit le hors-périmètre |
| D8 | Rotation du fichier de journal non testée à l'échelle (1 Mo) | Perte théorique d'anciennes logs si le rotationnement échoue | Aucune (les logs réelles sont petites) | `AppLogTests` couvre l'écriture seule ; extension de test possible, faible priorité |
| D9 | Instances multiples non verrouillées (profil SQLite/WebView2 partagé) | Deux instances concurrentes non testées en contention (deux instances ont tourné sans erreur observable) | N'utiliser qu'une instance | Hors périmètre de l'ordre ; laissé tel quel |
| D10 | Parcours de recette non encore exécutés (1.2 complet, 1.4→1.11) | Zone non vérifiée tant que la session manuelle n'est pas faite | Feuille dans `LIVRABLE_PHASE_1_PARCOURS.md` | En attente de la session de recette |
| D11 | Travail 4.4 non committé au moment du relevé (session parallèle) | Risque de perte si non committé | — | Commit du travail en cours avant la porte 6 |

## Dettes résolues pendant la mission (tracabilité)

- Exception `KeyNotFoundException` d'interception DataSaver → corrigé (`b6d7ee6`).
- Exceptions globales non tracées → handlers + lignes Démarrage/Fermeture (`af04a21`).
- Avertissements de nullabilité CS8622/CS8767 → 0 en build complet (`81c2e10`).
- Certificat erroné accepté via l'interstitiel WebView2 → rejet explicite (`b48f620`).
- Fichiers mal formattés (HistoryService, AssemblyInfo) → reformattés (`afcf0ae`).
- Profil WebView2 par défaut à côté de l'exécutable → unifié sur `BrowserPaths` (`ca31727`).
- Course d'annulation dans `AsyncDebouncer` → corrigé (`4fb6eca`).
