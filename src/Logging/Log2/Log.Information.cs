using System.Runtime.CompilerServices;
using Logging.Interface;
using Serilog.Core;
using Serilog.Events;

namespace Logging.Log2;

public partial class Log : ILog
{
    #region Information

    /// <inheritdoc/>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void InformationLine(
        string messageTemplate,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Information, messageTemplate, memberName, sourceFilePath, sourceLineNumber);
    }

    /// <inheritdoc/>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Information(
        Exception ex,
        string messageTemplate,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Information, messageTemplate, ex, memberName, sourceFilePath, sourceLineNumber);
    }

    /// <inheritdoc/>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Information<T>(
        string messageTemplate,
        T propertyValue = default!,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Information, messageTemplate, memberName, sourceFilePath, sourceLineNumber, propertyValue);
    }

    /// <inheritdoc/>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Information<T0, T1>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Information, messageTemplate, memberName, sourceFilePath, sourceLineNumber, propertyValue0, propertyValue1);
    }

    /// <inheritdoc/>
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Information<T0, T1, T2>(
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2,
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
    {
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        Write(LogEventLevel.Information, messageTemplate, memberName, sourceFilePath, sourceLineNumber, propertyValue0, propertyValue1, propertyValue2);
    }

    #endregion
}