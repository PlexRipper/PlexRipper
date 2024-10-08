namespace Logging.Interface;

// Make T optional to allow for non-generic implementations
// ReSharper disable once UnusedTypeParameter
public interface ILog<T> : ILog
    where T : class { }
