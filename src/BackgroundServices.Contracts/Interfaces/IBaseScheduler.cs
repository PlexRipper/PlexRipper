using PlexRipper.Domain;

namespace BackgroundServices.Contracts;

public interface IBaseScheduler : ISetupAsync, IStopAsync { }