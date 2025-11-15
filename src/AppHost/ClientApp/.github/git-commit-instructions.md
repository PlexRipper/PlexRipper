**Commit Format**
`<type>(Web-UI): <imperative-message>`

**Allowed Types**
`feat` – new user-visible functionality
`fix` – bug fix
`refactor` – structural change without behavior change
`perf` – performance improvement
`test` – test-only changes
`docs` – documentation updates
`build` – CI/CD and build system changes
`chore` – maintenance, cleanup, dependency bumps
`style` – formatting only, no logic changes

**Rules**
Always use the `Web-UI` scope.
Message must be imperative, present tense, minimal.
No trailing punctuation.
Capitalize after the colon.
Keep body optional; when used, explain *why*, never *what*.
Use footer only for breaking changes or issue references.

**Examples**
`feat(Web-UI): Add navigation sidebar`
`fix(Web-UI): Resolve null reference in settings panel`
`refactor(Web-UI): Simplify reactive state handling`
`chore(Web-UI): Update dependencies`
`docs(Web-UI): Add setup guide`

**Breaking Change Example**

```
feat(Web-UI): change settings layout

BREAKING CHANGE: settings sidebar moved to top navigation
```
