/**
 * Semantic Release Configuration
 *
 * How versioning works (forward-looking prereleases):
 *
 * semantic-release analyzes commits since the last tag and computes the NEXT
 * version based on conventional commit types (feat → minor, fix → patch, etc.).
 * This is stateless - there is no "memory" of a separate stable vs dev version.
 *
 * The STABLE_RELEASE environment variable controls the prerelease suffix:
 *
 *   STABLE_RELEASE unset/false → appends "-dev.N" to the computed version
 *   STABLE_RELEASE=true        → publishes the computed version as stable
 *
 * Example flow:
 *   - Last tag: v1.4.0
 *   - New commit: feat: add feature
 *   - Computed next version: 1.5.0
 *   - Dev prerelease publishes: 1.5.0-dev.1
 *   - Subsequent dev prereleases: 1.5.0-dev.2, 1.5.0-dev.3, ...
 *   - Stable release publishes: 1.5.0 (same computed version, no suffix)
 *   - Next commit after stable: fix: bug → computes 1.5.1 → 1.5.1-dev.1
 *
 * Key behaviors:
 *   - Dev versions are NOT based on "last stable + offset"
 *   - Stable releases are commit-driven, only gated by manual trigger
 *   - No version rollback or custom math - pure semantic-release behavior
 *
 * @type {import('semantic-release').GlobalConfig}
 */

const isStableRelease = process.env.STABLE_RELEASE === 'true';

export default {
  // Workaround for semantic-release issue #2503:
  // https://github.com/semantic-release/semantic-release/issues/2503
  // Prerelease-only branches require a "main" release branch to exist.
  // Adding maintenance branch pattern satisfies this requirement.
  branches: [
    '+([0-9]).x', // Maintenance branches (acts as "main" branch requirement)
    {
      name: 'dev',
      // false → publish computed version as stable (e.g., 1.5.0)
      // 'dev' → append prerelease suffix (e.g., 1.5.0-dev.1)
      prerelease: isStableRelease ? false : 'dev',
    },
  ],
  repositoryUrl: 'https://github.com/Reaparr/Reaparr',
  tagFormat: 'v${version}',
  plugins: [
    [
      '@semantic-release/commit-analyzer',
      {
        preset: 'angular',
        releaseRules: [
          { type: 'docs', scope: 'README', release: 'patch' },
          { type: 'refactor', release: 'patch' },
          { type: 'style', release: 'patch' },
        ],
        parserOpts: {
          noteKeywords: ['BREAKING CHANGE', 'BREAKING CHANGES'],
        },
      },
    ],
    [
      '@semantic-release/release-notes-generator',
      {
        preset: 'angular',
        writerOpts: {
          commitsSort: ['subject', 'scope'],
        },
      },
    ],
    [
      '@semantic-release/changelog',
      {
        changelogFile: 'CHANGELOG.md',
        changelogTitle: 'Reaparr Changelog',
      },
    ],
    [
      '@semantic-release/npm',
      {
        npmPublish: false,
        pkgRoot: './src/AppHost/ClientApp/',
      },
    ],
    [
      '@semantic-release/git',
      {
        assets: ['CHANGELOG.md', './src/AppHost/ClientApp/package.json'],
        message:
          'chore(release): Bump version to ${nextRelease.version} [skip ci]\n\n${nextRelease.notes}',
      },
    ],
    ['semantic-release-export-data'],
    [
      '@semantic-release/github',
      {
        // Disable automatic comments on issues/PRs
        successCommentCondition: false,
        failCommentCondition: false,
      },
    ],
  ],
};

