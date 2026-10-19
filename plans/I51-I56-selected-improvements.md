# I51–I56 selected improvements

Approved: all six fixes, technical review and one local commit per ID without human review pauses. No push/PR. Execute sequentially; a blocked verification/review pauses the queue. README/CHANGELOG untouched.

## I51 — Resume artifact validation
Reject missing or too-short partial artifacts before restoring saved chunk positions; preserve files/snapshot and use StorageError. No automatic restart/hashing or same-length-content guarantee. Verified: `DirectPlexDownloadClientResumeArtifactUnitTests` 3 passed; existing `DirectPlexDownloadClientStartUnitTests` 40 passed. Native diagnostics clean. Independent safety/proof reviews cleared after correcting the returned error to retain storage classification through DownloadJob. CodeScene gate passed; production score 8.16, new tests 10.0. Commit: `c4042d84b`. Status: complete.

## I52 — Torrent identity validation
Validate coherent Movie/Episode identity at generation and upload before mutation; scope hash association to validated source/exact owner and avoid zero-match false success. Verified: upload unit22, generation unit8, separated existing validation4, hosted identity3 passed. Hosted real generation/upload/info proves exact source/owner/hash/parent and mixed identity rejection without mutation. User accepts hosted Reaparr+SQLite proof, not live Arr compatibility. Independent source/proof reviews clear0.93/0.98; CodeScene gate passed, production10.0/9.44. Commit: `16736e119`. Status: complete.

## I53 — Remaining resume space
Depends on I51. Budget remaining bytes from coherent range-supported, same-total snapshots after artifact validation, not preallocated length. Completed valid resumes skip the positive-only directory command. Fresh/fallback and DASH unchanged. Verified I51+I53 artifact2/success3 and insufficient/fallback6 cases; existing Start40 passed. Real directory handler with mocked free space; native diagnostics clean. Independent safety/proof reviews clear0.88/0.90; CodeScene gate passed, source8.18/tests10.0. No live Plex/allocation benchmark. Commit: `5364662e5`. Status: complete.

## I54 — Compatible default destination
Validate library/folder existence and compatible destination category/media before update; preserve prior choice on rejection. No migration/old-state repair/custom-path, retagging or concurrent-mutation redesign. Verified real SQLite endpoint/resolver: compatible6/incompatible4/missing2 passed, invalid-ID2 passed after removing unwritten HTTP status assumption from validation-only harness test. BaseResultDTO assertions corrected to actual IsSuccess property. Exact all-library destination snapshots and target/control resolver checks. Independent persistence/proof reviews clear0.91/0.99 after harness correction; CodeScene source/tests10.0 and gate passed. Status: verified; committing.

## I55 — Failed-disable dialog recovery
Close server confirmation/settings only after successful result; restore loading/retry on failure. Keep existing global error presentation; no backend rollback or automatic retry. Component failure/error/retry and browser isolated-API journey. Self-review and normal gates. Status: pending.

## I56 — Movie duplicate query batching
Bounded ownership/server/library-aware duplicate identity preload per selected group. Preserve exact existing creation/skip/quality/multipart/save/report semantics. Actual SQLite read counts at small/large selections plus exact ownership and control-state assertions. No cache/schema/deps/cross-family refactor/latency claim. Independent review and normal gates. Status: pending.

## Verification boundaries
Preflight observed .NET 10.0.113/Bun 1.4.3, backend test-project discovery, Vitest binary, Chrome and correct Rider/WebStorm projects. Backend test execution and hosted startup exercised for I51/I52; browser workflow remains unverified until I55. Use native test MCP for backend; Bun for frontend checks; WebStorm-first frontend lifecycle. No production Plex/Arr data. Pre-fix test execution omitted under host final-phase validation rule; regressions distinguish the source-established defect. CodeScene checks required for changed source and pre-commit safeguard. Preserve unrelated work.
