using MediatR;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.PlexApi;
using PlexRipper.Application.PlexServers.Queries;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.PlexAccounts
{
    public class PlexAccountService : IPlexAccountService
    {
        private readonly IMediator _mediator;
        private readonly IPlexServerService _plexServerService;
        private readonly IPlexApiService _plexApiService;

        public PlexAccountService(IMediator mediator, IPlexServerService plexServerService, IPlexApiService plexApiService)
        {
            _mediator = mediator;
            _plexServerService = plexServerService;
            _plexApiService = plexApiService;
        }

        public async Task<ValidationResponse<List<PlexServer>>> GetPlexServersAsync(PlexAccount plexAccount, bool refresh = false)
        {
            if (refresh)
            {
                await _plexServerService.RefreshPlexServersAsync(plexAccount);
            }

            var result = await _mediator.Send(new GetPlexServersByPlexAccountIdQuery(plexAccount.Id));
            if (!result.IsValidResponse)
            {
                Log.Warning("Validation failed when attempting to retrieve the PlexServers by PlexAccount id");
                return result;
            }
            Log.Information($"Retrieved {result.Data.Count} PlexServers from the database"
                            + (refresh ? ", after refresh from the plexApi" : ""));
            return result;
        }

        /// <summary>
        /// Check if this account is valid by querying the Plex API
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>Are the account credentials valid</returns>
        public async Task<ValidationResponse<BooleanValue>> ValidatePlexAccountAsync(string username, string password)
        {
            if (username == string.Empty || password == string.Empty)
            {
                string msg = "Either the username or password were empty";
                Log.Warning(msg);
                return new ValidationResponse<BooleanValue>(new BooleanValue(false), msg);
            }

            var plexAccountFromApi = await _plexApiService.PlexSignInAsync(username, username);
            if (plexAccountFromApi == null)
            {
                string msg = $"The plexAccount with {username} was invalid when send to the PlexApi";

                Log.Warning(msg);
                return new ValidationResponse<BooleanValue>(new BooleanValue(false), msg);
            }

            Log.Debug($"The PlexAccount with username {username} is valid");
            return new ValidationResponse<BooleanValue>(new BooleanValue(true));
        }
        /// /// <summary>
        /// Check if this account is valid by querying the Plex API
        /// </summary>
        /// <param name="plexAccount">The account to check</param>
        /// <returns>Are the account credentials valid</returns>
        public Task<ValidationResponse<BooleanValue>> ValidatePlexAccountAsync(PlexAccount plexAccount)
        {
            return ValidatePlexAccountAsync(plexAccount.Username, plexAccount.Password);
        }

        /// <summary>
        /// This retrieves all the <see cref="PlexAccount"/> related data from the PlexApi. It's assumed that the <see cref="PlexAccount"/> has already been created in the Database.
        /// </summary>
        /// <param name="plexAccount">The <see cref="PlexAccount"/> to setup</param>
        /// <returns>If the setup was successful</returns>
        private async Task<ValidationResponse<BooleanValue>> SetupAccountAsync(PlexAccount plexAccount)
        {
            if (plexAccount == null)
            {
                string msg = "plexAccount was null";
                Log.Warning(msg);
                return new ValidationResponse<BooleanValue>(new BooleanValue(false), msg);
            }
            if (plexAccount.Id <= 0)
            {
                string msg = $"plexAccount.id was {plexAccount.Id}";
                Log.Warning(msg);
                return new ValidationResponse<BooleanValue>(new BooleanValue(false), msg);
            }

            Log.Debug("Setting up new PlexAccount");

            // Request new PlexAccount
            var plexAccountFromApi = await _plexApiService.PlexSignInAsync(plexAccount.Username, plexAccount.Password);
            if (plexAccountFromApi == null)
            {
                string msg = $"The plexAccount with {plexAccount.Username} was invalid when send to the PlexApi";

                Log.Warning(msg);
                return new ValidationResponse<BooleanValue>(new BooleanValue(false), msg);
            }

            // Merge the plexAccount from the Api into the current one.
            plexAccount.FromPlexApi(plexAccountFromApi);
            var result = await _mediator.Send(new UpdatePlexAccountCommand(plexAccount));

            if (result.IsValidResponse)
            {
                string msg = $"Failed to validate the plexAccount before the update process in the database";

                Log.Warning(msg);
                return new ValidationResponse<BooleanValue>(new BooleanValue(false), msg);
            }

            // Retrieve and store servers
            await _plexServerService.RefreshPlexServersAsync(plexAccount);

            Log.Debug("Account was setup successfully!");
            return new ValidationResponse<BooleanValue>(new BooleanValue(true));
        }

        /// <summary>
        /// Checks if an <see cref="PlexAccount"/> with the same username already exists
        /// </summary>
        /// <param name="username">The username to check for</param>
        /// <returns>true if username is available</returns>
        public async Task<ValidationResponse<PlexAccount>> CheckIfUsernameIsAvailableAsync(string username)
        {
            var result = await _mediator.Send(new GetPlexAccountByUsernameQuery(username));

            if (!result.IsValidResponse)
            {
                return result;
            }

            if (result.Data != null)
            {
                Log.Warning($"An Account with the username: {username} already exists.");
                return result;
            }

            Log.Debug($"The username: {username} is available.");
            return result;
        }

        #region CRUD

        /// <summary>
        /// Returns the <see cref="PlexAccount"/> with the corresponding <see cref="PlexAccount"/> and the accessible <see cref="PlexServer"/>s
        /// </summary>
        /// <param name="accountId">The Id to retrieve the <see cref="PlexAccount"/> by</param>
        /// <returns>The account found</returns>
        public async Task<ValidationResponse<PlexAccount>> GetPlexAccountAsync(int accountId)
        {
            var result = await _mediator.Send(new GetPlexAccountByIdQuery(accountId));

            if (!result.IsValidResponse)
            {
                return result;
            }

            if (result.Data != null)
            {
                Log.Debug($"Found an Account with the id: {accountId}");
                return result;
            }

            Log.Warning($"Could not find an Account with id: {accountId}");
            return result;
        }

        public Task<ValidationResponse<List<PlexAccount>>> GetAllPlexAccountsAsync(bool onlyEnabled = false)
        {
            Log.Debug(onlyEnabled ? "Returning only enabled account" : "Returning all accounts");
            return _mediator.Send(new GetAllPlexAccountsQuery(onlyEnabled));
        }

        /// <summary>
        /// Creates an <see cref="PlexAccount"/> in the Database and performs an SetupAccountAsync().
        /// </summary>
        /// <param name="plexAccount">The unique account</param>
        /// <returns>Returns the added account after setup</returns>
        public async Task<ValidationResponse<PlexAccount>> CreatePlexAccountAsync(PlexAccount plexAccount)
        {
            var result = await CheckIfUsernameIsAvailableAsync(plexAccount.Username);
            if (!result.IsValidResponse)
            {
                return result;
            }

            if (result.Data != null)
            {
                Log.Warning($"Account with username {result.Data.Username} cannot be created due to an account with the same username already existing with Account id: {result.Data.Id}");
                return result;
            }

            result = await _mediator.Send(new CreatePlexAccountCommand(plexAccount));

            if (!result.IsValidResponse)
            {
                Log.Warning("Failed to validate the PlexAccount that will be created");
                return result;
            }

            if (result.Data != null)
            {
                var isSuccessful = await SetupAccountAsync(result.Data);
                if (isSuccessful.IsValidResponse)
                {
                    return await _mediator.Send(new GetPlexAccountByIdQuery(plexAccount.Id));
                }
                return isSuccessful.ConvertTo<PlexAccount>();
            }

            Log.Warning("An account with these credentials already exists!");
            return result;
        }

        public async Task<ValidationResponse<PlexAccount>> UpdateAccountAsync(PlexAccount newAccount)
        {

            var result = await _mediator.Send(new UpdatePlexAccountCommand(newAccount));
            if (!result.IsValidResponse)
            {
                Log.Warning("Failed to validate the PlexAccount that will be updated");
                return result;
            }


            // Re-validate if the password changed
            // TODO this assumes the updated account is returned
            if (result.Data.Password != newAccount.Password)
            {
                await SetupAccountAsync(result.Data);
                return await GetPlexAccountAsync(result.Data.Id);
            }


            return result;
        }

        /// <summary>
        /// Hard deletes the PlexAccount from the Database
        /// </summary>
        /// <param name="plexAccountId"></param>
        /// <returns></returns>
        public Task<ValidationResponse<BooleanValue>> DeletePlexAccountAsync(int plexAccountId)
        {
            return _mediator.Send(new DeletePlexAccountCommand(plexAccountId));
        }
        #endregion
    }
}
