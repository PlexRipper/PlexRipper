---
name: reaparr-autofix-pr
description: Use on a Reaparr branch that already has an open PR when GitHub CI must be monitored and every dev-test.yml pipeline failure must be repaired in evidence-based batches with focused automatic commits and pushes. Never creates, readies, closes, or merges the PR.
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

The operating loop is evidence-driven and batch-oriented:

1. find the existing PR for the current branch;
2. wait for `.github/workflows/dev-test.yml` to complete for the current remote HEAD;
3. inspect every failed job, complete logs, annotations, and relevant artifacts;
4. diagnose all independent proven root causes from that run;
5. perform a bounded failure-family sweep for equivalent occurrences;
6. fix the complete proven repair batch, using separate focused commits when appropriate;
7. run focused checks and the complete affected gates locally when practical;
8. push the verified commit batch once;
9. wait for CI on the new SHA and repeat until green or genuinely blocked.

Minimize expensive workflow runs without sacrificing evidence or verification. Commit granularity and push granularity are independent: keep commits focused, but normally push all verified repairs from one completed run together.

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
- Do not make speculative edits before examining the failed job's complete logs and relevant artifacts.
- Diagnose every failure in the completed current-SHA run before editing or pushing.
- Keep unrelated repairs in separate focused commits, but batch their push after all are locally verified.
- Before pushing, perform a bounded failure-family sweep across sibling tests, mirrored implementations, direct usages, and workflow matrices. Fix only equivalent occurrences proven by source evidence.
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
- uploaded TRX, screenshots, videos, reports, or diagnostic artifacts when logs omit the causal detail;
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

When a failure log is incomplete:

1. list all artifacts for the exact run ID;
2. do not assume the expected artifact name is present—matrix expressions may produce incomplete names such as `test-results-`;
3. associate artifacts by job, creation time, file contents, and project name;
4. inspect the relevant TRX or report for inner exceptions and full test output before editing.

Classify each failure before changing code:

- **deterministic repository failure:** reproducible locally or conclusively proven by source/configuration evidence;
- **likely repository failure:** repeated with the same causal signature and a supported code path;
- **transient infrastructure/runner failure:** download, runner, service, port, or process failure without a demonstrated repository cause;
- **unknown:** evidence is insufficient.

Do not change repository code for transient or unknown failures. Re-run locally or through the permitted GitHub mechanism to gather evidence first. A local test-tool timeout is neither a test failure nor successful verification; retry with a focused command, inspect the still-running process/output when available, or use another project-approved test tool.

## Phase 4 — Batched Repair Loop

Process one completed current-SHA run as a repair batch:

1. retrieve evidence for every failing job before editing;
2. reproduce each deterministic or likely repository failure locally when practical;
3. trace each failing path and establish intended behavior from source and contracts;
4. perform a bounded failure-family sweep:
   - inspect sibling or mirrored tests and implementations;
   - search all direct usages of the changed contract, command, entity, or helper;
   - compare workflow matrices against files/projects that actually exist;
   - inspect equivalent insert, serialization, or mock setup patterns;
5. implement the smallest root-cause fixes for all proven occurrences;
6. run every focused failing test/check;
7. run the complete affected gate locally unless it is hosted-only, prohibitively unavailable, or a project-approved tool cannot complete it;
8. inspect diagnostics and the complete repair diff;
9. create separate focused Reaparr-conformant commits for independent root causes;
10. after all batch repairs are verified, push the commit batch once without force;
11. verify the PR head SHA updated to the final pushed commit;
12. wait for the workflow run for that new exact SHA;
13. inspect every job in the replacement run and repeat only for newly evidenced failures.

Automatic commit/push is part of this loop. Do not ask for confirmation for each narrowly scoped repair. Do not push immediately after the first fix while other failures from the same completed run remain undiagnosed or while a bounded family sweep is incomplete.

### Local verification commands

Use local checks to shorten feedback, but GitHub CI remains authoritative.

For frontend gates, from the repository root:

```bash
bun --cwd src/AppHost/ClientApp run typecheck
bun --cwd src/AppHost/ClientApp run unit-test
bun --cwd src/AppHost/ClientApp run lint
bun --cwd src/AppHost/ClientApp run cypress:ci
```

Focused Vitest/Cypress runs are diagnostic only. Run the complete affected gate before pushing whenever the gate is locally available. If it cannot be completed, record the exact blocker or tool timeout and rely on the replacement GitHub run; do not silently treat incomplete execution as verification. Do not substitute `lint:fix` for the final `lint` verification.

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
- do not amend a pushed commit.

Use separate commits for independent root causes unless one change necessarily fixes them together. Do not push after each commit by default. After the entire current-run repair batch passes local verification, push all focused commits normally to the established upstream PR branch in one operation.

### Pre-push consistency checks

Before pushing the batch:

- verify every workflow matrix project/path exists and every removed or renamed project is absent;
- verify changed workflow references, local actions, reusable workflows, package scripts, and artifact names are internally consistent;
- search for equivalent occurrences of each demonstrated defect family;
- run `git diff --check`, inspect every staged diff, and confirm unrelated working-tree changes remain unstaged;
- confirm all focused checks passed and record whether each complete affected gate passed, timed out, or was hosted-only.

## Phase 5 — Evaluate Every Replacement Run

After each push, start evaluation from the new SHA. Previous green jobs are stale evidence.

A replacement run can reveal:

- the original failure remains: revisit the diagnosis rather than stacking speculative edits;
- the original failure is fixed but another job now fails: collect all failures from the completed replacement run and begin a new batched repair;
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
- failure-family sweeps performed and any equivalent occurrences fixed;
- local focused checks and complete affected gates run, including explicit timeouts or hosted-only omissions;
- final GitHub workflow run URL and status;
- any legitimately skipped jobs and their workflow conditions;
- any blocker if the pipeline could not be made green.

End with one of these precise outcomes:

- **Pipeline green — PR left open for the user to review and merge.**
- **Pipeline repair blocked — PR left open; user action required:** followed by the blocker.

Never state that the PR was merged or that the skill will merge it.
