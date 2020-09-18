using Microsoft.AspNetCore.SignalR;
using PlexRipper.Domain;
using PlexRipper.Domain.Types;
using System.Threading.Tasks;
using PlexRipper.Application.Common;

namespace PlexRipper.SignalR.Hubs
{
    public class ProgressHub : Hub
    {
        public ProgressHub() { }
    }
}