# I33: Continue waiting moves after early exits

Approved scope: continue waiting work after a removed/cancelled move or individual user pause/stop, after task state is settled; preserve partial files, existing copy-failure retry policy, and shutdown behavior.

Evidence correction: cleanup failures already reach the outer failed-result queue continuation. Preserve that existing behavior and its tests. No extra fix is needed for that branch.

1. Add queue continuation for missing tasks and cancelled moves only when the current task has left the eligible moving/download-finished state. Manual pause/stop handlers check the queue once after all selected move statuses settle; do not kick while iterating siblings. Preserve original command result if the best-effort wakeup fails, logging its failure.
2. Use independent non-cancelled continuation cancellation when a job was individually cancelled. Guard the actual queue with existing host ApplicationStopping so shutdown cannot schedule fresh moves. Preserve the current one-shot retry trigger and ordinary completion continuation.
3. Regression tests: real two-task queue after manual pause/stop, removed task, real partial-copy cancellation, cleanup failure, shutdown, and delayed retry. Verify target status/bytes/offsets and exact next scheduled key.
4. Complete edits before diagnostics and focused affected test classes. Commit I33 separately after observed validation. No pre-fix execution under host validation policy.
