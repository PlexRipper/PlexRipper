# xUnit to TUnit Migration Plan

## Overview

Migrate all unit tests from xUnit v3 to TUnit while keeping Shouldly as the assertion library.

**Scope:** `tests/UnitTests/` (13 projects, ~780 tests)
**Out of Scope:** `tests/IntegrationTests/` (deferred)

---

## Phase 0: BaseTests Infrastructure

**Goal:** Update shared test infrastructure to support TUnit

### 0.1 Update Package References

**File:** `tests/BaseTests/BaseTests.csproj`

Remove:
- `xunit.v3` (3.2.1)
- `xunit.runner.visualstudio` (3.1.5)

Add:
- `TUnit` (latest stable)

### 0.2 Update Global Usings

**File:** `tests/BaseTests/GlobalTestsUsings.cs`

```diff
- global using Xunit;
+ global using TUnit.Core;
```

### 0.3 Remove xUnit Runner Config

**File:** `tests/BaseTests/xunit.runner.json` - DELETE

### 0.4 Update CancellationToken Access

**File:** `tests/BaseTests/_Shared/BaseUnitTest/BaseUnitTest.Common.cs`

```diff
- protected CancellationToken CancellationToken => TestContext.Current.CancellationToken;
+ protected CancellationToken CancellationToken =>
+     TUnit.Core.TestContext.Current?.CancellationToken ?? CancellationToken.None;
```

**File:** `tests/BaseTests/_Shared/BaseUnitTest/BaseCommandUnitTest.cs`
- Keep usage through `BaseUnitTest.CancellationToken`

### 0.5 Keep Serilog XUnit Sink (No Changes)

**Directory:** `tests/BaseTests/_Shared/Serilog.Sinks.XUnit/`
- Keep as-is; migrate ITestOutputHelper in follow-up task

**Validation:** Build BaseTests project successfully

---

## Phases 1-13: Unit Test Projects

For each project, apply these transformations:

### Package Reference Updates

**Each `*.UnitTests.csproj`:**

Remove:
- `xunit.v3`
- `xunit.runner.visualstudio`

Add:
- `TUnit`

### Attribute Migrations

| xUnit | TUnit | Notes |
|-------|-------|-------|
| `[Fact]` | `[Test]` | Direct replacement |
| `[Theory]` | `[Test]` | Remove theory marker |
| `[InlineData(...)]` | `[Arguments(...)]` | Direct replacement |
| `[MemberData(nameof(X))]` | `[MethodDataSource(nameof(X))]` | Prefer tuple return data |

### Project Migration Order

| Phase | Project | Files | Tests | Notes |
|-------|---------|-------|-------|-------|
| 1 | `BaseTests.UnitTests` | 4 | ~12 | Smallest, validates base class works |
| 2 | `FluentResultExtension.UnitTests` | 6 | ~30 | Small, isolated |
| 3 | `Logging.UnitTests` | 3 | ~15 | Small |
| 4 | `Settings.UnitTests` | 1 | ~5 | Smallest |
| 5 | `Domain.UnitTests` | 5 | ~25 | Domain logic |
| 6 | `Data.UnitTests` | 7 | ~35 | Database tests |
| 7 | `FileSystem.UnitTests` | 5 | ~25 | File ops |
| 8 | `External.UnitTests` | 4 | ~20 | External services |
| 9 | `BackgroundJobs.UnitTests` | 3 | ~15 | Quartz jobs |
| 10 | `PublicApi.UnitTests` | 3 | ~15 | API endpoints |
| 11 | `AppHost.UnitTests` | 10 | ~60 | App configuration |
| 12 | `PlexApi.UnitTests` | 23 | ~140 | Has MemberData (special handling) |
| 13 | `Application.UnitTests` | 54 | ~324 | Largest, do last |

---

## Phase 12 Special: PlexApi.UnitTests MemberData

**File:** `tests/UnitTests/PlexApi.UnitTests/PlexMedia/GetDashTranscodeDecision/GetDashTranscodeDecisionCommandHandler.UnitTests.cs`

Convert `TheoryData<Action<MakeDecisionMediaContainerConfig>, VideoQuality>` to tuple-returning method:

```csharp
// Before (xUnit)
public static TheoryData<Action<MakeDecisionMediaContainerConfig>, VideoQuality> Data => ...;

[Theory]
[MemberData(nameof(Data))]
public async Task Test(Action<MakeDecisionMediaContainerConfig> config, VideoQuality quality) { }

// After (TUnit)
public static IEnumerable<(Action<MakeDecisionMediaContainerConfig>, VideoQuality)> Data() { ... }

[Test]
[MethodDataSource(nameof(Data))]
public async Task Test(Action<MakeDecisionMediaContainerConfig> config, VideoQuality quality) { }
```

---

## Validation Protocol

After each phase:

1. Build: `dotnet build tests/UnitTests/<Project>/`
2. Run tests: `dotnet test tests/UnitTests/<Project>/ --no-build`
3. Verify test count parity against pre-migration count
4. Commit per phase: `test(WebAPI): Migrate <Project> from xUnit to TUnit`

---

## Rollback Strategy

If issues arise:
1. Revert the problematic phase commit
2. Investigate root cause
3. Re-apply with targeted fix

---

## Success Criteria

- [ ] All 13 unit test projects migrated
- [ ] All unit tests pass
- [ ] No xUnit runner/package references remain in UnitTests projects
- [ ] Build succeeds with no migration regressions
- [ ] Shouldly assertions preserved
- [ ] ITestOutputHelper migration deferred to follow-up
