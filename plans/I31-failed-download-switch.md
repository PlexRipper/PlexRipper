# I31: Undo a failed download switch

Approved scope: compensate the newly scheduled download if pausing the old active download fails or is cancelled. Preserve completed/advanced work, original failure, provisional sibling states and other tasks. No new server-wide scheduling architecture, schema or public API.

1. Keep start and pause results cancellation-aware. Capture original selected/sibling statuses already changed by the request.
2. On failed handoff, stop only the newly scheduled key using bounded independent cleanup cancellation. Restore selected state only after successful stop and only while it is still in the download phase; restore queued siblings only while they still have this provisional Queued state. Preserve completed, moving, errored, removed or otherwise advanced records. Merge cleanup errors with the original result.
3. Add deterministic command-harness tests with real database/status dispatcher and exact scheduler contracts for failure, cancellation, cleanup failure, completion during handoff, provisional sibling preservation and normal success. Strengthen the existing pause-failure test.
4. After all edits: C# diagnostics and smallest affected test classes, then one local commit containing only I31 production/tests/this plan. No pre-fix test execution under host validation policy.
