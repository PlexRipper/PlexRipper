using PlexRipper.Application.PlexAccounts;

namespace PlexRipper.Application.MigrationService
{
    public class MigrationService : IMigrationService
    {
        private readonly IMediator _mediator;

        private readonly IPlexAccountService _plexAccountService;

        public MigrationService(IMediator mediator, IPlexAccountService plexAccountService)
        {
            _mediator = mediator;
            _plexAccountService = plexAccountService;
        }

        public async Task<Result> SetupAsync()
        {
            return await AddClientIdsToPlexAccounts();
        }

        private async Task<Result> AddClientIdsToPlexAccounts()
        {
            var plexAccounts = await _mediator.Send(new GetAllPlexAccountsQuery());

            if (plexAccounts.IsFailed)
            {
                return plexAccounts.LogError();
            }

            var invalidAccounts = plexAccounts.Value.FindAll(x => string.IsNullOrEmpty(x.ClientId));
            if (!invalidAccounts.Any())
            {
                return Result.Ok();
            }

            foreach (var plexAccount in invalidAccounts)
            {
                plexAccount.ClientId = _plexAccountService.GeneratePlexAccountClientId();
                var result = await _mediator.Send(new UpdatePlexAccountCommand(plexAccount));
                if (result.IsFailed)
                {
                    result.LogError();
                }
            }

            return Result.Ok();
        }
    }
}