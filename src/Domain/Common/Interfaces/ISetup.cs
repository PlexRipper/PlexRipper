using FluentResults;

namespace PlexRipper.Domain
{
    public interface ISetup
    {
        public Result<bool> Setup();
    }
}