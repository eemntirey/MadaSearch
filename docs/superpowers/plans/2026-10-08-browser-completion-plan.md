# Madagascar Browser Completion Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax.

**Goal:** Complete the local-first Madagascar Browser daily-use workflows without replacing its WPF/WebView2/SQLite stack.

**Architecture:** Core services own persisted settings, library data, downloads, session records, offline snapshot metadata/files, and profile validation. The WPF shell owns controls, native dialogs, WebView2 event wiring, and startup/shutdown integration. SQLite changes are additive; existing history and favorites remain intact.

**Tech Stack:** .NET 8, WPF, WebView2 1.0.2849.39, Microsoft.Data.Sqlite, xUnit.

**Spec:** `docs/superpowers/specs/2026-10-08-browser-completion-design.md`

## Global Constraints

- Windows only; .NET 8; WebView2 1.0.2849.39.
- No new NuGet dependencies, cloud services, or password storage.
- Existing `%LocalAppData%\\MadagascarBrowser\\browser.db` history and favorites must survive upgrades.
- Core remains free of WPF and WebView2 references.
- Test command: `& "$env:LOCALAPPDATA\\Microsoft\\dotnet\\dotnet.exe" test MadagascarBrowser.sln`.

---

### Task 1: Persistent settings and controls

**Files:**
- Modify: `src/MadagascarBrowser.Core/Data/Database.cs`
- Create: `src/MadagascarBrowser.Core/Data/BrowserSettings.cs`
- Create: `src/MadagascarBrowser.Core/Data/BrowserSettingsService.cs`
- Modify: `src/MadagascarBrowser.Core/Navigation/NavigationService.cs`
- Modify: `src/MadagascarBrowser.Core/Blocking/AdBlockService.cs`
- Test: `tests/MadagascarBrowser.Tests/BrowserSettingsServiceTests.cs`
- Test: `tests/MadagascarBrowser.Tests/NavigationServiceTests.cs`
- Test: `tests/MadagascarBrowser.Tests/AdBlockServiceTests.cs`
- Modify: `src/MadagascarBrowser/Shell/BrowserServices.cs`
- Modify: `src/MadagascarBrowser/Shell/BrowserView.xaml`
- Modify: `src/MadagascarBrowser/Shell/BrowserView.xaml.cs`
- Create: `src/MadagascarBrowser/SettingsWindow.xaml`
- Create: `src/MadagascarBrowser/SettingsWindow.xaml.cs`

**Interfaces:**
- `BrowserSettings` holds `SearchProvider`, `AdBlockEnabled`, `DataSaverEnabled`, and `RestoreSession`.
- `BrowserSettingsService(Database).Load() : BrowserSettings` and `.Save(BrowserSettings) : void`.
- `NavigationService.ResolveNavigation(string input, string searchUrlTemplate)` keeps the existing one-argument overload behavior.
- `AdBlockService.Enabled` defaults to `true`; `ShouldBlock(Uri)` returns false when disabled.

- [x] Write tests for defaults, persistence, selected search provider, and disabled ad blocking.
- [x] Run the focused tests and verify they fail for the missing behavior.
- [x] Add the settings table with `CREATE TABLE IF NOT EXISTS`; implement the Core settings service and configurable navigation/blocking.
- [x] Add persistent UI controls for search provider, ad blocking, data saver, and restore-session preference; apply loaded values before navigation.
- [x] Run focused tests, then the full solution tests and build.

### Task 2: Bookmark and history management

**Files:**
- Modify: `src/MadagascarBrowser.Core/Data/HistoryService.cs`
- Test: `tests/MadagascarBrowser.Tests/HistoryServiceTests.cs`
- Create: `src/MadagascarBrowser/LibraryWindow.xaml`
- Create: `src/MadagascarBrowser/LibraryWindow.xaml.cs`
- Modify: `src/MadagascarBrowser/Shell/BrowserView.xaml`
- Modify: `src/MadagascarBrowser/Shell/BrowserView.xaml.cs`

**Interfaces:**
- `HistoryService.Remove(string url) : void` removes visits for that URL; `Clear() : void` removes all visits.
- Existing `FavoritesService.Add`, `.Remove`, and `.GetAll` remain the persistence API.
- `LibraryWindow` provides searchable History and Favorites views, open, remove, and clear-history actions.

- [x] Add tests proving history removal is URL-scoped and clear removes all rows.
- [x] Run focused history tests and verify they fail.
- [x] Implement `Remove` and `Clear` with parameterized SQLite commands.
- [x] Add a bookmark action for the current HTTP(S) page and a library window for bookmark/history operations with clear-history confirmation.
- [x] Run focused tests, full tests, and the WPF build.

### Task 3: Download management

**Files:**
- Modify: `src/MadagascarBrowser.Core/Data/Database.cs`
- Create: `src/MadagascarBrowser.Core/Data/BrowserDownload.cs`
- Create: `src/MadagascarBrowser.Core/Data/DownloadService.cs`
- Test: `tests/MadagascarBrowser.Tests/DownloadServiceTests.cs`
- Modify: `src/MadagascarBrowser/Shell/BrowserServices.cs`
- Modify: `src/MadagascarBrowser/Shell/BrowserView.xaml.cs`
- Create: `src/MadagascarBrowser/DownloadsWindow.xaml`
- Create: `src/MadagascarBrowser/DownloadsWindow.xaml.cs`

**Interfaces:**
- `BrowserDownload` records ID, source URL, destination, received bytes, total bytes, state, and start time.
- `DownloadService(Database).Start(...)`, `.UpdateProgress(...)`, `.Complete(...)`, `.Fail(...)`, `.Cancel(...)`, and `.GetRecent(int limit)` persist download lifecycle.
- WebView2 `DownloadStarting` uses `SaveFileDialog`, persists operation state, and updates progress/completion/cancellation.

- [x] Add tests for starting, progress updates, terminal states, and recent-download order.
- [x] Run focused tests and verify they fail.
- [x] Add an idempotent downloads table and implement the Core lifecycle service.
- [x] Wire WebView2 downloads to Save As and the service; add a window showing progress/state, cancellation, and opening completed files through the OS shell.
- [x] Run focused tests, full tests, and the WPF build.

### Task 4: Session restoration

**Files:**
- Modify: `src/MadagascarBrowser.Core/Data/Database.cs`
- Create: `src/MadagascarBrowser.Core/Data/BrowserSession.cs`
- Create: `src/MadagascarBrowser.Core/Data/SessionService.cs`
- Test: `tests/MadagascarBrowser.Tests/SessionServiceTests.cs`
- Modify: `src/MadagascarBrowser/Shell/BrowserServices.cs`
- Modify: `src/MadagascarBrowser/Shell/TabManager.cs`
- Modify: `src/MadagascarBrowser/MainWindow.xaml.cs`

**Interfaces:**
- `BrowserSession` contains ordered URLs and active-tab index.
- `SessionService(Database).Save(BrowserSession) : void` and `.Load() : BrowserSession` persist and restore ordering.
- `TabManager.RestoreSession(BrowserSession)` creates tabs in order and activates the saved index.

- [x] Add tests for order, active index clamping, and empty sessions.
- [x] Run focused tests and verify they fail.
- [x] Add session persistence and methods to collect/restore valid HTTP(S) tabs.
- [x] Save during window close; restore at startup only when the persisted preference is enabled.
- [x] Run focused tests, full tests, and the WPF build.

### Task 5: Explicit offline page saving

**Files:**
- Modify: `src/MadagascarBrowser.Core/Data/Database.cs`
- Create: `src/MadagascarBrowser.Core/Data/OfflinePage.cs`
- Create: `src/MadagascarBrowser.Core/Data/OfflinePageService.cs`
- Test: `tests/MadagascarBrowser.Tests/OfflinePageServiceTests.cs`
- Modify: `src/MadagascarBrowser/Shell/BrowserServices.cs`
- Modify: `src/MadagascarBrowser/LibraryWindow.xaml`
- Modify: `src/MadagascarBrowser/LibraryWindow.xaml.cs`
- Modify: `src/MadagascarBrowser/Shell/BrowserView.xaml`
- Modify: `src/MadagascarBrowser/Shell/BrowserView.xaml.cs`

**Interfaces:**
- `OfflinePageService(Database, string storageDirectory).Save(string url, string title, string mhtml) : OfflinePage`, `.GetAll()`, `.Remove(string id)`, and `.GetPath(string id)` store MHTML atomically with SQLite metadata.
- The shell captures the current page with WebView2's `Page.captureSnapshot` DevTools method and opens selected saved snapshots as local files.

- [x] Add tests for save/list/remove, atomic file creation, and metadata/file cleanup.
- [x] Run focused tests and verify they fail.
- [x] Implement the Core file/metadata lifecycle without WPF dependencies.
- [x] Add Save Offline and Saved Pages actions; report capture/write failures without losing existing snapshots.
- [x] Run focused tests, full tests, and the WPF build.

### Task 6: Local profile export/import

**Files:**
- Create: `src/MadagascarBrowser.Core/Data/BrowserProfileService.cs`
- Test: `tests/MadagascarBrowser.Tests/BrowserProfileServiceTests.cs`
- Modify: `src/MadagascarBrowser/Shell/BrowserServices.cs`
- Modify: `src/MadagascarBrowser/Shell/BrowserView.xaml`
- Modify: `src/MadagascarBrowser/Shell/BrowserView.xaml.cs`

**Interfaces:**
- `BrowserProfileService(Database, BrowserSettingsService).Export(string path) : void` and `.Import(string path) : void` serialize versioned JSON containing settings, bookmarks, and history only.
- Import validates the complete document before writing and uses a transaction so invalid input cannot partially change the profile.

- [x] Add round-trip and invalid-document tests; verify invalid imports preserve existing profile data.
- [x] Run focused tests and verify they fail.
- [x] Implement versioned JSON DTOs and transactional import/export.
- [x] Add native Open/Save dialogs for profile actions.
- [x] Run focused tests and the complete suite/build.

### Task 7: Final verification

**Files:** No planned production changes.

- [x] Re-read the spec and check every functional requirement against the implementation.
- [x] Run `& "$env:LOCALAPPDATA\\Microsoft\\dotnet\\dotnet.exe" test MadagascarBrowser.sln` (24 passed).
- [x] Run `& "$env:LOCALAPPDATA\\Microsoft\\dotnet\\dotnet.exe" build MadagascarBrowser.sln --configuration Release --no-restore` (0 warnings, 0 errors).
- [ ] Smoke-test all workflows in the Windows UI; UIA confirmed the Release app starts, but WebView2 content and dialogs were not fully exercised.
