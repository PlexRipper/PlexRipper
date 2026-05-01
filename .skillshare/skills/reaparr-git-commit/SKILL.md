---
name: reaparr-git-commit
description: Use when preparing or creating git commits in Reaparr, including commit message formatting, scope selection, and branch targeting rules.
---

# Reaparr Git Commit

## Purpose

This skill defines Reaparr commit and branching conventions. Use it whenever creating commit messages, reviewing commit messages, or validating branch targeting before opening a PR.

## Commit Message Format

Use this exact pattern:

```text
<type>(<scope>): <Imperative message>
```

## Commit Message Rules

| Field | Rules |
| --- | --- |
| Type | `feat`, `fix`, `refactor`, `perf`, `test`, `docs`, `build`, `chore`, `style` |
| Scope | `WebAPI` for backend, `Web-UI` for frontend |
| Message | Imperative present tense, capitalize first word, no trailing punctuation |

### Scope Selection Rule

- All frontend-related changes use scope `Web-UI`.
- All other changes use scope `WebAPI`.

## AI Attribution Rule

Never add AI attribution trailers.

Do not include trailers such as:

```text
Co-Authored-By: Claude ...
```

## Branching Rules

- `dev` is the integration branch and PR target.
- Feature branches merge into `dev`.

## Quick Examples

Good:

```text
feat(WebAPI): Add Library sync endpoint
fix(Web-UI): Correct download status badge
test(WebAPI): Add handler validation tests
```

Bad:

```text
feat(webapi): added endpoint.
fix: download status bug
refactor(Web-UI): Refactor component
```

Why bad:

- Wrong scope casing (`webapi` instead of `WebAPI`)
- Message not imperative or not capitalized correctly
- Trailing punctuation
- Missing scope

## Common Mistakes

- Using a scope other than `WebAPI` or `Web-UI`
- Writing past-tense or lowercase commit messages
- Adding trailing punctuation
- Omitting the scope
- Adding AI attribution trailers
- Targeting a PR branch other than `dev`
