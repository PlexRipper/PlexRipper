# I57–I66 selected improvements

User selected Fix I57–I66 and Implement all selected fixes. Implement one ID through verification, technical review and its separate local commit before editing the next. No push, PR, history rewrite or formatting work.

## Verification boundary

The user explicitly accepted targeted changed-file lint, Vitest and actual isolated browser workflows while 15 known unrelated frontend typecheck diagnostics remain in DirectoryBrowser.vue/debug-pages/dialogs.vue. Do not rerun that failure merely to confirm it, repair unrelated files or claim a clean full typecheck. Backend changes require native diagnostics and targeted test MCP execution. Use isolated HTTP/SQLite/third-party fixtures; no live Plex/Arr/GitHub or packaged updater compatibility claims. Complete related edits before validation. CodeScene reviews/safeguard and risk-based technical review remain required.

## I57 — Stop update-check feedback

Moved UpdateAvailable announcement ownership from CheckForUpdatesCommand to scheduled/startup CheckForUpdateJob. HTTP reads no longer broadcast; successful positive background checks announce once, with failed/cancelled/throwing paths contained. Manual checks update their requesting browser only. Verified: job6, preserved desktop command1, hosted endpoint5, SignalR consumer Vitest1 and isolated Firefox workflow1 passed; changed-file lint and native C# diagnostics passed. Hosted proof executes the actual handler twice with exact results and zero update notifications. Independent semantic/proof reviews clear; all7 source files reviewed by CodeScene, introduced test duplication removed, safeguard passed with stable/improved findings. Compiler Task/ValueTask ambiguity fixed using explicit async Task return type. Browser fixture corrected to current-document hub publication plus page-ready barrier. No timers/cache/retries/new command options or live compatibility claim. Status: verified and reviewed.

## I58 — Current Plex credential validation

Implemented current-input validation snapshots and owned validation requests. Active credential/token/mode changes invalidate validation/date/generated token; stale values/errors/finalizers cannot alter another draft. Changed authentication requires fresh proof, including same-token success; unchanged metadata edits remain usable for both validated and unvalidated saved accounts. Token-mode username remains profile metadata and inactive secrets are excluded from payloads. Verification dialog consumes owned emissions/loading; I66 failure feedback intentionally unchanged. Verified 41 account-dialog cases across focused runs, changed-file ESLint and final isolated Firefox workflows2 (metadata/save/revalidation and delayed validation). Corrected test synchronization to await stale completion rather than unconfigured fake timers. Independent semantic review clear; proof-review fixture findings corrected and observed passing. CodeScene reviewed all8 source files; introduced browser-body length and token-test duplication resolved, corrected proof scores10, and safeguard passed. Existing source complexity and other legacy duplication unchanged. Accepted full-typecheck/live-Plex limits unchanged.

## I59 — Integration dialog request ownership

Implemented private dialog-session/request ownership for load/test and shared save/setup mutation ownership. Opening an edit clears the old record and blocks accidental create until its intended detail loads; failed loads stay invalid. Late results/errors/finalizers and post-refresh completions cannot replace, close or configure a newer dialog. Successful obsolete server mutations still refresh the list, while post-submit draft edits survive without a close emission; current canonical results still emit normally. Verification: maintained integration cases22 plus ownership cases31 passed; changed-file ESLint passed; actual Firefox workflows6 passed across Sonarr/Radarr for late detail, save/refresh/B-spinner and Save-and-setup callbacks. Fixed a test releasing the wrong Sonarr Subject, corrected modal-covered card assertions without increasing timeouts, and sequenced Vitest after Nuxt generation to avoid a shared-cache setup race. Independent source/proof reviews clear, including final corrections; all4 source files reviewed at Code Health10 after bounded adapter/test consolidation. No backend/URL/Delete/refresh-pipeline redesign or live-service claim. Precommit safeguard passed.

## I60 — Separate account history

Implemented recorded account/server/library grouping and row identity, preserving all-account presentation while removing the obsolete account-selection/fetch gate and first-current-account dependency. Retained account IDs remain required historical scalars; no selector/schema/API changes. User separately approved the runtime-exposed chart dependency: bind through public getList/getOriginalID/getListChildRow instead of raw DOM IDs that JSGantt hashes. Unexpected loss of the two data edits was preserved for a decision and restored only after explicit user approval. Final proof: all3 store regressions passed (interleaved same-name accounts, independent active/revoked state, server/library boundaries); actual Firefox workflow1 passed (zero current accounts, HTTP200, exact account/date notes and independent row selection); five-file ESLint passed. Independent core and incremental binding/proof reviews clear. CodeScene: store9.28 and chart8.79 retain existing complexity findings; overview and both proofs10. No formatting, unrelated type cleanup, full-typecheck or live Plex compatibility claim. Separate I60 commit follows the required safeguard.

## I61 — Validate saved HTTP(S) URLs

Implemented the existing shared HTTP(S) validator at all four Arr save endpoints and custom Plex create/update, plus form and direct-store guards. Preserved server-owned trimming, valid ports/base paths, offline saves and the custom dialog's origin-only payload; no API/schema/local-classification change. Final proof: six valid hosted HTTP/SQLite controls passed, then all six rejection cases passed with exact existing metadata.url messages and full unchanged-state snapshots. All54 integration-store tests and all17 actual Firefox scenarios passed, including I59 request ownership and Sonarr successful-test invalidation. Changed-file ESLint passed. Production, final backend proof and final browser/store proof independently reviewed clear0.94 each. Every changed source reviewed by CodeScene; new backend proof10, browser scores7.78/7.82/9.04 contain only pre-existing findings. Safeguard passed15/15 eligible files, with Sonarr proof improved and no introduced regressions. Accepted unrelated full-typecheck and no-live-third-party limits remain. Verified and reviewed for the separate local commit.

## I62 — Classify custom LAN connections

Implemented only Local = req.Url.IsLocalUrl() in custom create/update; ownership, other metadata, chooser precedence and existing records remain unchanged. Hosted create/update regressions now use recognized private IPv4 URLs with scheme-less addresses and assert Local/Local-type responses plus exact persisted metadata; both passed. Strengthened SQLite chooser proof with an actual public-address HTTP control, no preferred/HTTPS override, and exact selected private connection ID/URL; passed. Self-review confirmed the bounded four-source diff. Changed test diagnostics OK; CodeScene create/update/hosted proof10 and chooser7.55 with pre-existing unrelated findings only. No migration, retroactive repair, helper or IPv6-range expansion. Awaiting safeguard and separate local commit.

## I63 — Preserve authoritative update-check failures

Propagate failed/cancelled GitHub fallback instead of successful NoUpdate. Preserve legitimate empty success and desktop Velopack success without release notes. No retry/cache/banner/install changes. Required failure/cancellation/control tests and last-good frontend state proof; independent review. If I57 selected, include updater composition before this commit.

## I64 — Preserve committed account deletion result

Return successful DELETE independently of follow-up refresh failure, remove only its ID locally, and scope dialog/loading completion to the owning account/session. Failed DELETE preserves state. No backend deletion/cascade/transaction changes or automatic retry. Required real-store pipeline and browser DELETE-success/GET-failure proof; independent review.

## I65 — Retryable notification actions

Observe hide/clear results; mutate local rows and close the drawer only after success. Failed actions retain rows/retry. Scope clear completion to captured IDs rather than overwriting newer arrivals or rolling back a full snapshot. No server clear API/concurrency redesign, retries or new alert system. Required failed-result/error/retry/concurrent-arrival store and browser proof; independent review.

## I66 — Inline two-factor retry feedback

Use existing failure state for unsuccessful verification emissions; preserve credentials, open dialog and retry. No new notification system or Plex/backend semantics. Required challenge-first failed-retry then success store/component/browser proof. Local self-review; include I58 composition if both selected.
