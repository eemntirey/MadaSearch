# Madagascar Browser Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (- [ ]) syntax for tracking.

**Goal:** Construire un navigateur web Windows (.NET 8 + WebView2) avec onglets, favoris, historique, blocage de pubs/économie de data, page d'accueil Madagascar, hors-ligne/sync local.

**Architecture:** 3 projets : MadagascarBrowser.Core (services purs), MadagascarBrowser (WPF + WebView2), MadagascarBrowser.Tests (xUnit). SQLite.

**Tech Stack:** .NET 8, WPF, Microsoft.Web.WebView2, Microsoft.Data.Sqlite, xUnit

**Spec:** docs/superpowers/specs/2026-10-07-madagascar-browser-design.md

## Global Constraints
- Windows, .NET 8, WebView2 >=153
- Pas de cloud, français, 100% local au démarrage
- git init en tâche 0, commits par task

---

### Task 0 : Spec + scaffold

Files: create spec + sln + Core + App + Tests

- [ ] git init (déjà fait) ; .gitignore bin/ obj/ .vs/
- [ ] dotnet new sln -n MadagascarBrowser ; dotnet new classlib -n MadagascarBrowser.Core -o src/MadagascarBrowser.Core ; dotnet new wpf -n MadagascarBrowser -o src/MadagascarBrowser ; dotnet new xunit -n MadagascarBrowser.Tests -o tests/MadagascarBrowser.Tests
- [ ] dotnet sln add src/MadagascarBrowser.Core/MadagascarBrowser.Core.csproj src/MadagascarBrowser/MadagascarBrowser.csproj tests/MadagascarBrowser.Tests/MadagascarBrowser.Tests.csproj
- [ ] references : App→Core ; Tests→Core
- [ ] packages : App + Microsoft.Web.WebView2 ; Core + Microsoft.Data.Sqlite ; Tests ok
- [ ] dotnet build → SUCCESS
- [ ] commit : feat: scaffold solution + spec

...
