using Quartz;

namespace Reaparr.BackgroundJobs.Contracts;

public interface ILibrarySyncJobListener : IJobListener, ISetup;
