using System.Threading.Tasks;
using AutoMapper;
using FluentResults;
using MediatR;
using Quartz;
using Quartz.Impl;

namespace PlexRipper.Application
{
    public class SchedulerService : ISchedulerService
    {
        private readonly IMapper _mapper;

        private readonly IMediator _mediator;

        private readonly IScheduler _scheduler;

        public SchedulerService(IMapper mapper, IMediator mediator, IScheduler scheduler)
        {
            _mapper = mapper;
            _mediator = mediator;
            _scheduler = scheduler;
        }

        public async Task<Result> SyncService()
        {

            IJobDetail job = JobBuilder.Create<SyncServerJob>()
                .WithIdentity("SyncServer", "SyncGroup")
                .Build();

            // Trigger the job to run now, and then every 40 seconds
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("StartNow", "TriggerGroup")
                .StartNow()
                .Build();

            await _scheduler.ScheduleJob(job, trigger);

            // Max 3 servers at once

            // Each job executes a sync with a plex server

            // Each job will first retrieve movie libraries and then tvShow

            // Progress is stored in the database
            return Result.Ok();
        }

        public async Task<Result> SetupAsync()
        {
            await _scheduler.Start();

            await SyncService();

            return _scheduler.IsStarted ? Result.Ok() : Result.Fail("Could not start Sync Server Scheduler");
        }
    }
}