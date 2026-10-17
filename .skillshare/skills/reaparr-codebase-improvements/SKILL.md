---
name: reaparr-codebase-improvements
description: Use when a user requests a Reaparr codebase audit, ten actionable improvements, an improvement backlog, or user-selected fixes rather than automatic cleanup.
---

# Reaparr Codebase Improvements

Find ten worthwhile, source-grounded improvements. Present their consequences and proposed fixes through the question tool. The user selects what gets implemented; the agent owns investigation, complete implementation of the selected scope, and honest verification.

**Core contract: evidence before proposals; selection before edits; verification before completion.** An invocation authorizes read-only investigation and proposal preparation, not source changes. The target is ten independent improvements, not ten rows at any cost.

This skill is self-contained. It requires no helper scripts, Python interpreter, workflow runner, paid service, or external-model review. Use the host's native read, discovery, semantic navigation, question, editing, and verification capabilities. Repository rules and enforced permissions remain authoritative.

## Quick reference

| Phase | Work | Exit condition |
|---|---|---|
| Scope | Establish boundaries, intended behavior, architecture, and checks | Investigation scope and exclusions are explicit |
| Discover | Trace high-risk and high-friction behavior across relevant layers | Candidate ledger and actual coverage are available |
| Vet | Challenge candidates, deduplicate, rank, define smallest remedies | Ten supported proposals, or an honest documented shortfall |
| Propose | Show evidence, tradeoffs, and validation; ask per-item decisions | Every presented item has an explicit decision or remains pending |
| Implement | Change only approved IDs and their disclosed dependencies | Approved behavior and all affected consumers are implemented |
| Verify | Execute focused checks; exercise runtime where necessary | Each approved item has observed evidence or an explicit blocker |
| Review and commit | Ask whether the user wants to review verified changes before committing | Review approval is pending, or each authorized fix has its own commit |

## 1. Establish the investigation scope

1. Read already-provided project instructions and load applicable repository-specific skills before generic ones. Discover configured build, test, lint, package, and runtime routes from maintained project files. Do not assume a particular stack or command.
2. Resolve the repository and user-specified directories, exclusions, priorities, and constraints. A whole-codebase request is not a branch-only review. Recent changes are leads, not the audit boundary. Never switch branches or discard uncommitted work.
3. State a short scope summary and maintain a task list covering investigation, proposal decisions, selected implementation, verification, and the review/commit decision. Source files remain unchanged during discovery. Do not create repository artifacts unless requested; when the repository requires written plans, use its prescribed location.
4. Inspect the actual entry points, module boundaries, state stores, external integrations, test organization, and canonical patterns. Prefer semantic tooling for contracts and references; verify indexed claims against current source. Start with a few targeted reads rather than opening the entire repository.
5. Infer routine details from available evidence. Use the question tool only for materially different scope choices, inaccessible prerequisites, permission decisions, or acceptance criteria that cannot be inferred. Do not delay an otherwise clear audit for generic preference questions.

**Exit:** record what will be investigated, what is excluded, relevant conventions, and available verification routes. Mention known access or index gaps without treating them as evidence that code is defective.

## 2. Search broadly, then follow consequences deeply

Use a coverage matrix. Mark each relevant lens as inspected, not applicable with a reason, or uninspected with a limit. A glance at one file does not mean a subsystem was audited.

| Lens | Concrete paths to investigate |
|---|---|
| Correctness | Boundary values, precedence, nullability, state transitions, idempotency, partial failure, cancellation |
| Data integrity | Migrations, destructive writes, transaction boundaries, constraints, reconciliation, compatibility |
| Security and privacy | Reachable authorization checks, trust boundaries, input paths, secrets, injection, unsafe file operations |
| Concurrency and reliability | Ordering, stale updates, background jobs, ownership, resource lifetime, retry amplification, shutdown |
| Performance and scale | Query shape, pagination, fan-out, allocation, repeated work, bounded buffers, rendering large collections |
| Contracts and integrations | API payloads, error semantics, generated clients, real-time messages, external service assumptions |
| Test effectiveness | Observable invariants, meaningful assertions, deterministic isolation, missing failure cases, flaky timing |
| Maintainability | Repeated business rules, leaky boundaries, misleading contracts, unnecessary coupling with a concrete consequence |
| UX and accessibility | Recoverable errors, loading/empty states, keyboard interaction, labels, responsive layouts, stale state |
| Delivery and operations | Startup, packaging, platform paths, CI gates, configuration validation, deployment and rollback risks |

Follow complete paths: input -> validation -> decision -> mutation or side effect -> response -> consumer. Check negative paths and existing defenses. Prefer consequential defects and demonstrable friction over stylistic preferences. Do not propose new features as bug fixes.

Maintain a candidate ledger with source locations, evidence, counterevidence, confidence, affected consumers, and open questions. Generate enough candidates to permit rejection; candidate count is not a deliverable. Expand into other relevant modules before declaring a shortfall.

When at least two substantial independent areas genuinely benefit from delegation, define non-overlapping read-only slices and dispatch them together under host limits. Give each reviewer the scope, intended behavior, admission rules, and required evidence. Collect terminal results, challenge them yourself, and report failed coverage. Never delegate merely to inflate reviewer count. Discovery agents make no edits and run no build, lint, test, or formatter commands mid-investigation.

**Exit:** the coverage matrix describes actual work, and every surviving candidate has a traced consequence rather than only a suspicious line.

## 3. Admit, challenge, and rank proposals

For each candidate, answer:

- What intended behavior or documented constraint is violated, or what concrete maintenance cost is demonstrated?
- Where is the reachable path? Which callers, configurations, inputs, or users are affected?
- What existing validator, transaction, lifecycle rule, framework behavior, or test might disprove it?
- Is it already fixed, an intentional tradeoff, a duplicate root cause, or excluded by the user?
- What is the smallest complete change within existing architecture, and what observable check would prove it?

Resolve factual framework/API questions using primary documentation when needed. A TODO, static-analyzer warning, lack of tests, or reviewer confidence alone is not a confirmed defect. Inspect relevant tests and callers before claiming a gap.

Classify admitted proposals:

- **Confirmed defect:** current source and a reachable path establish incorrect behavior; state whether execution reproduced it.
- **Supported improvement:** a concrete design, usability, test, or operational cost is evidenced; describe the tradeoff without pretending the current behavior is a bug.
- **Investigation lead:** material evidence is missing. Keep it outside the ten actionable proposals until resolved.

For performance, distinguish demonstrated unbounded/redundant work from measured impact. Do not invent benchmark results or claim a speedup without measurements. For security, establish reachability and relevant attacker control; otherwise label the uncertainty and withhold exploit claims.

Deduplicate by root cause and remedy. Related symptoms with one fix form one proposal; independent issues sharing a file remain separate. Rank by impact, confidence, reach, and implementation risk. Severity and implementation risk are separate judgments. Small effort is useful only after consequence is established. Seek breadth where valuable, not forced quotas per lens.

Select the ten strongest independent proposals and assign stable IDs **I01-I10**. Once shown, IDs do not move when the user requests details or changes ordering. If fewer than ten survive after reasonable coverage and challenge, present the supported number and explain the shortfall and remaining limits. Never fill slots with disproven claims, duplicate symptoms, guessed vulnerabilities, or generic cleanup.

### Required proposal card

Every card must contain:

- **ID and title**; classification, priority, and confidence.
- **Evidence:** exact file and line range or symbol, relevant caller/test, and the observed source fact. Use repository-relative paths.
- **Consequence:** triggering condition, actual/expected behavior, and who or what is affected. Explicitly label inference and unmeasured incidence.
- **Proposed change:** the smallest remedy, affected modules/consumers, and intended behavior preserved.
- **Tradeoffs and risk:** compatibility, data migration, dependencies, operational needs, and any mutually exclusive alternative.
- **Validation:** the observable regression/invariant, test seam or runtime scenario, and known prerequisites. Distinguish checks already run from checks proposed.
- **Effort:** small/medium/large as relative scope, not fabricated time estimates.

A remedy requiring a schema migration, dependency, API change, new abstraction, or deployment action must disclose that dependency here. It is not hidden implementation detail.

**Exit:** each proposed item is actionable, independently justified, and has a bounded scope and validation plan. An audit may finish with no findings; that is better than fabricated improvements.

## 4. Let the user choose through the question tool

First show a compact ranked overview: ID, title, class, impact, risk, and relative effort. Provide the proposal cards before selection, either in the message or rich question previews. The questions are the actual decision mechanism; a markdown checklist is not a substitute.

Read the available question-tool schema. With a 2-5-options-per-question limit, use **one single-select question per proposal**, with three options, in manageable batches (default: five questions, then the remaining five). Do not cram ten proposals into an invalid ten-option question. Collect all batch decisions before starting edits.

Each question is self-contained:

- **Fix Ixx:** explicitly approves that card's proposed change, disclosed dependencies, and necessary regression coverage.
- **Defer Ixx:** makes no change this run.
- **Details Ixx:** requests explanation or read-only investigation; grants no implementation authority.

Recommend fixes only where evidence and tradeoffs justify the recommendation. A recommended/default option is not consent: require an explicit submitted answer. Omitted, dismissed, timed-out, or unanswered questions stay pending and authorize no edits. If the tool cannot distinguish an untouched default from consent, do not preselect Fix. If the user selects Details, answer it, update material evidence, and ask again for Fix/Defer. A later change to a card's scope invalidates earlier approval for the added work.

Concrete native `ask` example (substitute the actual evidence-backed card):

```json
{
  "i": "Selecting codebase improvements",
  "questions": [{
    "id": "I03",
    "header": "I03 decision",
    "question": "I03: Bound the export queue to prevent unbounded memory growth?",
    "options": [
      {"label": "Fix I03", "description": "Approve the bounded-queue remedy and cancellation regression coverage described in I03.", "preview": null},
      {"label": "Defer I03", "description": "Leave I03 unchanged this run.", "preview": null},
      {"label": "Details I03", "description": "Explain evidence and backpressure tradeoffs before deciding.", "preview": null}
    ],
    "multi": false,
    "recommended": null
  }]
}
```

Maintain a decision ledger: ID -> approved/deferred/details/pending, approved remedy, disclosed dependencies, and verification plan. Explicit free-text selections such as “fix I02 and I07” also authorize precisely those cards. “Interesting,” “looks good,” or “all the safe ones” without an already defined safe set does not identify an approved implementation set; resolve it through clickable choices. An explicit approval of a previously defined set is sufficient without asking the same question again.

If options are mutually exclusive, resolve the remedy before implementation. If an approved item depends on a deferred item, explain the dependency and ask whether to approve the minimum additional scope or defer the dependent fix. Never silently fix the unselected item.

**Question tool unavailable:** deliver the proposal report and exact capability needed to select items; make no source changes. Do not claim clickable selection happened or silently replace it with plain-text questions.

**Exit:** summarize approved, deferred, and pending IDs with their exact boundaries. If none are approved, end with the proposal results and no source changes. If a response is still needed, stop at that explicit decision boundary; do not execute assumed choices.

## 5. Implement only the approved set

After the user submits decisions, continue with the approved set without a redundant confirmation. Read the current implementation and established tests, load the applicable implementation/testing skills, and plan the selected work. Revalidate source evidence if the tree changed since the audit.

Treat each selected card as a bounded behavior change. Update all affected callers, tests, and contracts necessary for that disclosed remedy; preserve unrelated user work and unselected improvements even in the same file. Do not bundle incidental refactors, dependency upgrades, new telemetry, speculative fallbacks, or formatting churn. Do not alter README or CHANGELOG files unless explicitly requested.

If implementation uncovers materially new scope, pause only that item and use the question tool to describe the added work, consequence, and alternatives. Continue independent approved work where possible. Approval does not override enforced tool permissions: request the narrowly scoped operation required, and do not retry denied operations through another tool or agent.

Track consent separately from execution status. For a newly discovered dependency, record the original card as **approved as disclosed**, the added work as **pending approval**, and execution as **blocked on scope decision**. Do not partially implement an inseparable remedy while that decision is unresolved.

For bugs, add regression coverage for the demonstrated failure at an established observable seam. For improvements, verify the claimed invariant, boundary, or transition. Assertions must distinguish a plausible broken implementation from the correct one; wiring-only checks and bare not-throw assertions are insufficient.

Complete all related implementation and test edits before final validation, following repository workflow rules. Avoid mandatory red-before-fix execution when the host requires validation only after edits. Do not weaken tests, suppress diagnostics, or replace real behavior with mocks just to produce a passing result.

**Exit:** every approved item's complete disclosed change is implemented, or its exact blocking prerequisite is identified. Nothing unselected is silently added.

## 6. Verify the approved fixes

Run the smallest repository-approved checks covering the changed behavior after edits are complete. Broaden only for shared-contract impact, a failure, or a real coverage gap. Use actual runtime smoke checks for startup, packaging, integrations, or other behavior that tests cannot prove. Exercise changed UI in a browser. Do not claim visual verification from static inspection.

Collect evidence per approved ID: check/scenario, result, and limits. When a required dependency, database, credential, permission, or tool is unavailable, complete reachable work and distinguish **implemented but unverified**, **blocked**, and **verified**. An unrun test is not a pass; unavailable verification is not proof of a code defect. Do not claim full completion when a required scenario remains unexercised.

Use **implemented but unverified** when the approved edits are complete but required behavioral evidence is missing; also record **completion blocked on verification** and name the prerequisite. Use **implementation blocked** when required edits cannot be completed. Use **verified** only when the required checks or scenarios actually succeeded. These statuses do not change the user's consent ledger.

**Exit:** verification evidence and limits are recorded for each implemented ID. Continue to the review/commit decision for verified changes; leave blocked or unverified items uncommitted.

## 7. Ask about review, then commit each fix separately

After verification, show a concise per-fix summary and the checks that passed. Use the question tool to ask: **“Would you like to review the changes before they are committed?”** Provide one single-select question with these two options and no preselected answer:

| Option | Meaning and next action |
|---|---|
| **Review first** | Present the changes grouped by fix, with affected files, material tradeoffs, and verification evidence. Leave them uncommitted and wait for explicit approval of the reviewed changes. |
| **Commit fixes separately** | The user declines review and explicitly authorizes committing the verified fixes now, with one commit per approved fix. Proceed without asking for a second commit confirmation. |

An unanswered, dismissed, timed-out, or unavailable question leaves changes uncommitted. Fix selection alone authorizes implementation, not commits. If no fixes were implemented, omit this question.

For **Review first**, make the relevant diffs available through the host's review capabilities or a readable per-fix walkthrough. Address feedback within the approved scope and rerun the affected verification before seeking approval of changed results. Ask about added scope separately. Review approval identifies the fixes that may be committed; leave any unapproved fixes uncommitted.

For either commit-authorized path:

1. Follow `reaparr-git-commit` and repository permission rules.
2. Create **one commit per approved, verified improvement ID**, including that fix's implementation and regression tests. Keep independent fixes separate even when they share files; stage their hunks independently rather than combining them. Order dependent fixes so prerequisite commits come first.
3. Stage only changes belonging to that fix. Preserve unrelated user work and keep deferred, pending, blocked, unverified, or review-unapproved changes out of commits.
4. Record the commit hash against its ID. If committing is blocked or denied, preserve the changes and report the exact prerequisite; do not bypass the restriction or claim a commit succeeded.

This decision authorizes local commits only. Pushing, publishing, opening PRs, or filing issues still requires separate explicit authorization.

**Exit:** every commit-authorized fix has its own observed commit hash, or an explicit commit blocker; review-pending changes remain uncommitted.

## 8. Deliver the results

Final output:

1. Investigation scope and actual coverage, including uninspected areas and whether ten supported proposals were found.
2. Decision ledger: fixed, deferred, pending, and blocked IDs.
3. Per implemented ID: behavior changed, affected paths, verification actually exercised, and remaining risk.
4. Exact prerequisites for unresolved selected items, without treating deferred proposals as unfinished authorized work.
5. Review/commit decision, per-fix commit hashes or explicit uncommitted status, and skills used.

## Common mistakes and stop conditions

| Temptation | Correct action |
|---|---|
| “The user asked for ten, so add plausible cleanup.” | Broaden investigation, then report an honest shortfall. |
| “The neighboring bug is obvious and this file is open.” | Keep it deferred; selection applies to IDs and remedies, not files. |
| “Details means they are interested enough to proceed.” | Explain, then obtain an explicit fix decision. |
| “A prose question is close enough to the question tool.” | Use valid tool payloads; if unavailable, stop before implementation. |
| “A small migration is included in any bug-fix approval.” | Compare with the approved card; ask before undisclosed migration scope. |
| “The final test could not run, but the code looks right.” | Report implemented-but-unverified and the missing prerequisite. |
| “The user did not answer the review question, so they probably do not want review.” | Leave changes uncommitted until an explicit decision is submitted. |
| “Several fixes touch this file, so one combined commit is simpler.” | Isolate each approved fix's hunks and tests into its own commit. |

Stop edits for an item when consent is missing, the finding is disproven, a new dependency exceeds its approved card, or the needed operation is denied. Stop at proposal-only completion when nothing is selected. Otherwise continue all actionable approved work through verification, the explicit review/commit decision, and honest delivery.
