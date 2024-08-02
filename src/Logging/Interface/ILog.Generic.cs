namespace Logging.Interface;

public interface ILog<T> : ILog
    where T : class { }
