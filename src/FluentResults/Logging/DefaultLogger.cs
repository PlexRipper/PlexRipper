using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Reaparr.FluentResults
{
    /// <summary>
    /// Default implementation of <see cref="IResultLogger"/> that doesn't log anything
    /// </summary>
    public class DefaultLogger : IResultLogger
    {
        /// <inheritdoc/>
        public void Log(
            string context,
            string? content,
            ResultBase result,
            LogLevel logLevel,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
        ) { }

        /// <inheritdoc/>
        public void Log<TContext>(
            string? content,
            ResultBase result,
            LogLevel logLevel,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
        ) { }
    }
}
