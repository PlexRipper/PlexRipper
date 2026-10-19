# I38: Report known failures as torrent errors

Approved scope: map AuthError, StorageError, SourceUnavailable, DownloadClientError and IntegrityError to qBittorrent error; map Restarting to queuedDL. Preserve normal/legacy mappings, ownership and unknown fallback/logging. No retry or Arr automatic-removal policy change.

1. Extend the existing wire-state switch only.
2. Add direct status matrix and hosted authenticated torrents/info regressions for integration-owned file tasks with non-target ownership controls. Assert exact hashes/states and unchanged rows; include normal and unknown mapping behavior.
3. Complete edits before C# diagnostics, focused endpoint suite and hosted checks. User waived live Arr execution; report hosted protocol verification only. Commit I38 separately, then summarize all selected commits and deferred issues and ask whether to audit another batch.
