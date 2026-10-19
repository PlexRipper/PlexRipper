---
name: reaparr-codebase-improvements
description: Use when a user requests a Reaparr codebase audit, ten actionable improvements, an improvement backlog, or user-selected fixes rather than automatic cleanup.
---

# Reaparr Codebase Improvements

Find ten worthwhile, source-grounded improvements, prioritizing bugs and broken behavior, then performance, then general improvements. Explain each problem and fix in plain English through the question tool. The user selects what gets implemented; the agent owns investigation, complete implementation of the selected scope, and honest verification.

**Core contract: evidence before proposals; selection before edits; verification before completion.** An invocation authorizes read-only investigation and proposal preparation, not source changes. Recording explicit denials in root-level `IMPROVEMENTS.MD` is the only write allowed before fix selection. Present proposals and decision questions in batches of ten; use a smaller batch only when fewer supported proposals remain. Never invent findings to fill a batch.

This skill is self-contained. It requires no helper scripts, Python interpreter, workflow runner, paid service, or external-model review. Use the host's native read, discovery, semantic navigation, question, editing, and verification capabilities. Repository rules and enforced permissions remain authoritative.

## Quick reference

| Phase | Work | Exit condition |
|---|---|---|
| Scope | Establish boundaries, intended behavior, architecture, and checks | Investigation scope and exclusions are explicit |
| Discover | Trace high-risk and high-friction behavior across relevant layers | Candidate ledger and actual coverage are available |
| Vet | Challenge candidates, deduplicate, rank, define smallest remedies | Ten supported proposals, or an honest documented shortfall |
| Propose | Show ten proposals, ask per-item decisions, then ask how to handle review and commits | Item decisions and the batch workflow choice are explicit |
| Implement | Change one approved ID and its disclosed dependencies at a time | That fix and all affected consumers are implemented |
| Verify | Execute focused checks for the current fix; exercise runtime where necessary | The current fix has observed evidence or an explicit blocker |
| Review and commit | Follow the selected review mode and commit the current fix before starting the next | That fix has its own commit, or progress pauses at an explicit approval gate or blocker |

## 1. Establish the investigation scope

1. Read already-provided project instructions and load applicable repository-specific skills before generic ones. Discover configured build, test, lint, package, and runtime routes from maintained project files. Do not assume a particular stack or command.
2. Resolve the repository and user-specified directories, exclusions, priorities, and constraints. A whole-codebase request is not a branch-only review. Recent changes are leads, not the audit boundary. Never switch branches or discard uncommitted work.
3. Read root-level `IMPROVEMENTS.MD` if it exists and exclude previously denied proposals. State a short scope summary and use the host's todo tool to track investigation, proposal decisions, selected implementation, verification, and commits. A prose checklist is not a substitute. Source files remain unchanged during discovery. Apart from the required denial record, do not create repository artifacts unless requested; when the repository requires written plans, use its prescribed location.
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

Follow complete paths: input -> validation -> decision -> mutation or side effect -> response -> consumer. Check negative paths and existing defenses. Spend investigation effort in the priority order below: actively trace failure cases before looking for performance gains or general cleanup. Do not propose new features as bug fixes.

Maintain a candidate ledger with source locations, evidence, counterevidence, confidence, affected consumers, and open questions. Generate enough candidates to permit rejection; candidate count is not a deliverable. Expand into other relevant modules before declaring a shortfall.

When at least two substantial independent areas genuinely benefit from delegation, define non-overlapping read-only slices and dispatch them together under host limits. Give each reviewer the scope, intended behavior, admission rules, and required evidence. Collect terminal results, challenge them yourself, and report failed coverage. Never delegate merely to inflate reviewer count. Discovery agents make no edits and run no build, lint, test, or formatter commands mid-investigation.

**Exit:** the coverage matrix describes actual work, and every surviving candidate has a traced consequence rather than only a suspicious line.

## 3. Admit, challenge, and rank proposals

For each candidate, answer:

- What intended behavior or documented constraint is violated, or what concrete maintenance cost is demonstrated?
- Where is the reachable path? Which callers, configurations, inputs, or users are affected?
- What existing validator, transaction, lifecycle rule, framework behavior, or test might disprove it?
- Is it already fixed, an intentional tradeoff, a duplicate root cause, excluded by the user, or previously denied in `IMPROVEMENTS.MD`?
- What is the smallest complete change within existing architecture, and what observable check would prove it?

Resolve factual framework/API questions using primary documentation when needed. A TODO, static-analyzer warning, lack of tests, or reviewer confidence alone is not a confirmed defect. Inspect relevant tests and callers before claiming a gap.

Classify admitted proposals:

- **Confirmed defect:** current source and a reachable path establish incorrect behavior; state whether execution reproduced it.
- **Supported improvement:** a concrete design, usability, test, or operational cost is evidenced; describe the tradeoff without pretending the current behavior is a bug.
- **Investigation lead:** material evidence is missing. Keep it outside the ten actionable proposals until resolved.

For performance, distinguish demonstrated unbounded/redundant work from measured impact. Do not invent benchmark results or claim a speedup without measurements. For security, establish reachability and relevant attacker control; otherwise label the uncertainty and withhold exploit claims.

### Priority order

Use this order both to guide the search and to rank the final list:

1. **Bugs and broken behavior:** wrong results, lost or corrupted data, security flaws, crashes, failed downloads, unreliable jobs, or broken user interactions. Give the highest priority to issues with the greatest harm and strongest evidence.
2. **Performance improvements:** slow queries, repeated work, excessive memory use, or work that grows without a limit. Explain where the waste occurs and distinguish measured slowdowns from source-supported concerns.
3. **General improvements:** clearer code, easier maintenance, stronger tests, or usability improvements where current behavior is not broken. Include only items with a concrete benefit, not style preferences.

Within each group, rank by impact, confidence, reach, and implementation risk. Keep severity separate from the risk of making the change. An easy cleanup must not outrank a supported bug just because it is quick to implement. An unconfirmed bug stays an investigation lead rather than displacing an evidence-backed proposal.

Deduplicate by root cause and remedy. Related symptoms with one fix form one proposal; independent issues sharing a file remain separate. Seek breadth where valuable, not forced quotas per lens.

Select the ten strongest independent proposals not previously denied and assign stable IDs starting at **I01**, or after the highest ID already recorded in `IMPROVEMENTS.MD`. Continue numbering for later requested batches. Once shown, IDs do not move when the user requests details or changes ordering. If fewer than ten survive after reasonable coverage and challenge, present the supported number and explain the shortfall and remaining limits. Never fill slots with disproven claims, duplicate symptoms, guessed vulnerabilities, or generic cleanup.

### Required proposal card

Use short sentences and everyday words in proposal cards, ranked summaries, question options, plans, and final explanations. Lead with **what goes wrong**, **who it affects**, and **what the fix changes**. Keep paths, symbols, and technical proof in the evidence details. Use technical terms only when needed, explaining unfamiliar terms briefly on first use.

For example: “A failed download can stay marked as running, so it never restarts. Clear that status when the download fails so it can be retried.” Prefer this to “Reconcile terminal-state propagation to restore retry eligibility.”

Every card must contain:

- **ID and title**; classification, priority, and confidence.
- **Evidence:** exact file and line range or symbol, relevant caller/test, and the observed source fact. Use repository-relative paths.
- **Problem:** in plain English, what goes wrong, when it happens, what should happen, and who or what is affected. Explicitly label inference and unmeasured incidence.
- **Fix:** in plain English, the smallest change that solves the problem, affected modules/consumers, and behavior that stays the same.
- **Tradeoffs and risk:** compatibility, data migration, dependencies, operational needs, and any mutually exclusive alternative.
- **Validation:** the observable regression/invariant, test seam or runtime scenario, and known prerequisites. Distinguish checks already run from checks proposed.
- **Effort:** small/medium/large as relative scope, not fabricated time estimates.

A remedy requiring a schema migration, dependency, API change, new abstraction, or deployment action must disclose that dependency here. It is not hidden implementation detail.

**Exit:** each proposed item is actionable, independently justified, and has a bounded scope and validation plan. An audit may finish with no findings; that is better than fabricated improvements.

## 4. Let the user choose through the question tool

Present each batch of ten proposals together: first a compact ranked overview with ID, title, class, impact, risk, and relative effort, then the proposal cards in the message or rich question previews. Present fewer only when fewer supported proposals remain. The questions are the actual decision mechanism; a markdown checklist is not a substitute.

Read the available question-tool schema. Submit **ten single-select proposal questions in one question-tool call**, one per proposal, with the four options below. Append the workflow question described below as the final question: a full batch has **ten proposal questions plus one workflow question**. A smaller batch has one question per supported proposal plus the workflow question. An options-per-question limit is not a questions-per-call limit: do not combine ten proposals into one ten-option question or split a batch into five-question calls. If the tool explicitly limits questions per call, explain that limit and use the fewest supported calls, keeping the workflow question last. Resolve the presented batch's decisions before presenting another batch or starting source edits.

Each question is self-contained:

- **Fix Ixx:** explicitly approves that card's proposed change, disclosed dependencies, and necessary regression coverage.
- **Defer Ixx:** makes no change this run; the item may be proposed in a future audit.
- **Deny Ixx:** rejects the proposal and records it in `IMPROVEMENTS.MD` so future audits do not propose it again.
- **Details Ixx:** requests explanation or read-only investigation; grants no implementation authority.

Recommend fixes only where evidence and tradeoffs justify the recommendation. A recommended/default option is not consent: require an explicit submitted answer. Omitted, dismissed, timed-out, or unanswered questions stay pending and authorize no edits or denial records. If the tool cannot distinguish an untouched default from consent, do not preselect Fix. If the user selects Details, answer it, update material evidence, and ask again for Fix/Defer/Deny; these follow-up questions cover only unresolved items. A later change to a card's scope invalidates earlier approval for the added work.

Concrete native `ask` example for one question (include one such entry per proposal in the batch's `questions` array):

```json
{
  "i": "Selecting codebase improvements",
  "questions": [{
    "id": "I03",
    "header": "I03 decision",
    "question": "I03: Bound the export queue to prevent unbounded memory growth?",
    "options": [
      {"label": "Fix I03", "description": "Approve the bounded-queue remedy and cancellation regression coverage described in I03.", "preview": null},
      {"label": "Defer I03", "description": "Leave I03 unchanged this run; it may be proposed later.", "preview": null},
      {"label": "Deny I03", "description": "Record I03 in IMPROVEMENTS.MD and exclude it from future proposals.", "preview": null},
      {"label": "Details I03", "description": "Explain the evidence and how limiting the queue affects downloads before deciding.", "preview": null}
    ],
    "multi": false,
    "recommended": null
  }]
}
```

Maintain a decision ledger: ID -> approved/deferred/denied/details/pending, approved remedy, disclosed dependencies, and verification plan. Explicit free-text selections such as “fix I02 and I07” also authorize precisely those cards. “Interesting,” “looks good,” or “all the safe ones” without an already defined safe set does not identify an approved implementation set; resolve it through clickable choices. An explicit approval of a previously defined set is sufficient without asking the same question again.

### Final batch question: review and commits

After the proposal questions, ask: **“Would you like to review each fix before it is committed, or should I complete all selected fixes without review pauses?”** Use one single-select question with these two options and no preselected answer:

| Option | Meaning and authorization |
|---|---|
| **Review each fix** | Implement and verify one selected fix, then show its changes and wait for explicit approval to commit it. Commit that fix after approval, then start the next. |
| **Implement all selected fixes** | Authorize implementing, verifying, and locally committing every selected fix without review pauses. Finish and commit one fix before starting the next; do not implement the whole batch first. |

Record this choice with the batch's decision ledger. It applies only to explicitly selected fixes in that batch, not deferred, denied, pending, or added scope. Fix selection alone does not authorize commits. An unanswered, dismissed, timed-out, or unavailable workflow question leaves the mode pending: save explicit denials, but do not start source edits. If no fixes were selected, the workflow choice has no effect and creates no commit authorization. Do not ask the same workflow question again after verification when it has already been answered.

### Remember denied proposals

Maintain `IMPROVEMENTS.MD` at the repository root. Create it when recording the first explicit denial if it does not exist. For each denied proposal, record its ID, date, plain-English problem, proposed fix, affected paths or symbols, and the user's reason if supplied. If no reason was given, write “Not provided”; do not ask for one just to fill the record. Preserve existing entries and unrelated content.

Compare future candidates against this record by the underlying problem and proposed fix, not just title, ID, or file name. Renaming or moving code does not make a denied proposal new. Exclude matching proposals from future batches unless the user explicitly asks to reconsider them; new evidence alone does not cancel a denial. Deferral, a request for details, and an unanswered question are not denials. If the record cannot be read or updated, report the exact blocker rather than claiming duplicate prevention or persistence succeeded.

If options are mutually exclusive, resolve the remedy before implementation. If an approved item depends on a deferred or denied item, explain the dependency and ask whether to approve the minimum additional scope or defer the dependent fix. Never silently fix the unselected item.

**Question tool unavailable:** deliver the proposal report and exact capability needed to select items; make no source changes. Do not claim clickable selection happened or silently replace it with plain-text questions.

**Exit:** summarize approved, deferred, denied, and pending IDs with their exact boundaries, save explicit denials in `IMPROVEMENTS.MD`, and record the batch's review/commit mode. If none are approved, end with the proposal results and no source changes. If an item decision or workflow answer is still needed, stop at that explicit decision boundary; do not execute assumed choices.

## 5. Implement only the approved set

After the user submits item decisions and the batch workflow choice, continue without a redundant confirmation. Work through selected fixes sequentially in ranked order, putting approved prerequisites first. For each fix, complete sections 5-7—implementation, verification, any required review, and its own commit—before starting source edits for the next fix. Read the current implementation and established tests, load applicable implementation/testing skills, and plan that fix. Revalidate source evidence if the tree changed since the audit.

### Track approved fixes with the todo tool

Before source edits, add one todo item for every approved improvement, using its stable ID and a short plain-English title, such as **I03: Allow failed downloads to retry**. Keep them in execution order, with approved prerequisites first. Deferred, denied, details-only, and unanswered proposals are not implementation tasks; keep those decisions in the decision ledger.

- Keep only the current fix in progress. Leave later fixes pending; do not implement fixes in parallel.
- Keep the current item open through implementation, regression tests, verification, review when requested, and its own commit. Finishing the code edits alone does not complete the todo.
- If review approval, scope approval, verification, or committing is blocked, record the reason on the current item and pause the queue. If the todo tool automatically advances to another item, pause that item too; automatic advancement does not authorize starting another fix.
- Record the observed commit hash against the improvement ID, then mark its todo complete and start the next approved item. Never mark a fix complete based on an intended commit or an unrun check.
- Update the todo tool as status changes and consult it when resuming work. Keep its execution status consistent with the decision ledger, without treating task status as approval.

Treat each selected card as a bounded behavior change. Update all affected callers, tests, and contracts necessary for that disclosed remedy; preserve unrelated user work and unselected improvements even in the same file. Do not bundle incidental refactors, dependency upgrades, new telemetry, speculative fallbacks, or formatting churn. Do not alter README or CHANGELOG files unless explicitly requested.

If implementation uncovers materially new scope, pause that fix and use the question tool to describe the added work, consequence, and alternatives. Read-only investigation may continue, but do not start another fix's source edits while the current fix is unresolved. Approval does not override enforced tool permissions: request the narrowly scoped operation required, and do not retry denied operations through another tool or agent.

Track consent separately from execution status. For a newly discovered dependency, record the original card as **approved as disclosed**, the added work as **pending approval**, and execution as **blocked on scope decision**. Do not partially implement an inseparable remedy while that decision is unresolved.

For bugs, add regression coverage for the demonstrated failure at an established observable seam. For improvements, verify the claimed invariant, boundary, or transition. Assertions must distinguish a plausible broken implementation from the correct one; wiring-only checks and bare not-throw assertions are insufficient.

Complete all related implementation and test edits for the current fix before its final validation, following repository workflow rules. Avoid mandatory red-before-fix execution when the host requires validation only after edits. Do not weaken tests, suppress diagnostics, or replace real behavior with mocks just to produce a passing result.

**Exit for the current fix:** its complete disclosed change is implemented, or its exact blocking prerequisite is identified. Nothing unselected is silently added. Proceed to verification for this fix, not implementation of the next.

## 6. Verify the approved fixes

Run the smallest repository-approved checks covering the changed behavior after edits are complete. Broaden only for shared-contract impact, a failure, or a real coverage gap. Use actual runtime smoke checks for startup, packaging, integrations, or other behavior that tests cannot prove. Exercise changed UI in a browser. Do not claim visual verification from static inspection.

Collect evidence per approved ID: check/scenario, result, and limits. When a required dependency, database, credential, permission, or tool is unavailable, complete reachable work and distinguish **implemented but unverified**, **blocked**, and **verified**. An unrun test is not a pass; unavailable verification is not proof of a code defect. Do not claim full completion when a required scenario remains unexercised.

Use **implemented but unverified** when the approved edits are complete but required behavioral evidence is missing; also record **completion blocked on verification** and name the prerequisite. Use **implementation blocked** when required edits cannot be completed. Use **verified** only when the required checks or scenarios actually succeeded. These statuses do not change the user's consent ledger.

**Exit for the current fix:** verification evidence and limits are recorded. If verified, follow the batch's selected review/commit mode. Leave blocked or unverified changes uncommitted and pause the sequence; report the missing prerequisite.

## 7. Review and commit the current fix, then continue

Show a concise summary of the current fix and the checks that passed. Follow the workflow choice made at the end of the batch questions:

- **Review each fix:** make this fix's diff available through the host's review capabilities or a readable walkthrough, with affected files, tradeoffs, and verification evidence. Use the question tool to ask for explicit approval to commit this fix, or feedback requesting changes. Leave it uncommitted until approval. Address feedback within the approved scope and rerun affected verification before seeking approval of changed results. Ask separately about added scope. An unanswered or unavailable approval question pauses the sequence; do not start the next fix.
- **Implement all selected fixes:** the batch answer already authorizes this verified fix's local commit. Commit it now without a second review or commit confirmation, then continue to the next selected fix.

For either commit-authorized path:

1. Follow `reaparr-git-commit` and repository permission rules.
2. Create **one commit for the current approved, verified improvement ID**, including its implementation and regression tests. Do not combine IDs or accumulate multiple fixes for later commits.
3. Stage only changes belonging to this fix. Preserve unrelated user work and keep deferred, denied, pending, blocked, unverified, or review-unapproved changes out of the commit. Do not bundle unrelated `IMPROVEMENTS.MD` denial records into a fix commit.
4. Record the observed commit hash against its ID, mark that fix's todo complete, then start the next approved todo item. If committing is blocked or denied, preserve the changes, mark the current item blocked, pause the queue, and report the exact prerequisite; do not bypass the restriction or claim a commit succeeded.

The workflow choice authorizes local commits only. Pushing, publishing, opening PRs, or filing issues still requires separate explicit authorization.

**Exit for the current fix:** its commit hash is recorded before the next fix starts, or the sequence pauses with an explicit review, verification, scope, or commit blocker. Repeat sections 5-7 until every selected fix is committed or the sequence is blocked.

## 8. Deliver the results

Final output:

1. Investigation scope and actual coverage, including uninspected areas and whether ten supported proposals were found.
2. Decision ledger: fixed, deferred, denied, pending, and blocked IDs; confirm recorded denials in `IMPROVEMENTS.MD` or report the persistence blocker.
3. Per implemented ID: behavior changed, affected paths, verification actually exercised, and remaining risk.
4. Exact prerequisites for unresolved selected items, without treating deferred proposals as unfinished authorized work.
5. Review/commit decision, per-fix commit hashes or explicit uncommitted status, and skills used.

## Common mistakes and stop conditions

| Temptation | Correct action |
|---|---|
| “The user asked for ten, so add plausible cleanup.” | Broaden investigation, then report an honest shortfall. |
| “Ten questions means ten options in one question.” | Submit ten separate questions in one call, with four choices each. |
| “A rejected idea has a new title, so propose it again.” | Match the underlying problem and fix against `IMPROVEMENTS.MD`; keep it excluded unless the user reopens it. |
| “The neighboring bug is obvious and this file is open.” | Keep it deferred; selection applies to IDs and remedies, not files. |
| “Details means they are interested enough to proceed.” | Explain, then obtain an explicit fix decision. |
| “A prose question is close enough to the question tool.” | Use valid tool payloads; if unavailable, stop before implementation. |
| “A small migration is included in any bug-fix approval.” | Compare with the approved card; ask before undisclosed migration scope. |
| “The final test could not run, but the code looks right.” | Report implemented-but-unverified and the missing prerequisite. |
| “The workflow question was unanswered, so proceed without review.” | Save explicit denials, but leave the mode pending and do not start source edits. |
| “Implement all means edit every fix now and commit them later.” | Implement, verify, review when requested, and commit one fix before starting the next. |
| “Several fixes touch this file, so one combined commit is simpler.” | Keep one fix and its tests per commit, completing it before editing the next fix. |

Stop edits for an item when consent is missing, the finding is disproven, new scope exceeds its approved card, or the needed operation is denied. Stop at proposal-only completion when nothing is selected. Otherwise follow the batch's workflow choice one fix at a time through implementation, verification, review when requested, and its own commit. Pause the sequence at an unresolved approval gate or blocker, and deliver an honest status.
