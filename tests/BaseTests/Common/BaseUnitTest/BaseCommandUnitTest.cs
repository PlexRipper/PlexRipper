using FastEndpoints;
using FluentValidation;
using Serilog.Events;

namespace PlexRipper.BaseTests;

public class BaseCommandUnitTest<TCommand, TCommandHandler> : BaseUnitTest
    where TCommandHandler : class, ICommandHandler<TCommand, Result>
    where TCommand : class, ICommand<Result>
{
    protected BaseCommandUnitTest(ITestOutputHelper output, LogEventLevel logEventLevel = LogEventLevel.Verbose)
        : base(output, logEventLevel) { }

    private IValidator<TCommand> GetValidator()
    {
        var commandType = typeof(TCommandHandler);
        var validatorTypeName = commandType.FullName!.Replace("Handler", "Validator");

        var validatorType = commandType.Assembly.GetTypes().FirstOrDefault(t => t.FullName == validatorTypeName);

        if (validatorType is null)
            throw new InvalidOperationException(
                $"Validator type '{validatorTypeName}' not found for handler:  {commandType.FullName}."
            );

        return (IValidator<TCommand>)Activator.CreateInstance(validatorType)!;
    }

    /// <summary>
    /// Use this method to test the execution of a command handler, including the corresponding validator.
    /// </summary>
    /// <param name="command"> The ICommand to execute inside the handler.</param>
    protected async Task<Result> TestHandlerExecuteAsync(TCommand command)
    {
        var validator = GetValidator();

        var validationResult = await validator.ValidateAsync(command, CancellationToken.None);
        if (!validationResult.IsValid)
            return validationResult.ToResult();

        var handler = mock.Create<TCommandHandler>();
        return await handler.ExecuteAsync(command, CancellationToken.None);
    }

    public override void Dispose()
    {
        base.Dispose();
        mock.Dispose();
    }
}
