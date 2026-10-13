# Music identity and audio-quality proposal

Status: researched audio-quality proposal, checked against the two complete supplied Adele hierarchies. **Current retrieval authorization is paginated section `/all` only; metadata-detail hydration is withdrawn.** Fresh `/all` probes on both servers returned audio streams when `includeOptionalElements=Stream` was requested (§3.5). The user selected **same Plex catalog content**, not exact-release ownership; §5 records that policy. The approved two-column identity cleanup is implemented and verified in isolated tests; its forward migration has not been applied to live data. Implementation of audio quality and native-GUID matching remains proposed, not authorized by this plan revision.

## 1. Agreed contract

- Start from the existing Movie/TV comparison architecture, matching waterfall, library-pair scopes, ownership union, upgrade aggregation, state rollup, jobs, and invalidation. Do not create another music comparison pipeline.
- Preserve the current music title/year/edition and parent-scoped disc/track/title fallbacks, including their ambiguity/conflict safeguards. Do not switch to identifiers-only matching. A successful native catalog match takes precedence over differing secondary release metadata, as agreed in §5.
- Use identities supplied by Plex, with entity-specific native Plex GUIDs as the primary comparison layer. Retain Plex-supplied MBIDs as secondary identities. No external identity-service calls, fingerprinting, or filename identity guesses.
- Introduce a separate **66-member `AudioQuality` enum**: music stores and compares `AudioQuality`; Movie/TV retain `VideoQuality`. These are classifications, not transcoding instructions. File encoding and transcoding behavior do not change.
- Preserve codec and the approved nullable per-file fields: bitrate, bit depth, sample rate, channels, channel layout, and audio profile. The user explicitly approved `AudioProfile` after research exposed the AAC-profile compatibility dependency.
- Guard comparisons of unknown or incompatible audio encodings. These are approved audio-measurement rules, not a change to cross-library aggregation.
- Obtain music identities and audio data through the existing paginated `/library/sections/{sectionId}/all` requests only. Keep `includeGuids=1`; request `includeOptionalElements=Stream` on track pages using the verified API-version selection. No per-track or batched metadata-detail requests, children/grandchildren requests, or detail fallback.
- Reconcile supplied identity/audio changes independently of `UpdatedAt`; backfill existing records without deleting the hierarchy.
- Complete the backend/API/generated-contract and affected music-selection caller cutover. No UI redesign, new audio-quality filters, new user preferences, or automatic commits.

The earlier proposal's “all-owned-copies” exception and removal of fallback matching are withdrawn. Neither is an agreed requirement.

### Authorized identity cleanup

- Remove `PlexMusicAlbum.MusicBrainzReleaseGroupId` and `PlexMusicTrack.MusicBrainzRecordingId`, which the sampled Plex responses did not supply.
- Retain `MusicBrainzArtistId`, `MusicBrainzReleaseId`, and `MusicBrainzReleaseTrackId` for the artist/album/track MBIDs actually supplied by Plex.
- Remove the obsolete recording-ID matching layer and removed-field conflict checks; retain release-track-ID matching and existing parent-scoped title/position fallbacks.
- Clean up affected mappers, fixtures, tests, and the obsolete match-type enum member. Generate a forward EF migration without rewriting historical migrations or applying it to live data.
- Delete persisted recording-only track matches (`MatchType = 10`) and invalidate music comparison scopes (`MediaType = 'MusicArtist'`) in the forward migration. Keep valid hits and Movie/TV scopes unchanged. Music remains not compared until the existing comparison job rebuilds its scopes.
- Stored values in the two removed columns are discarded when the migration is applied. Recording-only matches may cease to match after recomparison; no replacement identity algorithm is authorized.

Cleanup verification:

- Generated migration: `20261010212016_RemoveUnsuppliedMusicIdentityColumns`, using the existing Data design-time factory through EF MCP; historical migrations remain unchanged.
- Rider: successful final solution build and no errors reported for edited C# files.
- Actual TUnit runs: migration preservation regression **1 passed**, music comparison unit class **47 passed**, hosted music comparison integration class **1 passed**; **0 failed, 0 skipped**.
- The upgrade regression verifies that only the two approved columns disappear, retained Plex identities and hierarchy/file rows survive, obsolete recording-only hits are deleted, and Movie scopes plus valid track hits remain.
- The test MCP returned inconsistent counts; verification instead used the actual TUnit executables through the .NET project MCP, with reports under `/tmp/reaparr-music-identity-verification/`.


## 2. Domain terms

| Term | Meaning in this proposal |
|---|---|
| Plex rating key | Server-local lookup key; useful for sync, not cross-server identity |
| Plex identity | A supplied entity-specific `plex://artist/...`, `plex://album/...`, or `plex://track/...` catalog identity; not a file hash or a guarantee of an exact release |
| Local GUID | A supplied `local://...` identifier; never a cross-server identity |
| Plex-supplied MBID | An `mbid://...` value returned in Plex metadata; Reaparr uses the supplied value without an external lookup |
| Track occurrence | An existing track row under an album; its native track GUID can also occur under another album or single |
| Track version | The existing grouping by `PlexApiMediaId` |
| Track file | An existing music media-data row, keyed through its Plex media/part IDs |
| Audio quality | Encoding classification and compatible technical capacity, not proof of mastering quality or audible fidelity |
| Missing | No identity match across the applicable current owned-library scopes; missing audio metadata does not make a track missing |

## 3. Research evidence

### 3.1 Historical detail-endpoint observations — not an ingestion authorization

The supplied album was Adele's **30**, album rating key `78657`, with 12 tracks. These earlier observations establish field semantics; their detail endpoints are **not allowed by the current retrieval contract**. The authorized literal verification credentials are isolated in §14.

| Sample | Supplied audio metadata | Consequence |
|---|---|---|
| Artist detail `78622` | `plex://artist/5d07bce8403c64029056b5a6`; supplementary MBID `cc2c9c3c-b7bc-4b8b-84d8-4fbd8779e493` | Use Plex's artist identity for cross-library matching |
| Album detail `78657` | `plex://album/616714864c451f5a443193b0`; supplementary MBID `c973fb76-4f5a-4ed2-8fe6-e1b9ed783901` | Retain native and supplied external identities; this historical album response did not establish stream availability |
| Track detail `78658` | `plex://track/616962f44c451f5a447a9a92`; supplementary MBID `c23f6c2a-6f1a-46ff-a950-eace10be7f7c`; FLAC, 737 kbps, 16 bits, 44,100 Hz, 2 channels, stereo | Retain the supplied track identity and classify the audio stream |
| Track detail `103666` | FLAC, 3,137 kbps, 24 bits, 96,000 Hz, stereo | High-resolution properties are actually available on this server |
| Track detail `79088` | MP3, 128 kbps, 44,100 Hz, stereo; no bit depth | Missing lossy bit depth is normal, not evidence of incomplete analysis |
| Track detail `150743` | Vorbis, Ogg container, 160 kbps, 44,100 Hz, stereo; local GUIDs, no MBIDs | Container is not codec; existing identity fallbacks remain necessary |
| Track detail `40958` | FLAC stream supplies 778 kbps, 16 bits, 44,100 Hz; list Media omitted bitrate | Prefer audio-stream measurements, then appropriate music Media fallbacks |
| Track detail `50561` | Local GUID and an `.mp3` path, but no verified codec/bitrate/sample rate/streams even on detail | Analysis can be absent; extension alone must not create a quality classification |

Historical endpoint differences were exercised before the `/all`-only restriction:

- `/library/metadata/78657/children?includeGuids=1`: track identities and Media/Part data, but no audio streams in the sampled response.
- `/library/sections/4/all?type=10&includeGuids=1&includeMeta=1` with pagination: normal ingestion-style results contain Media/Part information but omit `Part.Stream`.
- `/library/metadata/78658?includeGuids=1`: `Part.Stream` includes audio `streamType=2` and a separate lyrics stream `streamType=4`.
- `/library/metadata/78658,78659?includeGuids=1`: two metadata objects with streams. Batching was observed historically, but neither batched nor individual detail requests are part of this proposal now.
- The sampled library reports approximately 98,000 tracks. The revised proposal enriches existing paginated `/all` requests rather than issuing additional detail calls; full-library enriched-payload cost has not been benchmarked.

`includeMeta=1` exposes browse/filter/sort metadata; it is not an audio-stream enrichment flag. The separately verified `includeOptionalElements=Stream` option does return streams on `/all` (§3.5). The supplied library's codec filter exposed FLAC, MP3, and Vorbis. AAC, Opus, ALAC, and PCM were **not** live-sampled; their enum coverage is an explicit user decision, not an observation about this server.

Some samples also supplied gain, loudness, peak, and dynamic-range values. Those are normalization/dynamics properties, not a quality order. They are outside this change. Sonic analysis describes similarity, not exact identity or encoding quality.

### 3.2 Plex-supplied identities

The sampled Plex metadata supplied native `plex://artist/...`, `plex://album/...`, and `plex://track/...` GUIDs, plus supplementary `mbid://...` values for those entities. Reaparr already stores the primary `Guid`; the current music matcher does not yet use native Plex GUID matching.

Equal entity-specific native GUIDs identify the same assigned Plex catalog entity across the sampled servers. Rating keys, metadata URLs, database IDs, and `local://...` values are not cross-server content identities. Equal GUIDs do not establish identical files, mastering, release packaging, or unique library rows.

Retain the three existing MBID fields selected by the user for the values actually returned by Plex. Do not populate additional identity fields through external lookups or infer extra identities from a UUID, title, position, or file.

Plex documents MusicBrainz as a central matching source and AllMusic as a metadata source. File organization and artist/album/track tags influence matching; parts of modern matching run in Plex's cloud. Reaparr consumes Plex's result, not a reimplementation of that algorithm. Plex does not publish the complete matching algorithm. The live comparison in §3.4 establishes useful cross-server music identities, not a universal guarantee that every independently matched copy receives the same GUID or that an album GUID denotes one exact release.

### 3.3 Installed SDK capability

The repository declares and lock-resolves `LukeHagar.PlexAPI.SDK` **0.21.0**. The package points to `LukasParke/plexcsharp` commit `125af68293f87d20ee53e1f0479f1b5b91fb2ae9`.

- `ListContentRequest.IncludeGuids` is a nullable `BoolInt`; the existing paginated list handler already sets it to `BoolInt.True`.
- The pinned `ListContentRequest` model does **not** expose `IncludeOptionalElements`. Do not invent that property. Add the verified wire query options at the existing `PlexApiClient.SendAsync` request boundary, narrowly for GET section `/all` track requests (`type=10`), without a new transport, dependency, SDK upgrade, or configuration option.
- `Media`, `Part`, and `Stream` support nullable audio measurements. `Stream` includes codec, bitrate, bit depth, sampling rate, channels, channel layout, and profile.
- There is no typed audio-quality enum or `Lossless` boolean supplied by this SDK.
- Neither inspected list nor detail request model exposes an `IncludeAudio` or `IncludeMedia` flag. The supported wire option is the generic `includeOptionalElements=Stream`, independently verified on both servers; `IncludeMeta` is not its equivalent.
- Generated non-nullable properties can still deserialize to defaults when PMS omits a field. Wire presence, not C# annotations alone, determines whether metadata is known.

### 3.4 Complete cross-server Adele comparison

The two latest supplied servers were inspected with authenticated, read-only requests and `includeGuids=1`. Media filenames are omitted from this plan; the user explicitly requested literal tokens in §14 after the credential-risk warning. Source JSON, rather than manually transcribed identifiers, is the comparison authority. Server identities and detail/child observations below are historical; fresh verification in §3.5 used only section `/all`.

| Source | Server identity from `/identity` | Library / artist key | Complete album rows | Complete track rows |
|---|---|---|---:|---:|
| A — supplied `174-180-75-9` server | `b9694848efe8431e1995140dfea418ff87ece17c` | `9` / `137068` | 8 | 11 |
| B — supplied `70-22-75-73` server | `e6f9b3a791782ca5fa991d632d2ad84d69173511` | `4` / `78622` | 10 | 52 |

Both return artist `plex://artist/5d07bce8403c64029056b5a6` and supplementary `mbid://cc2c9c3c-b7bc-4b8b-84d8-4fbd8779e493`. Server identities and library UUIDs differ; the equal artist GUID is not a coincidentally equal local rating key.

**Album comparison**

| Album | A / B rating keys | Shared native album GUID | A / B present tracks | Shared parent-scoped track GUIDs |
|---|---|---|---|---:|
| 19 | `152613` / `78670` | `plex://album/5d07c2d8403c640290914cf7` | 1 / 12 | 1 |
| 21 | `152611` / `78623` | `plex://album/5d07c4b5403c640290a216ff` | 1 / 11 | 1 |
| 25 | `152619` / `78639` | `plex://album/5d07c9a1403c640290ca914a` | 4 / 11 | 4 |
| 30 | `152617` / `78657` | `plex://album/616714864c451f5a443193b0` | 1 / 12 | 1 |

All four matched albums have **different supplementary MBIDs** between servers. For example, `30` returns `6d541211-c604-4344-a799-11adfea40c9d` on A and `c973fb76-4f5a-4ed2-8fe6-e1b9ed783901` on B. `25` also has differing supplied release dates (`2015-01-01` / `2015-11-20`), as does `21` (`2011-01-01` / `2011-01-19`). Requiring those secondary values to agree would reject equal native catalog identities.

Remaining album rows are singles, each with one present track:

- A only: Chasing Pavements, Cold Shoulder, Make You Feel My Love, Skyfall. Cold Shoulder's track is **Cold Shoulder (Basement Jaxx DUBB)**, not the studio-album track.
- B only: Easy on Me, Hello, Rolling in the Deep, Send My Love (to Your New Lover), Water Under the Bridge, When We Were Young.
- “Only” means no matching native album GUID in the other inspected hierarchy, not proof that the song is absent in every album.

**Track comparison within the four matched albums**

| Album / track | A / B rating keys | Shared native track GUID |
|---|---|---|
| 19 — Cold Shoulder | `152614` / `78674` | `plex://track/5d07d51b403c640290895303` |
| 21 — Rolling in the Deep | `152612` / `78624` | `plex://track/5d07db2e403c640290ef4d5f` |
| 25 — Hello | `152620` / `78640` | `plex://track/5d07f099403c6402900aec57` |
| 25 — Send My Love | `152622` / `78641` | `plex://track/5d07f099403c6402900ae1fc` |
| 25 — When We Were Young | `152621` / `78643` | `plex://track/5d07f098403c6402900add69` |
| 25 — Water Under the Bridge | `152623` / `78645` | `plex://track/5d07f099403c6402900ae180` |
| 30 — Easy on Me | `152618` / `78659` | `plex://track/615cf8e64c451f5a4487bcbc` |

All seven pairs have **different supplementary track MBIDs**, despite equal native track GUIDs and matching disc/track positions. For example, Hello returns `8e78b491-a69f-463e-a7bf-af7fc33cf9f1` on A and `965e94fb-44b0-47f2-82d2-b2113f8f822d` on B.

B has **52 track rows but 47 distinct native track GUIDs**. Hello, Send My Love, When We Were Young, Water Under the Bridge, and Easy on Me each share a track GUID between a studio album and a single; their parent album GUIDs, rating keys, and supplementary track MBIDs differ. Do not add a global unique constraint on track GUID or collapse these into one persisted row.

There are **9 shared track GUIDs without parent scoping, but only 7 shared album-track occurrences**. A's Chasing Pavements and Make You Feel My Love singles reuse track GUIDs present on B's `19`, but those single albums are absent on B. Counting songs globally would incorrectly mark those album occurrences owned under the current hierarchical comparison contract.

For the recommended catalog policy, with A owned and B remote, the native-ID comparison finds 7 of B's 52 track rows: all four studio albums are Partial, the six B-only singles are Missing, and 45 track rows have no parent-scoped match. Reversing direction, B owns all seven of A's studio-album tracks, but A's four singles remain Missing. These are proposed comparison outcomes from the inspected inventory, not results from the current production matcher.

**Audio and retrieval findings**

- Earlier detail metadata covered all **63 tracks**. Fresh enriched `/all` responses independently returned one media version, part, and audio stream per track with the same reported encodings: B has 52 FLAC 16-bit/44,100 Hz stereo streams; A has seven FLAC 16-bit/44,100 Hz stereo streams, three MP3 320 kbps/44,100 Hz stereo streams, and Skyfall in FLAC 24-bit/96,000 Hz stereo. No sampled audio stream supplied a profile.
- All seven parent-scoped pairs have equivalent reported lossless resolution and channel layout. Hello reports 789 / 794 kbps; Rolling in the Deep 942 / 952 kbps. These compression-bitrate differences are **not upgrades**. Skyfall's higher-resolution encoding has no matched track on B; it is missing content there, not a proven upgrade over a B copy.
- Historical default artist `/children` returned only four studio albums on **both** servers, while section `/all?type=9&artist.id=...` returned all 8 / 10 album rows, including singles. Historical `/grandchildren` and section `/all?type=10&artist.id=...` returned the same 11 / 52 track keys. These observations explain why ingestion stays on section `/all`; child endpoints are not authorized fallback paths.
- The supplied popular-track URLs use grouping, filters, limits, and pagination; they are not complete comparison inventories. Use the existing ungrouped section-list ingestion path, inspect actual `size`/`offset`/`totalSize`, and resolve every returned track's parent. Do not infer absent albums from the default artist-child view.
- Fresh default and stream-enriched `/all` responses retained exactly the same 52 / 11 track-key sets and all 63 native/supplementary identity records; enriched responses supplied 63 audio streams. Source-key equality and audio coverage were asserted against saved JSON. These are two sampled artists, not an atomic server-wide snapshot or proof of universal GUID stability.

### 3.5 Verified `/all`-only acquisition

Four fresh read-only requests compared default and enriched track pages on A and B. Both variants used `type=10`, the sampled `artist.id`, `includeGuids=1`, and explicit pagination. The enriched variant additionally used **`X-Plex-Pms-Api-Version=1.0&includeOptionalElements=Stream`**. Reproducible calls are in §14.

| Returned data | B: default → enriched | A: default → enriched |
|---|---|---|
| Track / Media / Part rows | 52 / 52 / 52 → unchanged | 11 / 11 / 11 → unchanged |
| Native GUID and supplementary Guid coverage | 52 tracks → unchanged | 11 tracks → unchanged |
| Audio streams, `streamType=2` | 0 → 52 | 0 → 11 |
| Lyrics streams, `streamType=4` | 0 → 115 | 0 → 26 |
| Media codec / bitrate / channels / container | Present before and after enrichment | Present before and after enrichment |
| Stream bit depth / sampling rate / channel layout | Absent → FLAC 16 bits / 44,100 Hz / stereo | Absent → FLAC 16/44,100 and 24/96,000; MP3 44,100 Hz / stereo with no bit depth |
| Audio profile | Not supplied | Not supplied |

The default Media objects expose `audioCodec`, `bitrate`, and `audioChannels`; Part objects expose file identity, size, container, and duration. Default responses did not supply the stream-only measurements. Enriched audio streams expose codec, bitrate, bit depth where applicable, sampling rate, channels, and `audioChannelLayout`. MP3's omitted bit depth is normal. A's Skyfall supplies 24 bits, 96,000 Hz, stereo, and 2,758 kbps.

Plex documents `includeOptionalElements` as a response-selection request, **not a guarantee**; endpoints may ignore selectors and unanalyzed files may lack fields. API version 1.0 is supported by the two previously identified PMS 1.43.4 servers; older servers were not tested. Missing streams remain unknown, with only genuinely supplied Media fallbacks—never an extra detail request or fabricated resolution/profile.

This keeps the existing number of paginated inventory requests, rather than adding requests per track or detail batch. Stream enrichment also returned lyrics and therefore increases response payload; no full-library bandwidth/throughput claim is made. Research-only `artist.id` filters must not restrict production full-library sync. Keep existing pagination, cancellation, error handling, and completeness checks.


## 4. Current code gaps and affected boundaries

| Boundary | Current evidence | Required proposal |
|---|---|---|
| Music retrieval | `RefreshPlexMusicLibraryCommand` builds the tree from list responses; the existing list request already includes GUIDs but default responses omit streams | Enrich the same paginated section `/all` track requests with the verified optional-Stream query; no detail requests |
| Stream conversion | `MediaContainerMappers.GetLibraryItemsMetadata.cs` already carries much of the requested audio metadata transiently | Preserve nullable meaning and consume audio streams returned on `/all` in the music mapper |
| Album/track identity | Music mappers explicitly null album/track MBID fields | Populate only supplied identifiers with verified entity semantics |
| Track persistence | `PlexMusicTrackMediaData` retains codec but has no requested measurements or audio enum | Add the approved per-file audio fields and typed classification |
| Sync reconciliation | `SyncPlexMusicCommand` updates primarily on `UpdatedAt`/parent changes and replaces changed-track media rows | Detect actual supplied identity/audio changes and reconcile existing file rows safely |
| Comparison | Music identity matching and ownership rollup already exist; quality-aware hit data is missing | Add guarded audio comparison to existing match hits and state commands |
| Details | Music comparison details emit `VideoQuality.None` and select an arbitrary matched representative | Return audio qualities and files that actually explain the comparison hit |
| Selection/download | Shared music preview/generation paths still depend on video-oriented quality selection | Make every music quality consumer use typed audio data; preserve explicit version/file choices |

Primary implementation files:

- `src/Domain/_Shared/Enums/VideoQuality.cs` — convention reference only; do not extend it with audio values.
- New enum: `src/Domain/_Shared/Enums/AudioQuality.cs`.
- `src/Domain/Entities/Plex/PlexMusicArtist.cs`, `PlexMusicAlbum.cs`, `PlexMusicTrack.cs`, `PlexMusicTrackMediaData.cs`.
- `src/PlexApi/_Shared/Mappers/MediaContainer/MediaContainerMappers.GetLibraryItemsMetadata.cs`.
- `src/PlexApi/_Shared/Mappers/PlexMedia/PlexMediaDataMapper.PlexMusicArtist.cs`, `.PlexMusicAlbum.cs`, `.PlexMusicTrack.cs`.
- `src/PlexApi/PlexMedia/GetAllMediaByTypeFromPlexAPI/GetAllMediaByTypeFromPlexAPICommandHandler.cs` — retain paginated `/all` acquisition and `IncludeGuids`; no detail-command caller.
- `src/PlexApi/PlexAPI/PlexAPIClient/PlexApiClient.cs` — existing SDK HTTP boundary: add the two verified query options only for GET section `/all` track requests; preserve unrelated query parameters, headers, retries, timeout and cancellation. The pinned SDK has no typed optional-element property.
- `src/Application/PlexLibraries/RefreshLibraryMedia/ByType/RefreshPlexMusicLibraryCommand.cs`.
- `src/Application/PlexLibraries/RefreshLibraryMedia/CRUD/SyncPlexMusicCommand.cs`.
- `src/Application/PlexLibraries/Comparison/Music/CompareMusicPlexLibraryCommandHandler.cs`, plus existing music match persistence and owned/remote state commands.
- `src/Application/PlexMedia/Comparison/Music/GetMusicMediaComparisonDetailsCommand.cs`.
- `src/Application/_Shared/Mappers/PlexMedia/PlexMediaDTOMapper.PlexMusic.cs`.
- `src/Application.Contracts/_Shared/Mappings/PlexMedia/DTO/PlexMediaDataDTO.cs`, `PlexMediaQualityDTO.cs`, and existing comparison-detail row contracts/mappers.
- `src/Application/PlexDownloads/Preview/GetDownloadPreviewQuery.cs`.
- `src/Application/PlexDownloads/Generate/GenerateDownloadTaskMusicTracksCommand.cs`.

Affected symbol families also include internal `PlexMediaQuality`, `PlexMediaQualityExtensions`/`PickMediaQuality`, `PlexMediaExtensions.Music`, `DownloadMediaDTO`, `DownloadPreviewDTO`, and music download-file quality mapping. Follow their actual inbound usages before implementation; do not create a parallel audio-only preview/download service.

## 5. Identity matching

The user selected **same Plex catalog content** after reviewing the two-server findings. The policy is catalog ownership with album-scoped tracks, not exact release/edition ownership. This settles the proposal's identity semantics; it does not authorize production implementation.

1. Keep server/library/rating-key keys for reconciliation and server-local lookup. Never use local rating keys, Media/Part IDs, `local://` GUIDs, or source URLs as cross-server content identities.
2. Match artists by valid, equal `plex://artist/...` GUIDs first. Within matched artists, match albums by equal `plex://album/...` GUIDs. Within matched albums, match tracks by equal `plex://track/...` GUIDs. Validate the identity's entity type; do not use a track ID as an album or artist ID.
3. A successful native catalog match wins over differing supplementary MBIDs, release dates, present-track counts, and release-local positions. Those values do not prove the catalog identities differ. Keep parent scope and separate track occurrences; do not run the current release-conflict gate as an unconditional veto on the new native layer.
4. If no native candidate matches, retain the existing Plex-supplied MBID layers and their compatibility checks: artist ID, album release ID, and release-track ID. If those also yield no match, retain the existing guarded artist/album title/year/edition and parent-scoped disc/track/title fallbacks. Their conflict rejection and ambiguity handling remain unchanged; do not add fuzzy matching, a duration tolerance, or identifier-conflict overrides to the weaker layers.
5. Preserve the existing candidate/hit persistence and owned-union rollup. Native GUIDs are not globally unique row keys. Count distinct remote track rows for coverage, not distinct GUID strings, and retain every relevant media version and file.
6. Persist the actual winning identity layer through the existing match-type contract and comparison details; a native Plex match must not be labelled an MBID match. Reuse an existing suitable match kind if present; otherwise add the required native-Plex kind during the approved caller cutover.
7. Populate only the retained identity fields from supplied Plex values. A partial response omitting `Guid` children does not erase a known supplementary identity.
8. Apply quality comparison only after a track identity match. Encoding, bitrate, file size, duration, or filename is not an identity substitute.

**Movie/TV parity:** `MatchMovie` and `MatchShow` return the first successful provider-ID layer without requiring all supplementary provider IDs to agree, then retain title/year fallback. The selected native music precedence follows that pattern. Movie/TV code, provider ordering, scope aggregation, and quality behavior remain unchanged.

**Recorded identity decision**

- Equal native identities match despite differing supplied release metadata; actual matched track rows determine Owned/Partial/Missing. This recognizes the four shared albums and seven shared occurrences in §3.4 without inventing external identities.
- Exact-release ownership was not selected. Reaparr will not require equal supplementary album/track MBIDs or equal release dates to accept an equal native catalog identity.
- Album parent scope remains unchanged. No relaxation of conflicting fallback identities, global cross-album song ownership, or further matching exception was approved.


## 6. Per-file audio contract

Add these to the existing music media-data record, not a new track-quality table:

| Field | Source and semantics |
|---|---|
| `AudioCodec` | Existing field; prefer the chosen audio stream, then supplied music Media codec |
| `AudioBitrateKbps` | Nullable integer; chosen audio-stream bitrate, then supplied music Media bitrate; Plex integers are interpreted in kbps for this music contract |
| `AudioBitDepth` | Nullable bits per sample from the audio stream; no lossy bit-depth guess |
| `AudioSampleRateHz` | Nullable sampling rate in Hz from the audio stream |
| `AudioChannels` | Nullable channel count; audio stream, then supplied music Media channel count |
| `AudioChannelLayout` | Nullable supplied layout; do not invent surround/stereo layouts from filename or marketing labels |
| `AudioProfile` | Nullable stream profile, then supplied music Media audio profile |
| `AudioQuality` | Separate enum classification computed from the known codec/measurements |

All source measurements above come from the allowed section `/all` response, using stream enrichment for track pages. A property absent from that response is unavailable to this acquisition contract; no detail request fills it.

Use the existing container, duration, file-size, media-ID and part-ID fields. Stream type 2 identifies audio; lyrics/subtitle streams must never become the source. Prefer selected audio, then default audio, then deterministic audio-stream order when neither marker is present. Do not take maximum properties from different streams and combine them into a fictional encoding.

Absent, zero/default, and empty values mean unknown where the property requires a positive measurement. For optional strings, preserve the supplied meaningful value; do not treat SDK normalization to empty as an authoritative deletion.

The current transient Media bitrate comment says bits per second while values are copied unchanged; the sampled Plex values are consistent with kbps. Make the music field's unit explicit and do not multiply/divide silently. Keep existing metadata-duration conversion to seconds and file-duration milliseconds unchanged.

## 7. Exact proposed AudioQuality members: 66

Names below define the agreed set; assign explicit stable enum values when implementing. Integer order is **not** a universal quality order across codecs. Keep `None` and `Unknown` distinct, following the existing enum convention.

```text
None
Unknown

MP3_Below96Kbps
MP3_96Kbps
MP3_128Kbps
MP3_160Kbps
MP3_192Kbps
MP3_256Kbps
MP3_320KbpsPlus

AAC_Below96Kbps
AAC_96Kbps
AAC_128Kbps
AAC_160Kbps
AAC_192Kbps
AAC_256Kbps
AAC_320KbpsPlus

Vorbis_Below96Kbps
Vorbis_96Kbps
Vorbis_128Kbps
Vorbis_160Kbps
Vorbis_192Kbps
Vorbis_256Kbps
Vorbis_320KbpsPlus

Opus_Below96Kbps
Opus_96Kbps
Opus_128Kbps
Opus_160Kbps
Opus_192Kbps
Opus_256Kbps
Opus_320KbpsPlus

FLAC_16Bit_44_1kHz
FLAC_16Bit_48kHz
FLAC_16Bit_88_2kHz
FLAC_16Bit_96kHz
FLAC_16Bit_176_4kHz
FLAC_16Bit_192kHz
FLAC_24Bit_44_1kHz
FLAC_24Bit_48kHz
FLAC_24Bit_88_2kHz
FLAC_24Bit_96kHz
FLAC_24Bit_176_4kHz
FLAC_24Bit_192kHz

ALAC_16Bit_44_1kHz
ALAC_16Bit_48kHz
ALAC_16Bit_88_2kHz
ALAC_16Bit_96kHz
ALAC_16Bit_176_4kHz
ALAC_16Bit_192kHz
ALAC_24Bit_44_1kHz
ALAC_24Bit_48kHz
ALAC_24Bit_88_2kHz
ALAC_24Bit_96kHz
ALAC_24Bit_176_4kHz
ALAC_24Bit_192kHz

PCM_16Bit_44_1kHz
PCM_16Bit_48kHz
PCM_16Bit_88_2kHz
PCM_16Bit_96kHz
PCM_16Bit_176_4kHz
PCM_16Bit_192kHz
PCM_24Bit_44_1kHz
PCM_24Bit_48kHz
PCM_24Bit_88_2kHz
PCM_24Bit_96kHz
PCM_24Bit_176_4kHz
PCM_24Bit_192kHz
```

Count: 2 sentinels + 4 lossy families × 7 bands + 3 lossless families × 12 combinations = **66**.

Lossy bands use lower bounds: positive bitrate below 96; 96–127; 128–159; 160–191; 192–255; 256–319; at least 320 kbps. A reported 185 kbps is classified in the 160 band, but the actual 185 remains available. A label is a band, not a claim that the encoding is exactly constant-bitrate 160 kbps.

Lossless tiers require an exact supported bit-depth/sample-rate pair. Do not round uncommon rates or depths into a nearby tier. For example, 20-bit ALAC and 32-bit FLAC remain `Unknown` in this enum while their raw properties are retained. Unsupported codecs also remain `Unknown`. This is the agreed enum coverage, not a claim that those valid formats are intrinsically poor.

`None` means audio quality does not apply to a non-file parent. A track/file with insufficient or unsupported data is `Unknown`, not `None`. Do not populate an artist or album with an invented aggregate file encoding.

## 8. Guarded audio upgrades, unchanged comparison aggregation

### 8.1 Audio measurement rules

- A known tier is not a proven upgrade over an unknown tier. Insufficient information means no definitive quality-upgrade hit; it does not invalidate an identity match or turn ownership into Missing.
- Equivalent lossless encodings at the same bit depth/sample rate are equal for this purpose, regardless of FLAC/ALAC/PCM compression bitrate. FLAC bitrate must never be ranked as quality.
- For lossless resolution, require compatible channel layout/profile and no regression in bit depth or sample rate, with at least one strict increase. Mixed regressions such as 16-bit/192 kHz versus 24-bit/96 kHz are incomparable.
- A recognized lossless encoding can be an encoding-capacity upgrade over a recognized lossy encoding when channel/profile/rate compatibility is established. This says nothing about the source master or whether a lossless file was made from a lossy source.
- For lossy bitrate comparison, require the same actual codec and compatible known encoding properties, including relevant profile, sample rate and channels/layout. Use the stored bitrate, including within an enum band. This is an encoding heuristic, not a perceptual-quality proof.
- Different lossy codecs are not ranked by equal or greater bitrate. MP3 320 versus AAC 256 is not a definitive upgrade merely because 320 > 256.
- More channels is not automatically better. Stereo and a surround mix are not ordered by channel count.
- Missing AAC profiles cannot establish profile compatibility. Do not assume two absent profiles mean the encoders used the same profile.
- Display/file classification and upgrade eligibility are separate: a MP3 band can be known while missing sample rate/layout prevents a definitive comparison.

Reuse existing quality helper placement and conventions, but do not use a raw `Max`, integer cast, or `ToId` across all audio enum members as a total order. Do not add a configurable ranking engine.

For logical tracks/versions, preserve the existing best-quality aggregation intent over comparable file encodings. If a logical item's competing versions have no unambiguous best representative under the approved guards, retain the versions and suppress a definitive logical-item upgrade rather than pick whichever enum ordinal is largest. Explicit user-selected version/file IDs still take precedence.

### 8.2 Movie/TV parity retained

- Preserve the existing per remote/owned library-pair comparison scope and music content snapshot eligibility.
- For a matched logical track pair, persist the appropriate existing matched/higher-quality hit state and audio qualities needed by details.
- Across current owned-library scopes, any HigherQuality hit wins, just as Movie/TV currently behave. Another equal/better owned library does not suppress that hit.
- Missing requires no applicable match across the owned union. Count distinct remote child identities when rolling up coverage.
- Reuse Owned, Missing, Partial, HigherQuality and PartialAndHigherQuality parent states and their existing precedence. Pending/NotCompared/Unknown job-state handling remains unchanged.
- Keep existing outdated-scope invalidation and job dispatch. Technical changes that can alter quality must participate in the existing content-change signature so stale comparisons are invalidated.
- Comparison details use the established Missing/HigherQuality inclusion behavior and show the remote/owned files and typed audio qualities explaining the hit. Do not select an arbitrary lowest-ID owned row when it does not explain the hit.

The intentional audio deviation is guarded technical comparability, approved after live/API/codec research. There is no music-specific all-owned-copies aggregation rule.

## 9. `/all`-only retrieval, reconciliation, and backfill

1. Keep the existing ungrouped, paginated section `/all` calls for music artists (`type=8`), albums (`type=9`), and tracks (`type=10`). Resolve parent keys and verify actual `size`/`offset`/`totalSize`; popular/grouped/limited user URLs are research inputs, not complete inventories.
2. Preserve `includeGuids=1`. Add `X-Plex-Pms-Api-Version=1.0` and `includeOptionalElements=Stream` to track-page requests at the existing SDK HTTP boundary, scoped to GET section `/all` with `type=10`. Do not invent SDK fields, change non-music requests, or introduce a second transport/configuration/pipeline.
3. Map each returned track and its Media/Part/audio Stream data by source keys. There is no new/changed/incomplete detail-hydration candidate list and no per-track or batched secondary fetch.
4. If streams or measurements are omitted, use only appropriate supplied music Media fallbacks. Otherwise retain unknown values. No `/library/metadata`, `/children`, `/grandchildren`, external metadata, or automatic Analyze/Refresh calls are allowed.
5. Lossless classification needs codec, bit depth and sample rate. Lossy classification needs codec and positive bitrate; comparison additionally needs compatible rate/channels/layout and relevant profiles. A lossy file's absent bit depth is normal. Optional loudness, language and gain never cause additional acquisition.
6. Merge returned metadata into existing rows by server/library/rating key plus media/part IDs. Preserve known values when the same unchanged source part omits them. If source-file properties changed, do not copy old encoding measurements into a replacement file; its omitted measurements become unknown.
7. Detect actual supplied identity/audio changes even when Plex `UpdatedAt` is unchanged. Reconcile stable media/part rows in place and insert new parts. Remove parts only when the existing authoritative, complete-source listing rules establish removal; failed/partial pages and omitted optional fields do not establish deletion.
8. Existing artist/album/track/file IDs and hierarchy links must survive backfill. Do not use `ForceMediaRefresh` or blindly delete/reinsert changed-track files before keyed reconciliation.
9. Backfill through an ordinary successful full paginated `/all` sync with the enriched track query. Existing rows receive supplied measurements without detail calls; unsupported or unanalyzed files can remain unknown.
10. For the future audio feature, use an additive EF migration for nullable measurements/audio classification and necessary match quality fields. This is separate from the authorized two-column removal migration; neither migration is to be applied to live data during this plan task.
11. Preserve existing cancellation/error handling, signatures/invalidation, and current-snapshot checks. Only publish successful sync/content state after the required inventory and reconciliation succeed; cancelled/failed pages must not produce a falsely completed comparison.

**Freshness limit:** an executed enriched `/all` sync can observe supplied audio-only changes without a changed timestamp. Fields PMS does not return cannot be refreshed by this contract, and an existing cached/skipped listing is not a newly observed snapshot. No detail fallback or universal analysis-completeness guarantee is implied.

## 10. Contracts and caller cutover

- Keep Movie/TV VideoQuality values, wire meaning, ordering and behavior unchanged.
- Add typed audio quality/raw fields to the necessary existing media-data, version-quality, preview and comparison-detail contract shapes. In shared shapes, the existing media-type discriminator determines which quality applies; audio fields are inapplicable to non-music records rather than another video rank.
- Switch every music rank/display/selection consumer away from inherited VideoQuality placeholders. Do not introduce casts, aliases, or audio values inside VideoQuality. Shared video fields remain video fields, not a music compatibility path.
- Browse/detail mapping must expose each stored track file's raw measurements and each version's actual audio classification. Do not use the first part's encoding as evidence for all versions.
- Preserve explicit `DataId` and Plex media-version selection through preview and download generation. Automatic selection uses the same guarded audio ordering as music quality handling; incomparable candidates use the existing deterministic selection convention without pretending one is superior.
- Carry audio classification through affected music download preview/task mappings so a chosen file does not revert to VideoQuality.Unknown/None. This does not authorize an audio transcoding pipeline or copying every technical field onto unrelated download tables.
- Regenerate affected TypeScript contracts when implementing backend contract changes. Start the backend in development mode before `bun run generate-ts`, as the repository requires. Update necessary existing music callers/tests, not UI layout or optional quality filtering.
- The current generic `qualityId` filter is video-oriented. This plan does not add audio-quality filtering; do not silently reinterpret existing video filter IDs as audio IDs.

## 11. Implementation sequence and verification checkpoints

The audio phases below remain proposed future changes. Only the identity cleanup above is authorized now. Use the repository's existing test conventions and tools; no per-phase commits are implied.

### Phase A — classification and persistence contract

- Add the agreed AudioQuality enum and approved nullable per-file properties.
- Add classification/comparison behavior in the existing quality-helper boundary; explicit stable enum values are storage identifiers, not a cross-codec rank.
- Add the additive model/migration changes. Leave unrelated video columns, Photo/Other Video behavior and sentinel-order inconsistencies untouched.
- Tests: supported sample classifications; bitrate band boundaries; unknown/zero/omitted values; unsupported codecs/rates/depths; same lossless resolution with different compression bitrates; compatible lossy changes within a band; AAC profile mismatch/missing; mixed resolution regressions; incompatible layouts; cross-lossy-codec non-ordering.

### Phase B — paginated `/all` acquisition and Plex mapping

- Keep GUID inclusion and add the verified optional-Stream/API-version query at the existing request boundary, scoped to music-track `/all` GETs. Preserve non-music requests, existing query/header values, pagination, cancellation and retry behavior; do not add duplicate query keys.
- Map supplied audio streams into per-file records. Retain only Plex-supplied artist/album/track MBIDs in the three retained fields; do not restore the two removed identity columns.
- Consume existing music inventory pages directly; no selective hydration flow or detail-command invocation.
- Tests: actual-shaped default/enriched `/all` fixtures; audio plus lyrics streams; selected/default/multiple audio streams; supplied Media fallback; selectors ignored/streams omitted remain unknown; no-detail HTTP acquisition contract; normal MP3 lacking bit depth; out-of-order source keys; pagination completeness; identity/audio changes at equal UpdatedAt; request scoping leaves Movie/TV and unrelated requests unchanged.

### Phase C — nondestructive reconciliation/backfill

- Update the existing writer's change detection, nullable merge and keyed file-row reconciliation.
- Update the existing content signature/invalidation inputs for identity and quality-affecting changes.
- Tests: equal timestamp but changed identities/audio; idempotent repeated sync; preserved artist/album/track/file IDs; omitted optional data does not clear known data or delete records; incomplete/failed pages do not establish removal; replacement source does not inherit old quality; ordinary enriched `/all` backfill updates existing records; cancelled/failed inventory does not publish a completed comparison snapshot.

### Phase D — music comparison and details

- When future implementation is authorized, add native Plex GUID matching with the §5 approved identity precedence and matched-parent scope. Retain the weaker Plex-supplied MBID and guarded title/position layers; do not apply exact-release gates to a catalog layer by accident.
- Persist audio-aware hit state/quality and reuse existing owned/remote rollup commands.
- Update music details to report explaining files and audio qualities, not arbitrary matches/video placeholders.
- Tests: missing artist/album/track; partial album and artist; fallback/local-ID fixtures; conflicting editions/ambiguous titles; scoped track positions; equal/lower/higher and incomparable audio; partial plus higher; coverage union across owned libraries; any-higher-hit remains higher even with another equal/better owned library; stale snapshots are excluded. Existing Movie/TV aggregation tests remain unchanged.
- Native-ID regressions: the four equal album GUIDs with different supplied MBIDs/dates still match under catalog policy; all seven equal track GUIDs with different MBIDs match; full `25` versus its four-track subset rolls up Partial; the five album/single GUID repetitions stay separate rows and do not manufacture cross-album ownership; the inspected pair yields 7 scoped matches, not 9 global song matches; no native match is mislabelled as an MBID match. Equal-resolution FLAC bitrate differences produce no HigherQuality hit. Preserve conflict/ambiguity tests for the weaker fallback layers.

### Phase E — consumers and end-to-end verification

- Complete browse/detail/version/preview/generation/task quality mappings and affected generated-client callers.
- Tests: explicit version/DataId survives preview and generation; differing encodings remain distinct versions; music auto-selection never uses VideoQuality ordering; comparison details report the file responsible for a hit.
- Integration: Plex-shaped paginated music `/all` responses → existing database hierarchy → comparison states/details → preview/download selection. Include default/enriched responses, FLAC 16/44.1, FLAC 24/96, MP3 128/320, Vorbis 160, and unanalyzed metadata fixtures; reject unexpected metadata-detail requests.
- Run targeted backend classes through dotnet-test-mcp, then affected integration tests and Movie/TV comparison regressions where shared contracts changed. Use Rider diagnostics for edited code and the Rider build path. Frontend generated-contract/type checks use Bun. A UI smoke check is required only if actual UI behavior is changed during an explicitly approved implementation.
- Verify the additive migration against a disposable database copy: row identities and data survive. Never apply it to live user data without authorization.

Definition of complete implementation: supplied identifiers and approved measurements survive `/all`-only ingestion and updates; existing data is safely enriched without secondary detail requests; music matching/fallbacks/ownership aggregation retain the agreed parity; guarded upgrades and their explanations reach actual consumers; no music quality caller still ranks audio with VideoQuality.

## 12. Exclusions and risks

- Only the agreed two-column identity cleanup and its affected callers/tests/schema changes are authorized for implementation. The audio feature, live migration application, Plex analysis, commits, README/CHANGELOG edits, and UI redesign are not authorized by this revision.
- No external identity-service integration, acoustic fingerprints, sonic analysis, source-authenticity detection, normalization/dynamics persistence, audio-quality filter, ranking preferences, new transport, SDK upgrade, or separate comparison service.
- Native GUID equality identifies the assigned Plex catalog entity, not exact release packaging or equal audio bytes. The two-server study demonstrates matching GUIDs, not universal stability across every agent, rematch, legacy library, or independently identified copy. Incorrect Plex matches remain incorrect inputs; the existing guarded fallbacks still cover unmatched items.
- High bitrate, high sample rate, or a lossless container cannot prove a better master or authentic lossless source. The feature compares reported encoding properties.
- The 66-member set deliberately leaves uncommon lossless combinations and unsupported codecs enum-Unknown while retaining raw measurements. It is not a universal audio format taxonomy.
- `/all` optional-element support and available analysis vary by server/item. Verified enriched samples do not prove older-server support or full-library performance; missing fields remain unknown, without metadata-detail fallback. Additional lyrics streams increase payload even though request count stays paginated-list-only.
- Audio bitrate is explicitly kbps in the new music contract. The existing transient comment mismatch must not become a silent thousand-fold scaling error.
- §14 contains live bearer credentials at the user's explicit request. Do not commit, publish, or copy this plan into public issues/reviews while those tokens are present. Header authentication avoids embedding tokens in URLs but does not make the stored credentials harmless.

## 13. Sources and verification performed

Historical live PMS checks covered the original album/artist/track samples and the two-server Adele study in §3.4: 18 album rows, 63 track rows, complete details/audio streams, independent section/grandchildren key sets, and all parent links. Those detail endpoints are no longer authorized. This revision made **four section `/all` requests only**, comparing default and enriched responses on both servers: 52 / 11 unchanged track-key sets, native and supplementary identities on every track, no default streams, and 63 enriched audio streams. Saved-JSON assertions passed for exact key-set and identity/parent/media/part equality, expected counts, one audio stream per sampled track, FLAC resolution/layout, normal absent MP3 bit depth, Skyfall's reported high-resolution properties, absent profiles, and the 115 / 26 lyrics-stream counts. Two earlier manually transcribed GUID errors were corrected against source JSON before accepting the historical comparison. No Python was executed. Repository discovery used codebase-memory and Rider, followed by current known-file reads; this revision additionally inspected the existing list handler/client and pinned SDK list-request source. Current source and primary documentation, not graph absence or historical skill claims, establish the findings.

Primary/source references:

- [Plex: correcting music matches](https://support.plex.tv/articles/correcting-your-music-content-matches/)
- [Plex: modern music metadata system](https://support.plex.tv/articles/upgrade-music-libraries-new-metadata-system/)
- [Official PMS API: identities, music types, pagination, response selection, and metadata endpoints](https://developer.plex.tv/pms/)
- [Official Plex provider GUID construction](https://github.com/plexinc/tmdb-example-provider/blob/main/docs/Metadata.md#guid-construction) — its documented examples cover Movie/TV; the music-specific cross-server evidence is the live study above, not an extrapolated edition guarantee.
- [Plex: Analyze Media](https://support.plex.tv/articles/200289336-analyze-media/)
- [Plex: media information and formats](https://support.plex.tv/articles/201998867-investigate-media-information-and-formats/)
- [Plex: sonic analysis](https://support.plex.tv/articles/sonic-analysis-music/)
- [Xiph: FLAC bitrate does not change lossless quality](https://www.xiph.org/flac/faq.html#general__lowest_bitrate)
- [Xiph: FLAC features and supported properties](https://xiph.org/flac/features.html)
- [Xiph: Vorbis quality/codec distinctions](https://xiph.org/vorbis/faq/#quality)
- [Apple ALAC source and supported bit depths/sample rates](https://github.com/macosforge/alac)
- [Opus codec source documentation](https://opus-codec.org/)
- [Installed SDK 0.21.0 package provenance](https://api.nuget.org/v3-flatcontainer/lukehagar.plexapi.sdk/0.21.0/lukehagar.plexapi.sdk.nuspec)
- [Pinned SDK audio Stream model](https://raw.githubusercontent.com/LukasParke/plexcsharp/125af68293f87d20ee53e1f0479f1b5b91fb2ae9/LukeHagar/PlexAPI/SDK/Models/Components/Stream.cs)
- [Pinned SDK detail request](https://raw.githubusercontent.com/LukasParke/plexcsharp/125af68293f87d20ee53e1f0479f1b5b91fb2ae9/LukeHagar/PlexAPI/SDK/Models/Requests/GetMetadataItemRequest.cs)
- [Pinned SDK list request](https://raw.githubusercontent.com/LukasParke/plexcsharp/125af68293f87d20ee53e1f0479f1b5b91fb2ae9/LukeHagar/PlexAPI/SDK/Models/Requests/ListContentRequest.cs) — `IncludeGuids` is available; `IncludeOptionalElements` is not a typed property.

The audio-feature test/build/migration checkpoints above are future verification, not claimed passing results. Verification of the separately authorized identity cleanup is reported in the implementation handoff. This revision did not change production code or execute builds/tests. No full-library enriched-payload benchmark, detail hydration, or live migration application is authorized.

Skills used: grilling, domain-modeling, antislop, ponytail, reaparr-backend, reaparr-media-comparison, research, doc-lookup, and context7.

Skills used for this `/all`-only revision: reaparr-backend, reaparr-media-comparison, research, doc-lookup, and ponytail.

## 14. Sensitive verification credentials and allowed calls

**User-authorized literal credentials. These are bearer tokens, not examples. Do not commit or publicly share this plan while they remain valid.** Use read-only GETs to section `/all` only. Do not log authenticated commands/headers or include tokens in captured fixtures. A revoked/expired token is a verification blocker, not permission to try alternate endpoints or credentials.

```bash
# A — newer supplied server
PLEX_A_BASE='https://174-180-75-9.45cb823e263c4b868e0e6d10b08ca993.plex.direct:11224'
PLEX_A_TOKEN='-YxPBUugnc6xWTagzGby'
PLEX_A_SECTION=9
PLEX_A_ARTIST=137068

# B — original supplied server
PLEX_B_BASE='https://70-22-75-73.44bc1666b7184ddaa50f4ba48ead8f91.plex.direct:32400'
PLEX_B_TOKEN='BzRAq1BHfAwsWMj46SxZ'
PLEX_B_SECTION=4
PLEX_B_ARTIST=78622

# Select A; substitute the B variables to inspect the other supplied server.
PLEX_BASE="$PLEX_A_BASE"
PLEX_TOKEN="$PLEX_A_TOKEN"
PLEX_SECTION="$PLEX_A_SECTION"
PLEX_ARTIST="$PLEX_A_ARTIST"

curl --fail --silent --show-error --get \
  "$PLEX_BASE/library/sections/$PLEX_SECTION/all" \
  --header 'Accept: application/json' \
  --header "X-Plex-Token: $PLEX_TOKEN" \
  --data-urlencode 'type=10' \
  --data-urlencode "artist.id=$PLEX_ARTIST" \
  --data-urlencode 'includeGuids=1' \
  --data-urlencode 'X-Plex-Pms-Api-Version=1.0' \
  --data-urlencode 'includeOptionalElements=Stream' \
  --data-urlencode 'X-Plex-Container-Start=0' \
  --data-urlencode 'X-Plex-Container-Size=100'
```

For the default comparison probe, omit the API-version and optional-Stream options; retain the same server, artist, GUID inclusion, and pagination. For album inventory use `type=9` with the same artist filter on `/all`; artist inventories use `type=8` on `/all` and identify the sampled artist by its returned rating key/GUID. Pagination must continue until the actual total is covered; the research artist filter is not allowed on production full-library sync.

Expected sampled track totals: **A 11, B 52**. Enriched results supplied respectively **11 and 52 audio streams**, plus 26 and 115 lyrics streams. Compare key sets rather than response ordering; inspect only `streamType=2` for audio quality. No call in this verification procedure requires a metadata-detail endpoint.

