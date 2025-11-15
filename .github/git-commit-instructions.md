**Commit Format**
`<type>(WebAPI): <Imperative Message>`

**Allowed Types**
`feat` – new user-visible functionality
`fix` – bug fix
`refactor` – structural change without behavior change
`perf` – performance improvement
`test` – test-only changes
`docs` – documentation updates
`build` – CI/CD and build system changes
`chore` – maintenance, cleanup, dependency bumps
`style` – formatting-only, no logic changes

**Rules**
Always use the `WebAPI` scope.
Message must be imperative, present tense, minimal.
Capitalize after the colon.
No trailing punctuation.
Body optional; include only *why*, never *what*.
Footer only for breaking changes or issue references.

**Examples**
`feat(WebAPI): Add authentication endpoint`
`fix(WebAPI): Resolve null reference in download workflow`
`refactor(WebAPI): Simplify cache handling`
`chore(WebAPI): Update dependencies`
`docs(WebAPI): Add API usage guide`

**Breaking Change Example**

```
feat(WebAPI): Rework routing structure

BREAKING CHANGE: Legacy v1 endpoints removed
```

**Your Commit**
`refactor(WebAPI): First draft of Radarr setup integration and all its endpoints`
