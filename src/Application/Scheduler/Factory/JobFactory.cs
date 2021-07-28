using Autofac;
using Quartz;
using Quartz.Spi;

namespace PlexRipper.Application.Factory
{
    public class AutofacJobFactory : IJobFactory
    {
        private readonly IComponentContext _containerContext;

        public AutofacJobFactory(IComponentContext containerContext)
        {
            _containerContext = containerContext;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return (IJob)_containerContext.Resolve(bundle.JobDetail.JobType);
        }

        public void ReturnJob(IJob job) { }
    }
}