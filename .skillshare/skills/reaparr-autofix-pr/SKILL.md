---
name: reaparr-autofix-pr
description: Use on a Reaparr branch that already has an open PR when GitHub CI must be monitored and every dev-test.yml pipeline failure must be fixed incrementally with automatic commits and pushes. Never creates, readies, closes, or merges the PR.
---

# Reaparr PR Pipeline Repair

## Purpose

Repair an already-open Reaparr pull request until its GitHub Actions pipeline is green.

This skill assumes:

- it is invoked from the PR's head branch;
- an open GitHub PR already exists for that branch;
- the PR targets `dev`;
- GitHub CI is the source of truth for which pipeline jobs failed;
- pipeline repair commits should be pushed automatically so CI can rerun.

The operating loop is deliberately incremental:

1. find the existing PR for the current branch;
2. wait for `.github/workflows/dev-test.yml` to run for the current remote HEAD;
3. inspect failed jobs and logs;
4. fix the smallest proven root cause;
5. run focused local verification and the affected gate where practical;
6. commit and push the repair automatically;
7. wait for CI on the new SHA;
8. repeat until the pipeline is green or a genuine blocker requires the user.

**Never merge the PR.** Merging is exclusively the user's responsibility.

## Strict Scope

This skill exists only to fix pipeline failures.

It may:

- inspect the existing PR and workflow runs;
- read failed-job logs, annotations, and artifacts;
- edit code, tests, configuration, workflows, and build files when necessary to fix a demonstrated CI failure;
- run focused and affected local checks;
- create focused repair commits;
- push those commits to the current PR branch;
- wait for and monitor replacement CI runs;
- report when all applicable jobs are green.

It must not:

- create a PR;
- change the PR base branch;
- mark a draft PR ready for review;
- edit PR title/body or labels unless strictly required to diagnose CI and explicitly requested;
- resolve review conversations or dismiss reviews;
- merge, auto-merge, close, or reopen the PR;
- rebase, rewrite history, amend already-pushed commits, or force-push;
- perform unrelated refactors, cleanup, feature work, or review remediation;
- claim the PR is approved or merge-ready beyond reporting pipeline status.

A green pipeline means only that the pipeline is green. The user decides whether and when to merge.

## Authorization Model

Invoking this skill is explicit authorization to make narrowly scoped pipeline-repair commits and push them to the existing current-branch PR without asking before each commit or push.

That standing authorization covers only changes directly justified by a failure in the current GitHub CI run. It does not authorize feature changes, broad refactors, history rewriting, PR metadata changes, branch changes, or merging.

Stop and ask the user when:

- no open PR unambiguously matches the current branch;
- the PR does not target `dev`;
- the checked-out branch is not the PR head branch;
- the local branch, remote PR head, and working tree cannot be reconciled safely;
- unrelated pre-existing local changes overlap the required repair;
- a fix would materially change intended product behavior or public APIs;
- a fix requires weakening a quality gate or making an anti-bypass exception;
- credentials, permissions, runner availability, infrastructure, or external services are the root cause and cannot be repaired in the repository;
- a push is rejected and resolving it would require rebasing, resetting, force-pushing, or overwriting another person's work.

## Required Skills and Tools

Load and follow the matching Reaparr skills before inspecting or changing an affected area:

- Backend: `reaparr-backend` first.
- Backend unit tests: then `reaparr-backend-unit-tests`.
- Backend integration tests: then `reaparr-backend-integration-tests`.
- Frontend: `reaparr-frontend` first.
- Frontend Vitest: then `reaparr-frontend-unit-tests`.
- Frontend Cypress: then `reaparr-frontend-integration-tests`.
- Commits: `reaparr-git-commit`.
- Desktop builds: `reaparr-desktop-build`.

Use Rider MCP for backend work and WebStorm MCP for frontend work as required by those skills. Use GitHub MCP for PR lookup, workflow runs, job logs, annotations, and artifacts. Follow `AGENTS.md` throughout.

## Canonical Pipeline

At the beginning of a repair session, reread:

- `.github/workflows/dev-test.yml`;
- local composite actions invoked by it;
- reusable Docker and desktop workflows invoked by it;
- relevant package scripts and project-specific skills.

The checked-in workflow is canonical. Do not rely on a stale gate list in this skill if the workflow has changed.

The current workflow includes these possible gates:

- `Actionlint` for `.github/**` changes, with shellcheck and pyflakes enabled;
- frontend TypeScript typecheck;
- frontend Vitest unit tests;
- frontend ESLint;
- frontend Cypress integration tests in Firefox, parallelized through Cypress Cloud;
- backend locked restore and solution build on .NET 10;
- every backend TUnit project in the workflow's unit-test matrix;
- backend integration tests;
- code-coverage report generation;
- Docker builds for amd64 and arm64;
- desktop builds for Windows, Linux, and macOS.

Trust the workflow conditions when deciding whether a skipped job is legitimately non-applicable. Docker and desktop reusable workflows are downstream pipeline gates and must be monitored even when frontend/backend path-filtered jobs are skipped.

## Non-Negotiable Repair Rules

- Fix root causes, not symptoms.
- Keep each repair diff minimal, deterministic, and directly traceable to CI evidence.
- Never install host/system packages.
- Never use `rm`, `rmdir`, or `rm -rf`; use `trash` only when cleanup is necessary.
- Never force-push.
- Preserve user-authored and unrelated local changes.
- Never commit logs, coverage output, test results, screenshots, videos, build artifacts, secrets, or unrelated generated files.
- Do not make speculative edits before examining the failed job's complete logs.
- Do not batch unrelated failures into one large repair when they can be fixed and verified independently.
- Do not keep pushing after CI is fully green.

### Strict anti-bypass policy

Never obtain a green pipeline by:

- skipping, disabling, deleting, quarantining, or filtering out a failing test;
- weakening meaningful assertions or replacing them with tautologies;
- blindly updating snapshots or expected data;
- adding blanket lint, type, compiler, or analyzer suppressions;
- reducing coverage or excluding changed code from coverage;
- loosening workflow paths, conditions, required jobs, architectures, or platform targets;
- inflating retries, sleeps, waits, or timeouts to hide nondeterminism;
- swallowing exceptions, ignoring exit codes, or allowing failed commands to succeed;
- changing production behavior solely to satisfy a test whose expectation is incorrect.

A test expectation may change only when CI exposed a genuine obsolete expectation and source/contracts prove the intended behavior changed. Preserve equivalent or stronger coverage and explain this in the repair commit.

## Phase 1 — Bind to the Existing PR

Before changing files:

1. read repository instructions and applicable skills;
2. record the current branch, upstream, local HEAD, remote HEAD, and working-tree status;
3. find the open PR whose head branch is the current branch;
4. verify the PR head repository/branch matches the push target;
5. verify its base is exactly `dev`;
6. record the PR number, URL, draft state, and current head SHA;
7. identify pre-existing staged, unstaged, and untracked files and preserve them.

Do not search for an arbitrary PR to repair. Do not switch to another PR branch automatically. If the current branch has no single matching open PR, stop and ask the user.

If the PR is draft and `Check-PR-Status` prevents the pipeline from running, report that as a blocker. Do not mark it ready.

## Phase 2 — Wait for the Current-SHA Workflow

Do not begin by running every local suite speculatively. First wait for GitHub CI to identify actual pipeline failures.

For the current remote PR head SHA:

1. locate the `dev-test.yml` / `Execute Tests` workflow run triggered for that exact SHA;
2. if it has not appeared yet, poll until it appears;
3. if it is queued or in progress, wait until jobs fail or the run completes;
4. ignore runs belonging to older SHAs;
5. record every failed, cancelled, timed-out, action-required, or unexpectedly missing job;
6. distinguish legitimate conditional skips from pipeline problems.

If the run completes green, stop without editing or pushing and report the green run URL and SHA.

If no run appears because the PR only changes a `paths-ignore` location, report that the workflow is intentionally not triggered. Do not create an empty commit or unrelated change to force CI.

## Phase 3 — Triage Failures from Evidence

For each failing job, retrieve:

- full job and step names;
- the first causal error, not only the final nonzero exit message;
- complete relevant stack traces and annotations;
- matrix project, platform, architecture, runtime, browser, and command;
- artifacts such as TRX, coverage diagnostics, Cypress screenshots/videos, or build logs when useful;
- whether other failures are downstream consequences of the same cause.

Group failures only when logs prove a common root cause. Prioritize:

1. workflow syntax/action failures that prevent other jobs from running;
2. restore/build failures;
3. earliest causal application or test failure;
4. independent unit/type/lint failures;
5. integration failures;
6. coverage/reporting failures;
7. Docker and desktop platform-specific failures.

For Cypress, fix the earliest uncaught application error before secondary connection or port errors. A test that passes only after CI retries remains suspect and should be investigated when it caused the failed run.

## Phase 4 — Incremental Fix Loop

Handle the smallest coherent failure group at a time:

1. reproduce the failure locally when practical;
2. trace the failing path and establish intended behavior from source and contracts;
3. implement the smallest root-cause fix;
4. run the focused failing test/check;
5. run the complete affected gate where practical;
6. inspect diagnostics and the complete repair diff;
7. ensure only intended files will be committed;
8. create a focused Reaparr-conformant commit;
9. push to the existing PR branch without force;
10. verify the PR head SHA updated to the pushed commit;
11. wait for the workflow run for that new exact SHA;
12. inspect the new CI result and repeat.

Automatic commit/push is part of this loop. Do not ask for confirmation for each narrowly scoped repair.

### Local verification commands

Use local checks to shorten feedback, but GitHub CI remains authoritative.

For frontend gates, from the repository root:

```bash
bun --cwd src/AppHost/ClientApp run typecheck
bun --cwd src/AppHost/ClientApp run unit-test
bun --cwd src/AppHost/ClientApp run lint
bun --cwd src/AppHost/ClientApp run cypress:ci
```

Focused Vitest/Cypress runs are diagnostic only. When practical, run the complete affected gate before pushing. Do not substitute `lint:fix` for the final `lint` verification.

For backend solution compilation, use Rider MCP's exact solution-build tool:

```text
rider_build_solution
```

Invoke it with `projectPath` pointing to this repository and no `filesToRebuild` so Rider builds the currently opened full solution. Use `rebuild: true` only when a clean rebuild is needed to reproduce or disprove stale-output behavior. Inspect and resolve every returned build problem before pushing. Do not substitute shell `dotnet restore` or `dotnet build` commands for this skill's local solution-build verification.

For tests, match the TUnit/Microsoft.Testing.Platform behavior in the current workflow through the applicable test MCP tooling. Use focused filters during diagnosis, then run the complete affected project before pushing when practical. Follow the backend test skills and `dotnet-test-mcp` requirements.

For hosted-only gates—Cypress Cloud parallel recording, amd64/arm64 Docker runners, Windows/Linux/macOS desktop builds, or credential-dependent steps—do not install host packages or request secrets merely to recreate CI. Make the evidence-based repository fix and require the next GitHub run to prove it.

### Commit discipline

Before every automatic repair commit:

- inspect staged, unstaged, and untracked files;
- stage explicit intended paths only—never use indiscriminate `git add .`;
- exclude pre-existing user changes and generated artifacts;
- inspect the staged diff;
- use `reaparr-git-commit` conventions;
- include no AI attribution;
- do not amend a pushed commit;
- push normally to the established upstream PR branch.

Use separate commits for independent root causes unless one change necessarily fixes them together.

## Phase 5 — Evaluate Every Replacement Run

After each push, start evaluation from the new SHA. Previous green jobs are stale evidence.

A replacement run can reveal:

- the original failure remains: revisit the diagnosis rather than stacking speculative edits;
- the original failure is fixed but another job now fails: begin a new incremental repair;
- a transient infrastructure failure occurs: verify from logs; rerun through the permitted GitHub mechanism if appropriate, but do not change code to disguise infrastructure problems;
- a permission/secret/runner outage blocks CI: report the blocker and stop when repository code cannot resolve it;
- all applicable jobs pass: finish without further commits.

A cancelled, skipped, neutral, or missing job is not automatically success. Confirm from `dev-test.yml` that a skip is expected. Required checks must be conclusively successful on the current PR head SHA.

## Completion Condition

The repair loop is complete only when the workflow for the current PR head SHA shows:

- no failed, cancelled, timed-out, action-required, or unexpectedly missing required jobs;
- every path-applicable frontend/backend/actionlint job is successful;
- backend matrix children and integration/coverage jobs are successful when applicable;
- Docker amd64 and arm64 jobs are successful;
- desktop Windows, Linux, and macOS jobs are successful;
- every skipped job is explainable by the checked-in workflow condition.

Then stop. Do not merge, enable auto-merge, mark ready, resolve reviews, or make cleanup commits.

## Completion Report

Report pipeline evidence only:

- PR number and URL;
- final PR head SHA;
- each failure root cause and the focused commit that fixed it;
- local checks run and their outcomes;
- final GitHub workflow run URL and status;
- any legitimately skipped jobs and their workflow conditions;
- any blocker if the pipeline could not be made green.

End with one of these precise outcomes:

- **Pipeline green — PR left open for the user to review and merge.**
- **Pipeline repair blocked — PR left open; user action required:** followed by the blocker.

Never state that the PR was merged or that the skill will merge it.
