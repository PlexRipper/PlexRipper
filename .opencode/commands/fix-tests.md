---
description: fix frontend backend integration tests
---

<summary>
You MUST run frontend unit tests, backend unit tests, and integration tests for this PR.
You MUST fix root causes of failing tests with small focused diffs.
You MUST re-run affected tests until all pass and then summarize what changed.
</summary>

<user_guidelines>
$ARGUMENTS
</user_guidelines>

<context>
@AGENTS.md
</context>

<objective>
You MUST load and follow @AGENTS.md and all relevant skills before acting.
You MUST stabilize the pull request by fixing failing frontend unit tests, backend unit tests, and integration tests.
You MUST prefer MCP tools and project scripts, keep behavior deterministic, and avoid unrelated refactors.
</objective>

1. Discover and run the smallest commands needed to execute:
    - Frontend unit tests
    - Backend unit tests
    - Integration tests
2. Collect failures and group by root cause.
3. Apply minimal fixes that preserve intended behavior.
4. Re-run only impacted test suites first, then run all three suites for final verification.
5. Return:
    - failing tests found
    - root causes
    - files changed
    - final passing evidence
    - any remaining risks
