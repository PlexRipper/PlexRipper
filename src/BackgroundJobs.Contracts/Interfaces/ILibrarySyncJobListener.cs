using Quartz;
using Reaparr.Domain;

namespace Reaparr.BackgroundJobs.Contracts;

public interface ILibrarySyncJobListener : IJobListener, ISetup;
