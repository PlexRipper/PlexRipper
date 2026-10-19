# I36: Return the torrent-add failure marker Arr recognizes

Approved production scope: change only the failed CreateDownloadTasksCommand response body from Fail. to protocol-compatible Fails. Preserve HTTP200, success Ok., authentication, upload validation and hash binding behavior. No zero-report policy or schema change.

Verification scope explicitly revised by user: hosted protocol checks only for I36–I38; live Sonarr/Radarr execution waived. No claim of live callback or installed-version verification.

1. Assert exact failure body and absence of hash mutation in the existing endpoint test.
2. Add hosted real-command regression: obtain a valid torrent from a seeded movie, remove that movie, upload stale torrent through the authenticated public route, assert Fails. and no new task/hash state. Preserve a non-target task.
3. Finish edits, run C# diagnostics and the smallest endpoint/hosted regression checks via dotnet-test-mcp, then commit I36 separately. No pre-fix execution under host validation policy.
