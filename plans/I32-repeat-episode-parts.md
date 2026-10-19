# I32: Avoid adding an existing episode part again

Approved scope: sequential repeat generation skips existing (PlexApiMediaId,PlexApiPartId) identities within the same integration-owned episode. Add only missing parts and their logs/report counts. Preserve multipart filenames, quality selection, hierarchy IDs and other integration jobs. No schema or bulk cleanup of existing tasks.

1. Reuse the music generator's existing-child identity check before mapping/adding episode file rows.
2. Cover repeat requests, a missing part of an existing quality, another quality and separate integration ownership using real seeded data and the command harness. Assert exact source identities, preserved task IDs/paths/statuses, hierarchy, logs and report counts.
3. After all edits run C# diagnostics and the episode generation class, then make one local I32 commit. Pre-fix execution omitted under host validation policy.
