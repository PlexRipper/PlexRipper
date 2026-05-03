using System.Collections;
using Autofac;
using FastEndpoints;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Reaparr.Domain;
using Reaparr.Logging;
using Serilog;

namespace Reaparr.Build;

internal sealed class BuildCommandExecutor : ICommandExecutor
{
    private readonly ILifetimeScope _scope;
    private readonly ILogger _log;

    public BuildCommandExecutor(ILifetimeScope scope, ILogger log)
    {
        _scope = scope;
        _log = log.ForContext<BuildCommandExecutor>();
    }

    public async Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken ct = default)
        where TResult : ResultBase, new()
    {
        try
        {
            if (command is null)
                return Fail<TResult>("Command cannot be null.");

            var commandType = command.GetType();
            var validationFailures = await ValidateCommandAsync(command, commandType, ct);
            if (validationFailures.Count > 0)
            {
                var message = string.Join("; ", validationFailures.Select(x => x.ErrorMessage));
                return Fail<TResult>(message);
            }

            var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResult));
            var handler = _scope.ResolveOptional(handlerType);
            if (handler is null)
                return Fail<TResult>($"No command handler registered for '{commandType.Name}'.");

            var executeMethod = handlerType.GetMethod(nameof(ICommandHandler<ICommand<TResult>, TResult>.ExecuteAsync));
            if (executeMethod is null)
                return Fail<TResult>($"Command handler for '{commandType.Name}' does not expose ExecuteAsync.");

            var task = (Task<TResult>)executeMethod.Invoke(handler, [command, ct])!;
            return await task;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Fail<TResult>("Command execution was cancelled.");
        }
        catch (Exception ex)
        {
            _log.Here()
                .Error(ex, "Failed to execute build command {CommandType}", command?.GetType().Name ?? "Unknown");
            return Fail<TResult>(ex.Message);
        }
    }

    private async Task<List<ValidationFailure>> ValidateCommandAsync<TResult>(
        ICommand<TResult> command,
        Type commandType,
        CancellationToken ct
    )
        where TResult : ResultBase, new()
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(commandType);
        var validatorCollectionType = typeof(IEnumerable<>).MakeGenericType(validatorType);
        var validators = _scope.ResolveOptional(validatorCollectionType) as IEnumerable;

        if (validators is null)
            return [];

        var failures = new List<ValidationFailure>();

        foreach (var validator in validators)
        {
            var validateMethod = validatorType.GetMethod(
                nameof(IValidator<object>.ValidateAsync),
                [typeof(IValidationContext), typeof(CancellationToken)]
            );
            if (validateMethod is null)
                continue;

            var context = new FluentValidation.ValidationContext<object>(command);
            var validationTask = (Task<ValidationResult>)validateMethod.Invoke(validator, [context, ct])!;
            var validationResult = await validationTask;
            if (!validationResult.IsValid)
                failures.AddRange(validationResult.Errors);
        }

        return failures;
    }

    private static TResult Fail<TResult>(string message)
        where TResult : ResultBase, new()
    {
        var result = new TResult();
        result.Reasons.Add(new Error(message));
        return result;
    }
}
