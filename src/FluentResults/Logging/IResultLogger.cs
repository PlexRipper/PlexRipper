using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Reaparr.FluentResults
{
    /// <summary>
    /// Logging interface.  Implement this if you want to have custom logging of results
    /// </summary>
    public interface IResultLogger
    {
        /// <summary>
        /// Log result information
        /// </summary>
        /// <param name="context">Additional log context</param>
        /// <param name="content">Content to log</param>
        /// <param name="result">The result to log</param>
        /// <param name="logLevel">The <see cref="Microsoft.Extensions.Logging.LogLevel"/></param>
        /// <param name="memberName">The caller member name.</param>
        /// <param name="sourceFilePath">The caller source file path.</param>
        /// <param name="sourceLineNumber">The caller source line number.</param>
        void Log(
            string context,
            string? content,
            ResultBase result,
            LogLevel logLevel,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
        );

        /// <summary>
        /// Log result information
        /// </summary>
        /// <typeparam name="TContext">Additional log context</typeparam>
        /// <param name="content">Content to log</param>
        /// <param name="result">The result to log</param>
        /// <param name="logLevel">The <see cref="Microsoft.Extensions.Logging.LogLevel"/></param>
        /// <param name="memberName">The caller member name.</param>
        /// <param name="sourceFilePath">The caller source file path.</param>
        /// <param name="sourceLineNumber">The caller source line number.</param>
        void Log<TContext>(
            string? content,
            ResultBase result,
            LogLevel logLevel,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
        );
    }
}
