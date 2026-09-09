namespace Reaparr.FluentResults
{
    /// <summary>
    /// Definition of an error
    /// </summary>
    public interface IError : IReason
    {
        /// <summary>
        /// Reasons of the error
        /// </summary>
        List<IError> Reasons { get; }
    }
}
