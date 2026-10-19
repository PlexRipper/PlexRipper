# I37: Return IMDb identifiers that Arr parsers accept

Approved scope: emit digits in the Torznab imdb response attribute, removing a leading tt while preserving leading zeroes. Stored identifiers and numeric/tt-prefixed query normalization stay unchanged. Both Movie/TV search and RSS use the common projection. No schema or dependency changes; no language-attribute changes (I39 deferred).

1. Normalize only the output attribute at the projection boundary; reuse existing identifier formatting logic if its output contract matches.
2. Update affected response expectations; cover tt/numeric IDs, leading zeroes, Movie/TV projections and attrs filtering, plus hosted XML for RSS/search and both query forms returning the same rows.
3. Complete edits before C# diagnostics and targeted unit/hosted checks. User explicitly waived real Sonarr/Radarr execution for I36–I38; report hosted protocol evidence only. Commit I37 separately.
