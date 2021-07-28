using System;
using System.Threading.Tasks;
using AutoMapper;
using PlexRipper.Domain;
using Quartz;

namespace PlexRipper.Application
{
    public class SyncServerJob : IJob
    {
        private readonly IMapper _mapper;

        public SyncServerJob(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (_mapper is not null)
            {
                Log.Debug("Mapper is set!");
            }

            await Console.Out.WriteLineAsync("HelloJob is executing.");
        }
    }
}