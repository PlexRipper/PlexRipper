# I51–I56 selected improvements

Approved: all six fixes, technical review and one local commit per ID without human review pauses. No push/PR. Execute sequentially; a blocked verification/review pauses the queue. README/CHANGELOG untouched.

## I51 — Resume artifact validation
Reject missing or too-short partial artifacts before restoring saved chunk positions; preserve files/snapshot and use StorageError. No automatic restart/hashing or same-length-content guarantee. Verified: `DirectPlexDownloadClientResumeArtifactUnitTests` 3 passed; existing `DirectPlexDownloadClientStartUnitTests` 40 passed. Native diagnostics clean. Independent safety/proof reviews cleared after correcting the returned error to retain storage classification through DownloadJob. CodeScene gate passed; production score 8.16, new tests 10.0. Commit: `c4042d84b`. Status: complete.

## I52 — Torrent identity validation
Validate coherent Movie/Episode media/data/part/library/server identity at generation and upload before mutation; scope hash association to validated source and avoid zero-match false success. Preserve payload, ownership and valid multipart semantics. Verified: `AddTorrentEndpointUnitTests` 22 passed, `DownloadTorrentEndpointUnitTests` 8 passed, separated existing `AddTorrentValidationUnitTests` 4 passed, `TorrentIdentityIntegrationTests` 3 passed. Hosted tests exercise real generation/upload/info, exact canonical identity/owner/hash and same-library mixed identity rejection without task/parent/hash mutation. User accepts hosted Reaparr+SQLite proof, not live Arr compatibility. Independent source/proof reviews cleared (0.93/0.98); CodeScene gate passed, production scores 10.0/9.44. Status: verified; committing.

## I53 — Remaining resume space
Depends on I51. Use validated durable chunk progress, not preallocated file length, to calculate required remaining free bytes. Fresh/fallback and DASH full-size behavior unchanged. Test sufficient/exact/insufficient/zero remaining/fresh plus I51+I53 composition before I53 commit. Independent review and normal gates. Status: pending.

## I54 — Compatible default destination
Validate library/folder existence and compatible destination type/media before update; preserve prior choice on rejection. No migration/old-state repair/custom-path or retyping redesign. SQLite endpoint and resolver cases, independent persistence review and normal gates. Status: pending.

## I55 — Failed-disable dialog recovery
Close server confirmation/settings only after successful result; restore loading/retry on failure. Keep existing global error presentation; no backend rollback or automatic retry. Component failure/error/retry and browser isolated-API journey. Self-review and normal gates. Status: pending.

## I56 — Movie duplicate query batching
Bounded ownership/server/library-aware duplicate identity preload per selected group. Preserve exact existing creation/skip/quality/multipart/save/report semantics. Actual SQLite read counts at small/large selections plus exact ownership and control-state assertions. No cache/schema/deps/cross-family refactor/latency claim. Independent review and normal gates. Status: pending.

## Verification boundaries
Preflight observed .NET 10.0.113/Bun 1.4.3, backend test-project discovery, Vitest binary, Chrome and correct Rider/WebStorm projects. Backend test execution and hosted startup exercised for I51/I52; browser workflow remains unverified until I55. Use native test MCP for backend; Bun for frontend checks; WebStorm-first frontend lifecycle. No production Plex/Arr data. Pre-fix test execution omitted under host final-phase validation rule; regressions distinguish the source-established defect. CodeScene checks required for changed source and pre-commit safeguard. Preserve unrelated work.
