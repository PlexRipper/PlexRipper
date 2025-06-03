using FluentValidation;
using Serilog.Events;

namespace PlexRipper.BaseTests;

public abstract class BaseCommandUnitTest<TCommand> : BaseUnitTest
    where TCommand : class
{
    protected BaseCommandUnitTest(ITestOutputHelper output, LogEventLevel logEventLevel = LogEventLevel.Verbose)
        : base(output, logEventLevel) { }

    private IValidator<TCommand> GetValidator()
    {
        var commandType = typeof(TCommand);
        var validatorTypeName = commandType.FullName!.Replace("Command", "Validator");
        var validatorType =
            commandType.Assembly.GetTypes().FirstOrDefault(t => t.FullName == validatorTypeName)
            ?? throw new InvalidOperationException(
                $"Validator type '{validatorTypeName}' not found for command: {commandType.FullName}."
            );

        return (IValidator<TCommand>)Activator.CreateInstance(validatorType)!;
    }

    /// <summary>
    /// Use this method to test the execution of a command handler, including the corresponding validator.
    /// </summary>
    /// <param name="command"> The ICommand to execute inside the handler.</param>
    protected async Task<Result<TResponse>> TestHandlerExecuteAsync<TResponse>(TCommand command)
    {
        var validator = GetValidator();
        var validationResult = await validator.ValidateAsync(command, CancellationToken.None);
        if (!validationResult.IsValid)
            return validationResult.ToResult();

        // Infer the handler type by name
        var commandType = typeof(TCommand);
        var handlerTypeName = commandType.FullName!.Replace("Command", "CommandHandler");
        var handlerType =
            typeof(TCommand).Assembly.GetType(handlerTypeName)
            ?? throw new InvalidOperationException(
                $"Handler type '{handlerTypeName}' not found for command: {commandType.FullName}."
            );

        var handler = mock.Create(handlerType);

        // dynamically cast the handler to ICommandHandler<TCommand, TResponse>
        // to avoid needing to know the exact response type at compile time
        dynamic dynHandler = handler;

        // we still strongly type the command and CancellationToken
        Result<TResponse> result = await dynHandler.ExecuteAsync(command, CancellationToken.None);
        return result;
    }

    public override void Dispose()
    {
        base.Dispose();
        mock.Dispose();
    }
}
