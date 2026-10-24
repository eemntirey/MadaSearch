# Note de version — 24 octobre 2026

Version de développement `feat/design-moderne`.

## Nouveautés visibles

- **Fenêtre de navigation privée** : ouverture depuis le menu, titre et badge
  « Navigation privée », profil WebView2 `InPrivate`. Aucune session ni entrée
  d'historique n'est enregistrée par cette fenêtre.
- **Effacement des données de navigation** : nouveau dialogue avec cookies,
  cache, historique, stockage local et mots de passe. L'historique SQLite est
  effacé uniquement si la case correspondante est cochée.
- **Téléchargements** : le nom proposé respecte `Content-Disposition`,
  notamment `filename*` (RFC 5987) et les doubles extensions (`.tar.gz`). Les
  téléchargements automatisés peuvent cibler un dossier via
  `MADAGASCAR_E2E_DOWNLOAD_DIR`.
- **Retour / suivant** : vérifiés sur un cycle réel de navigation, sans rejouer
  une page d'erreur.
- **Outils de développement** : accessibles par le menu ou `F12` dans les builds
  Debug. Le build public ne les expose pas.
- **Fenêtre À propos** : version de l'application, version réelle du runtime
  WebView2, canal (Evergreen/Fixed) et état de sécurité.
- **Bandeau runtime** : affiché si un runtime Fixed Version est antérieur à
  `130.0.0.0`, avec possibilité de le masquer.
- **Autofill et mots de passe locaux** : deux cases opt-in dans Paramètres,
  désactivées par défaut, sans synchronisation.
- **Options de démarrage** : `--private` et `--data-root <chemin>` pour isoler
  un profil de recette.

## Corrections

- Le nom de fichier téléchargé ne dépend plus de l'URL quand le serveur fournit
  `filename`/`filename*`.
- Les caractères non-ASCII des noms de fichiers sont décodés correctement.
- Le chemin fourni par l'en-tête ne peut plus sortir du dossier de destination.
- La navigation privée ne sauvegarde pas la session à la fermeture.

## Vérification

- Build : `dotnet build MadagascarBrowser.sln` — 0 erreur, 0 avertissement.
- Tests unitaires et Shell : 225/225.
- Recette GUI FlaUI : 7/7, sur exécutable réel avec profil temporaire.
- Captures : `docs/captures/e2e/` (navigation, historique, téléchargement,
  DevTools, fenêtre privée, effacement, À propos).

## Limites connues

- Pas de mise à jour automatique de l'application (dette ouverte).
- Pas de mise à jour distante des listes de blocage (dette ouverte).
- La recette E2E exige un poste Windows interactif ; elle n'est pas exécutable
  en CI headless.
- Le moteur, TLS, le réseau et le rendu restent ceux de WebView2/Chromium ;
  seule leur état est exposé.
- La traduction de pages et la galerie d'extensions restent des limites de
  plateforme, hors périmètre du produit.
