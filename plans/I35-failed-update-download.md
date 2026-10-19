# I35: Do not apply an update after its download fails

Approved scope: gate the existing store's apply call on download success, return the original failed result, clear busy flags and allow retry. No backend updater, API, retry policy, dependency or component redesign.

1. Replace the unconditional download-to-apply chain with the existing result-success conditional RxJS pattern.
2. Unit checks: logical HTTP200 failure, HTTP500 failure, network failure, corrected retry, apply-stage success/failure and unsubscribe finalization. Assert outgoing request order, original error and busy-stage transitions.
3. One browser workflow uses actual update dialog controls with desktop response headers: failed download, visible original error, no apply, dismiss error, successful retry and exactly one apply. Mock API replies only; no actual updater/restart.
4. Finish edits, then targeted Vitest/lint and CI Firefox Cypress spec. Frontend LSP is unavailable in this session; use native package checks. Commit I35 alone after observed checks. No pre-fix execution under host validation policy.
