# Compte-rendu — PHASE 0 (Blockers)

**Branche :** `mission/opencode` · **Worktree :** `C:\Users\eemntirey\Desktop\3HIsearch-mission`
**Base :** `8a92218` · **Commits phase :** `72e5d64` (0.1), `c6e0a04` (0.4)
**Build :** 0 erreur · **Tests :** 127 verts (122 base + 5 ajoutés 0.4)

## Vérifications manuelles (MadagascarBrowser01.exe, PID 12892)

| # | Objet | Résultat |
|---|-------|----------|
| 0.1 | Profil WebView2 forcé hors exécutable, deny-W respecté | **PASS** — profil dans `%LocalAppData%\MadagascarBrowser\WebView2`, aucun `.WebView2` résiduel à côté de l'exécutable |
| 0.2a | Cadenas : aucune connexion (page d'accueil / file) | **PASS** — "Aucune connexion réseau" |
| 0.2b | Cadenas : HTTPS | **PASS** — "Connexion chiffrée (HTTPS)" (`https://example.com/`, `https://mg.wikipedia.org/...`) |
| 0.2c | Cadenas : HTTP | **PASS** — "Connexion non chiffrée (HTTP)" (`http://info.cern.ch/`) |
| 0.2d | Cadenas : erreur certificat | **PASS** — "Erreur de certificat" (`https://expired.badssl.com/`) |
| 0.3a | `window.open` → nouvel onglet + focus | **PASS** |
| 0.3b | Lien `target=_blank` → nouvel onglet + focus | **PASS** |
| 0.4a | Recherche dans la barre d'adresse → Google | **PASS** — `python 3.11` → `https://www.google.mg/search?q=python%203.11` |
| 0.4b | `file:///...mhtml` hors ligne dans la barre d'adresse | **PASS** — MHTML 5 429 o chargée |
| 0.5 | Usage intensif sans erreur SQLite / crash | **PASS** — 5 navigations (HTTPS/HTTP/file), ouverture+fermeture d'onglets, commutations ×2 ; processus toujours "Responding", UI "Prêt", `PRAGMA integrity_check` = ok, WAL actif, 49 lignes d'historique cohérentes, suggestions toujours lisibles pendant écritures |

## Correctifs livrés

- **0.1** (`72e5d64`) : chemin de profil WebView2 unifié via `BrowserPaths` ; signalement des profils par défaut résiduels.
- **0.4** (`c6e0a04`) : `NavigationService.IsAbsoluteUrl` accepte `file://` ; ajout `LooksLikeRootedPath` (chemins `C:\` / `C:/` → `file://` ; `c: documents` reste une recherche) ; 5 tests ajoutés.

## Dette connue (explicite, hors PHASE 0)

1. **PHASE 2.4** — Pied de page "Navigation 100% locale · Antananarivo" faux (la recherche part vers google.mg). Visible en UI.
2. **Cadenas** — Indicateur HTTPS affiché brièvement pendant une navigation `file://` avant résolution finale ; état final correct.
3. **`mailto:`** — non couvert par le correctif 0.4 (hors périmètre blockers).
4. **Tests Shell** — pas encore de projet de tests WPF (cible PHASE 1 : `net8.0-windows` + `ITabHost` injecté).

## Porte de sortie PHASE 0

Tous les points 0.1 → 0.5 sont PASS. Aucun blocker ouvert. La PHASE 1 n'a pas été commencée.
