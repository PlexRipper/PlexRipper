namespace Reaparr.Logging.Interface;

// Make T optional to allow for non-generic implementations
// ReSharper disable once UnusedTypeParameter
public interface ILog<T> : Logging.ILog
    where T : class { }
