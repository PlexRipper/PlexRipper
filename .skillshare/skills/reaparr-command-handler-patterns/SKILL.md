---
name: reaparr-command-handler-patterns
description: Use when creating or updating Reaparr backend FastEndpoints command/handler pairs (`ICommand` + `ICommandHandler`), especially when adding validators, Result-based responses, and command dispatch via `ICommandExecutor`.
---

# Reaparr Command/Handler Patterns

## Required First Skill

Load `reaparr-backend` before this skill. It owns shared backend tooling, architecture, build/test commands, and verification gates.

## Overview

Use this skill for Reaparr's command pipeline built on FastEndpoints + FluentResults.

Original FastEndpoints command-bus reference:
- https://fast-endpoints.com/docs/command-bus#_1-define-a-command

The project convention is:
- command record implements `ICommand<Result>` or `ICommand<Result<T>>`
- validator derives from `AbstractValidator<TCommand>`
- handler implements `ICommandHandler<TCommand, Result>` or `ICommandHandler<TCommand, Result<T>>`
- handler returns `Result.Ok(...)` / `Result.Fail(...)` (never `null`)

## Upstream vs Reaparr

FastEndpoints upstream examples often use plain return types on commands (for example `ICommand<UserDto>`).

In Reaparr, prefer wrapping command outcomes in FluentResults:
- `ICommand<Result>` for non-payload commands
- `ICommand<Result<T>>` for payload commands

This keeps validation/error middleware and handler composition consistent with the rest of the codebase.

## When to Use

Use this skill when:
- creating a new command/handler pair in backend projects (`Application`, `BackgroundJobs`, `PlexApi`, etc.)
- adding validation rules for a command
- wiring command-to-command dispatch with `ICommandExecutor`
- updating existing handlers to follow Reaparr Result/logging conventions

Do not use this for endpoint classes (`BaseEndpoint<,>`) or frontend code.

## Required Structure

### 1) Command record

- Use a record implementing `ICommand<Result>` or `ICommand<Result<T>>`.
- Keep command properties minimal and explicit.
- If the command is consumed across projects, place the record in a `*.Contracts` project and handler in the implementation project.

Example references:
- `src/Application/PlexDownloads/Start/StartDownloadTaskCommand.cs`
- `src/Application.Contracts/PlexDownloads/Stop/StopDownloadTaskCommand.cs`
- `src/BackgroundJobs.Contracts/QueueLibrarySyncJobCommand.cs`

### 2) Validator

- Always create an `AbstractValidator<TCommand>`.
- Validate the command object for null where appropriate: `RuleFor(x => x).NotNull();`
- Add guard rules for required/invalid inputs (`NotEmpty`, `GreaterThan(0)`, `Must(...)`, etc.).

Example references:
- `src/Application/PlexDownloads/Execute/DownloadClient/GetDashDownloadUrlCommand.cs`
- `src/BackgroundJobs/LibrarySync/Commands/QueueLibrarySyncJob/QueueLibrarySyncJobCommandHandler.cs`

### 3) Handler

- Implement `ICommandHandler<TCommand, TResult>`.
- Inject Serilog logger as `ILogger log` and assign with explicit constructor assignment:
  - `_log = log.ForContext<YourHandler>();`
- Prefer `var` for locals.
- Return `Result`/`Result<T>` only; never return `null`.
- Use `ICommandExecutor` for internal command dispatch when composing workflows.

Example references:
- `src/Application/PlexServers/RefreshAccess/RefreshPlexServerAccessCommand.cs`
- `src/Application/PlexDownloads/Continue/ContinueDownloadTaskCommand.cs`
- `src/Domain/_Shared/FastEndpoints/CommandExecutor.cs`

## Error Handling and Result Rules

- Prefer safe result composition patterns:
  - `return Result.Ok();`
  - `return Result.Ok(value);`
  - `return Result.Fail("message").LogError();`
  - `return Result.Fail(new ExceptionalError(ex)).LogError();`
- For exception boundaries, use either:
  - `await Result.Try(async () => { ... })`, or
  - explicit `try/catch` with `Result.Fail(new ExceptionalError(ex))`
- Do not swallow unexpected exceptions without converting to a failed `Result`.

## Command Pipeline Notes (Reaparr-specific)

- Commands are executed through `ICommandExecutor.Send(...)`.
- Validation is enforced by command middleware (`ValidationPipeline<TRequest, TResponse>`) for `TResponse : ResultBase`.
- Keep command responses as `Result`-based types for pipeline compatibility.

References:
- `src/Domain/_Shared/FastEndpoints/ICommandExecutor.cs`
- `src/Application/_Shared/Config/FastEndpoints/Pipelines/ValidationPipeline.cs`

## Baseline Template

Use this as the starting point and adjust fields/rules for the specific command.

```csharp
using FastEndpoints;
using FluentValidation;

public record $NAME$Command(string Request) : ICommand<Result<$RETURNTYPE$>>;

public class $NAME$CommandValidator : AbstractValidator<$NAME$Command>
{
    public $NAME$CommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.Request).NotEmpty().MaximumLength(2000);
        // Add command-specific rules here...
    }
}

public class $NAME$CommandHandler : ICommandHandler<$NAME$Command, Result<$RETURNTYPE$>>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;

    public $NAME$CommandHandler(ILogger log, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<$NAME$CommandHandler>();
        _commandExecutor = commandExecutor;
    }

    public async Task<Result<$RETURNTYPE$>> ExecuteAsync(
        $NAME$Command command,
        CancellationToken cancellationToken
    )
    {
        return await Result.Try(async () =>
        {
            var request = command.Request;

            // TODO: implement command logic

            return Result.Ok(default($RETURNTYPE$)!);
        });
    }
}
```

## Common Mistakes to Avoid

- Using `ILogger<T>` directly instead of repo convention (`ILogger` + `ForContext<THandler>()`).
- Omitting validator classes for commands.
- Returning `null` or raw values where `Result`/`Result<T>` is expected.
- Forgetting to propagate `cancellationToken` on async calls.
- Putting shared command records in implementation projects when they belong in `*.Contracts`.
