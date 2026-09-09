using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Reaparr.FluentResults
{
    /// <summary>
    /// Definition of a ResultBase
    /// </summary>
    public interface IResultBase
    {
        /// <summary>
        /// Is true if Reasons contains at least one error
        /// </summary>
        bool IsFailed { get; }

        /// <summary>
        /// Is true if Reasons contains no errors
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        /// Get all reasons (errors and successes)
        /// </summary>
        List<IReason> Reasons { get; }

        /// <summary>
        /// Get all errors
        /// </summary>
        IReadOnlyList<IError> Errors { get; }

        /// <summary>
        /// Get all successes
        /// </summary>
        IReadOnlyList<ISuccess> Successes { get; }
    }

    /// <summary>
    /// Default implementation of <see cref="IResultBase"/>
    /// </summary>
    public abstract class ResultBase : IResultBase
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool IsFailed => Reasons.OfType<IError>().Any();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool IsSuccess => !IsFailed;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public List<IReason> Reasons { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IReadOnlyList<IError> Errors => Reasons.OfType<IError>().ToList().AsReadOnly();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IReadOnlyList<ISuccess> Successes => Reasons.OfType<ISuccess>().ToList().AsReadOnly();

        /// <summary>
        /// Default constructor
        /// </summary>
        protected ResultBase()
        {
            Reasons = new List<IReason>();
        }

        /// <summary>
        /// Check if the result object contains an error from a specific type
        /// </summary>
        public bool HasError<TError>()
            where TError : IError
        {
            return HasError<TError>(out _);
        }

        /// <summary>
        /// Check if the result object contains an error from a specific type
        /// </summary>
        public bool HasError<TError>(out IEnumerable<TError> result)
            where TError : IError
        {
            return HasError(static _ => true, out result);
        }

        /// <summary>
        /// Check if the result object contains an error from a specific type and with a specific condition
        /// </summary>
        public bool HasError<TError>(Func<TError, bool> predicate)
            where TError : IError
        {
            return HasError(predicate, out _);
        }

        /// <summary>
        /// Check if the result object contains an error from a specific type and with a specific condition
        /// </summary>
        public bool HasError<TError>(Func<TError, bool> predicate, out IEnumerable<TError> result)
            where TError : IError
        {
            ArgumentNullException.ThrowIfNull(predicate);

            return ResultHelper.HasError(Errors, predicate, out result);
        }

        /// <summary>
        /// Check if the result object contains an error with a specific condition
        /// </summary>
        public bool HasError(Func<IError, bool> predicate)
        {
            return HasError(predicate, out _);
        }

        /// <summary>
        /// Check if the result object contains an error with a specific condition
        /// </summary>
        public bool HasError(Func<IError, bool> predicate, out IEnumerable<IError> result)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            return ResultHelper.HasError(Errors, predicate, out result);
        }

        /// <summary>
        /// Check if the result object contains an exception from a specific type
        /// </summary>
        public bool HasException<TException>()
            where TException : Exception
        {
            return HasException<TException>(out _);
        }

        /// <summary>
        /// Check if the result object contains an exception from a specific type
        /// </summary>
        public bool HasException<TException>(out IEnumerable<IError> result)
            where TException : Exception
        {
            return HasException<TException>(static _ => true, out result);
        }

        /// <summary>
        /// Check if the result object contains an exception from a specific type and with a specific condition
        /// </summary>
        public bool HasException<TException>(Func<TException, bool> predicate)
            where TException : Exception
        {
            return HasException(predicate, out _);
        }

        /// <summary>
        /// Check if the result object contains an exception from a specific type and with a specific condition
        /// </summary>
        public bool HasException<TException>(Func<TException, bool> predicate, out IEnumerable<IError> result)
            where TException : Exception
        {
            ArgumentNullException.ThrowIfNull(predicate);

            return ResultHelper.HasException(Errors, predicate, out result);
        }

        /// <summary>
        /// Check if the result object contains a success from a specific type
        /// </summary>
        public bool HasSuccess<TSuccess>()
            where TSuccess : ISuccess
        {
            return HasSuccess<TSuccess>(static _ => true, out _);
        }

        /// <summary>
        /// Check if the result object contains a success from a specific type
        /// </summary>
        public bool HasSuccess<TSuccess>(out IEnumerable<TSuccess> result)
            where TSuccess : ISuccess
        {
            return HasSuccess(static _ => true, out result);
        }

        /// <summary>
        /// Check if the result object contains a success from a specific type and with a specific condition
        /// </summary>
        public bool HasSuccess<TSuccess>(Func<TSuccess, bool> predicate)
            where TSuccess : ISuccess
        {
            return HasSuccess(predicate, out _);
        }

        /// <summary>
        /// Check if the result object contains a success from a specific type and with a specific condition
        /// </summary>
        public bool HasSuccess<TSuccess>(Func<TSuccess, bool> predicate, out IEnumerable<TSuccess> result)
            where TSuccess : ISuccess
        {
            return ResultHelper.HasSuccess(Successes, predicate, out result);
        }

        /// <summary>
        /// Check if the result object contains a success with a specific condition
        /// </summary>
        public bool HasSuccess(Func<ISuccess, bool> predicate, out IEnumerable<ISuccess> result)
        {
            return ResultHelper.HasSuccess(Successes, predicate, out result);
        }

        /// <summary>
        /// Check if the result object contains a success with a specific condition
        /// </summary>
        public bool HasSuccess(Func<ISuccess, bool> predicate)
        {
            return ResultHelper.HasSuccess(Successes, predicate, out _);
        }
    }

    /// <summary>
    /// Default implementation of <see cref="IResultBase"/> generics
    /// </summary>
    public abstract class ResultBase<TResult> : ResultBase
        where TResult : ResultBase<TResult>
    {
        /// <summary>
        /// Add a reason (success or error)
        /// </summary>
        public TResult WithReason(IReason reason)
        {
            Reasons.Add(reason);
            return (TResult)this;
        }

        /// <summary>
        /// Add multiple reasons (success or error)
        /// </summary>
        public TResult WithReasons(IEnumerable<IReason> reasons)
        {
            Reasons.AddRange(reasons);
            return (TResult)this;
        }

        /// <summary>
        /// Add an error
        /// </summary>
        public TResult WithError(string errorMessage)
        {
            return WithError(Result.Settings.ErrorFactory(errorMessage));
        }

        /// <summary>
        /// Add an error
        /// </summary>
        public TResult WithError(IError error)
        {
            return WithReason(error);
        }

        /// <summary>
        /// Add multiple errors
        /// </summary>
        public TResult WithErrors(IEnumerable<IError> errors)
        {
            return WithReasons(errors);
        }

        /// <summary>
        /// Add multiple errors
        /// </summary>
        public TResult WithErrors(IEnumerable<string> errors)
        {
            return WithReasons(errors.Select(errorMessage => Result.Settings.ErrorFactory(errorMessage)));
        }

        /// <summary>
        /// Add an error
        /// </summary>
        public TResult WithError<TError>()
            where TError : IError, new()
        {
            return WithError(new TError());
        }

        /// <summary>
        /// Add a success
        /// </summary>
        public TResult WithSuccess(string successMessage)
        {
            return WithSuccess(Result.Settings.SuccessFactory(successMessage));
        }

        /// <summary>
        /// Add a success
        /// </summary>
        public TResult WithSuccess(ISuccess success)
        {
            return WithReason(success);
        }

        /// <summary>
        /// Add a success
        /// </summary>
        public TResult WithSuccess<TSuccess>()
            where TSuccess : Success, new()
        {
            return WithSuccess(new TSuccess());
        }

        /// <summary>
        /// Add multiple successes
        /// </summary>
        public TResult WithSuccesses(IEnumerable<ISuccess> successes)
        {
            foreach (var success in successes)
            {
                WithSuccess(success);
            }

            return (TResult)this;
        }

        /// <summary>
        /// Log the result. Configure the logger via Result.Setup(..)
        /// </summary>
        public TResult Log(
            LogLevel logLevel = LogLevel.Information,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
        ) => Log(string.Empty, null, logLevel, memberName, sourceFilePath, sourceLineNumber);

        /// <summary>
        /// Log the result only when it contains an error. Configure the logger via Result.Setup(..)
        /// </summary>
        public TResult LogIfFailed(
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
        ) => IsFailed ? Log(LogLevel.Error, memberName, sourceFilePath, sourceLineNumber) : (TResult)this;

        /// <summary>
        /// Log the result. Configure the logger via Result.Setup(..)
        /// </summary>
        public TResult Log(
            string context,
            LogLevel logLevel = LogLevel.Information,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
        ) => Log(context, null, logLevel, memberName, sourceFilePath, sourceLineNumber);

        /// <summary>
        /// Log the result with a specific logger context. Configure the logger via Result.Setup(..)
        /// </summary>
        public TResult Log(
            string context,
            string? content,
            LogLevel logLevel = LogLevel.Information,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
        )
        {
            var logger = Result.Settings.Logger;
            logger.Log(context, content, this, logLevel, memberName, sourceFilePath, sourceLineNumber);
            return (TResult)this;
        }

        /// <summary>
        /// Log the result with a typed context. Configure the logger via Result.Setup(..)
        /// </summary>
        public TResult Log<TContext>(
            LogLevel logLevel = LogLevel.Information,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
        ) => Log<TContext>(null, logLevel, memberName, sourceFilePath, sourceLineNumber);

        /// <summary>
        /// Log the result with a typed context. Configure the logger via Result.Setup(..)
        /// </summary>
        public TResult Log<TContext>(
            string? content,
            LogLevel logLevel = LogLevel.Information,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
        )
        {
            var logger = Result.Settings.Logger;
            logger.Log<TContext>(content, this, logLevel, memberName, sourceFilePath, sourceLineNumber);
            return (TResult)this;
        }

        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var reasonsString = Reasons.Any() ? $", Reasons='{ReasonFormat.ReasonsToString(Reasons)}'" : string.Empty;

            return $"Result: IsSuccess='{IsSuccess}'{reasonsString}";
        }
    }
}
