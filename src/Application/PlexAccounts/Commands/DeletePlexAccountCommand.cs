namespace PlexRipper.Application.PlexAccounts
{
    public class DeletePlexAccountCommand : IRequest<Result>
    {
        public int Id { get; }

        public DeletePlexAccountCommand(int Id)
        {
            this.Id = Id;
        }
    }
}