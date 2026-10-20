# I57–I66 selected improvements

User selected Fix I57–I66 and Implement all selected fixes. Implement one ID through verification, technical review and its separate local commit before editing the next. No push, PR, history rewrite or formatting work.

## Verification boundary

The user explicitly accepted targeted changed-file lint, Vitest and actual isolated browser workflows while 15 known unrelated frontend typecheck diagnostics remain in DirectoryBrowser.vue/debug-pages/dialogs.vue. Do not rerun that failure merely to confirm it, repair unrelated files or claim a clean full typecheck. Backend changes require native diagnostics and targeted test MCP execution. Use isolated HTTP/SQLite/third-party fixtures; no live Plex/Arr/GitHub or packaged updater compatibility claims. Complete related edits before validation. CodeScene reviews/safeguard and risk-based technical review remain required.

## I57 — Stop update-check feedback

Moved UpdateAvailable announcement ownership from CheckForUpdatesCommand to scheduled/startup CheckForUpdateJob. HTTP reads no longer broadcast; successful positive background checks announce once, with failed/cancelled/throwing paths contained. Manual checks update their requesting browser only. Verified: job6, preserved desktop command1, hosted endpoint5, SignalR consumer Vitest1 and isolated Firefox workflow1 passed; changed-file lint and native C# diagnostics passed. Hosted proof executes the actual handler twice with exact results and zero update notifications. Independent semantic/proof reviews clear; all7 source files reviewed by CodeScene, introduced test duplication removed, safeguard passed with stable/improved findings. Compiler Task/ValueTask ambiguity fixed using explicit async Task return type. Browser fixture corrected to current-document hub publication plus page-ready barrier. No timers/cache/retries/new command options or live compatibility claim. Status: verified and reviewed.

## I58 — Current Plex credential validation

Bind validation to current credential/token/auth-mode inputs, serialize only the active mode, invalidate stale validation and require it before saving changed authentication inputs. Ignore late validation responses; preserve metadata-only edits and same-token successful validation. Frontend flow only; no backend DTO/protocol or access-reconciliation redesign. Required store/component/browser proof; independent review.

## I59 — Integration dialog request ownership

Scope load/save/setup/test results, errors, finalizers and dialog-closing callbacks to their current session/request. Preserve existing current-input guards and list-refresh contract. No rollback of completed server mutations or general request framework. Required deferred-response and browser A/close/B proofs; independent review.

## I60 — Separate account history

Group intervals/row identity by recorded account/server/library, preserve all-account presentation, and remove the unused account-selection/fetch gate and first-current-account dependency. Retained account IDs are required historical scalars without account FK. No selector/schema/API changes. Required interleaved-account and zero-current-account history tests/browser proof; independent review.

## I61 — Validate saved HTTP(S) URLs

Use existing shared HTTP(S) validation at four Arr save endpoints and custom Plex create/update, with corresponding form guards. Preserve trimming, valid ports/base paths and offline saves. No mandatory connection test or API/schema change. Required validator/SQLite unchanged-state and actual form browser proof; independent review. If I59 selected, include composition here.

## I62 — Classify custom LAN connections

Derive Local from the complete submitted URL in custom create/update using the existing helper. Preserve ownership, other metadata, preference/HTTPS/local/public order and untouched records. No migration, retroactive repair or IPv6-range expansion. Required endpoint/DTO and chooser SQLite proof; local self-review.

## I63 — Preserve authoritative update-check failures

Propagate failed/cancelled GitHub fallback instead of successful NoUpdate. Preserve legitimate empty success and desktop Velopack success without release notes. No retry/cache/banner/install changes. Required failure/cancellation/control tests and last-good frontend state proof; independent review. If I57 selected, include updater composition before this commit.

## I64 — Preserve committed account deletion result

Return successful DELETE independently of follow-up refresh failure, remove only its ID locally, and scope dialog/loading completion to the owning account/session. Failed DELETE preserves state. No backend deletion/cascade/transaction changes or automatic retry. Required real-store pipeline and browser DELETE-success/GET-failure proof; independent review.

## I65 — Retryable notification actions

Observe hide/clear results; mutate local rows and close the drawer only after success. Failed actions retain rows/retry. Scope clear completion to captured IDs rather than overwriting newer arrivals or rolling back a full snapshot. No server clear API/concurrency redesign, retries or new alert system. Required failed-result/error/retry/concurrent-arrival store and browser proof; independent review.

## I66 — Inline two-factor retry feedback

Use existing failure state for unsuccessful verification emissions; preserve credentials, open dialog and retry. No new notification system or Plex/backend semantics. Required challenge-first failed-retry then success store/component/browser proof. Local self-review; include I58 composition if both selected.
