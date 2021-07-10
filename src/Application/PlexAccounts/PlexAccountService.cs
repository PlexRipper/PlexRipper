using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.PlexServers;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexAccounts
{
    public class PlexAccountService : IPlexAccountService
    {
        private readonly IMediator _mediator;

        private readonly IPlexServerService _plexServerService;

        private readonly IPlexLibraryService _plexLibraryService;

        private readonly IPlexApiService _plexApiService;

        private readonly ISignalRService _signalRService;

        public PlexAccountService(IMediator mediator, IPlexServerService plexServerService, IPlexLibraryService plexLibraryService,
            IPlexApiService plexApiService, ISignalRService signalRService)
        {
            _mediator = mediator;
            _plexServerService = plexServerService;
            _plexLibraryService = plexLibraryService;
            _plexApiService = plexApiService;
            _signalRService = signalRService;
        }

        public async Task<Result<List<PlexServer>>> GetPlexServersAsync(PlexAccount plexAccount, bool refresh = false)
        {
            if (refresh)
            {
                await _plexServerService.RefreshPlexServersAsync(plexAccount);
            }

            var result = await _mediator.Send(new GetPlexServersByPlexAccountIdQuery(plexAccount.Id));
            if (!result.IsFailed)
            {
                Log.Warning("Validation failed when attempting to retrieve the PlexServers by PlexAccount id");
                return result;
            }

            Log.Information($"Retrieved {result.Value.Count} PlexServers from the database"
                            + (refresh ? ", after refresh from the plexApi" : string.Empty));
            return result;
        }

        /// <summary>
        /// Check if this account is valid by querying the Plex API.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>If the account credentials valid.</returns>
        public async Task<Result<bool>> ValidatePlexAccountAsync(string username, string password)
        {
            if (username == string.Empty || password == string.Empty)
            {
                string msg = "Either the username or password were empty";
                Log.Warning(msg);
                return Result.Fail(msg);
            }

            var plexAccountFromApi = await _plexApiService.PlexSignInAsync(username, password);
            if (plexAccountFromApi == null)
            {
                string msg = $"The plexAccount with {username} was invalid when sent to the PlexApi";

                Log.Warning(msg);
                return Result.Fail(msg);
            }

            Log.Debug($"The PlexAccount with username {username} is valid");
            return Result.Ok(true);
        }

        /// /// <summary>
        /// Check if this account is valid by querying the Plex API.
        /// </summary>
        /// <param name="plexAccount">The account to check.</param>
        /// <returns>Are the account credentials valid.</returns>
        public Task<Result<bool>> ValidatePlexAccountAsync(PlexAccount plexAccount)
        {
            return ValidatePlexAccountAsync(plexAccount.Username, plexAccount.Password);
        }

        /// <summary>
        /// This retrieves all the <see cref="PlexAccount"/> related data from the PlexApi. It's assumed that the <see cref="PlexAccount"/> has already been created in the Database.
        /// </summary>
        /// <param name="plexAccount">The <see cref="PlexAccount"/> to setup.</param>
        /// <returns>If the setup was successful.</returns>
        private async Task<Result<bool>> SetupAccountAsync(PlexAccount plexAccount)
        {
            if (plexAccount == null)
            {
                string msg = "plexAccount was null";
                Log.Warning(msg);
                return Result.Fail(msg);
            }

            if (plexAccount.Id <= 0)
            {
                string msg = $"plexAccount.id was {plexAccount.Id}";
                Log.Warning(msg);
                return Result.Fail(msg);
            }

            Log.Debug("Setting up new PlexAccount");

            await _signalRService.SendPlexAccountRefreshUpdate(plexAccount.Id, 0, 1);

            // Request new PlexAccount
            var plexAccountFromApi = await _plexApiService.PlexSignInAsync(plexAccount.Username, plexAccount.Password);
            if (plexAccountFromApi == null)
            {
                string msg = $"The plexAccount with {plexAccount.Username} was invalid when sent to the PlexApi";

                Log.Warning(msg);
                return Result.Fail(msg);
            }

            // Merge the plexAccount from the Api into the current one.
            plexAccount.FromPlexApi(plexAccountFromApi);
            var result = await _mediator.Send(new UpdatePlexAccountCommand(plexAccount));

            if (result.IsFailed)
            {
                string msg = "Failed to validate the plexAccount before the update process in the database";

                Log.Warning(msg);
                return result.ToResult().WithError(msg);
            }

            // Retrieve and store servers
            var refreshResult = await _plexServerService.RefreshPlexServersAsync(plexAccount);
            if (refreshResult.IsFailed)
            {
                string msg = "Failed to refresh the PlexServers when setting up the PlexAccount";
                Log.Warning(msg);
                return refreshResult.ToResult().WithError(msg);
            }

            var plexServerList = await _mediator.Send(new GetPlexServersByPlexAccountIdQuery(plexAccount.Id));
            if (plexServerList.IsFailed)
            {
                return plexServerList.ToResult();
            }

            // Retrieve and store the corresponding PlexLibraries
            if (!plexServerList.Value.Any())
            {
                Log.Debug("Account was setup successfully, but did not have access to any servers!");
                return Result.Ok(false);
            }

            Log.Debug("Account was setup successfully!");
            return Result.Ok(true);
        }

        /// <inheritdoc/>
        public async Task<Result<bool>> RefreshPlexAccount(int plexAccountId = 0)
        {
            if (plexAccountId == 0)
            {
                var enabledAccounts = await _mediator.Send(new GetAllPlexAccountsQuery(false, false, true));
                if (enabledAccounts.IsFailed)
                {
                    return enabledAccounts.ToResult();
                }

                foreach (var plexAccount in enabledAccounts.Value)
                {
                    var result = await SetupAccountAsync(plexAccount);
                    if (result.IsFailed)
                    {
                        return result;
                    }
                }
            }
            else
            {
                var plexAccount = await _mediator.Send(new GetPlexAccountByIdQuery(plexAccountId));
                if (plexAccount.IsFailed)
                {
                    return plexAccount.ToResult();
                }

                var result = await SetupAccountAsync(plexAccount.Value);
                if (result.IsFailed)
                {
                    return result.ToResult();
                }
            }

            return Result.Ok(true);
        }

        /// <summary>
        /// Checks if an <see cref="PlexAccount"/> with the same username already exists.
        /// </summary>
        /// <param name="username">The username to check for.</param>
        /// <returns>true if username is available.</returns>
        public async Task<Result<bool>> CheckIfUsernameIsAvailableAsync(string username)
        {
            var result = await _mediator.Send(new GetPlexAccountByUsernameQuery(username));

            if (result.Has404NotFoundError())
            {
                return Result.Ok(true);
            }

            if (result.IsFailed)
            {
                return result.ToResult();
            }

            if (result.Value != null)
            {
                Log.Warning($"An Account with the username: {username} already exists.");
                return Result.Ok(false);
            }

            Log.Debug($"The username: {username} is available.");
            return Result.Ok(true);
        }

        #region CRUD

        /// <summary>
        /// Returns the <see cref="PlexAccount"/> with the accessible <see cref="PlexServer"/>s and all <see cref="PlexLibrary"/>.
        /// </summary>
        /// <param name="accountId">The Id to retrieve the <see cref="PlexAccount"/> by.</param>
        /// <returns>The account found.</returns>
        public async Task<Result<PlexAccount>> GetPlexAccountAsync(int accountId)
        {
            var result = await _mediator.Send(new GetPlexAccountByIdQuery(accountId, true, true));

            if (result.IsFailed)
            {
                return result;
            }

            if (result.Value != null)
            {
                Log.Debug($"Found an Account with the id: {accountId}");
                return result;
            }

            Log.Warning($"Could not find an Account with id: {accountId}");
            return result;
        }

        /// <summary>
        /// Retrieves all <see cref="PlexAccount"/>s with the included <see cref="PlexServer"/>s and <see cref="PlexLibrary"/>s.
        /// </summary>
        /// <param name="onlyEnabled">Should only enabled <see cref="PlexAccount"/>s be retrieved.</param>
        /// <returns>A list of all <see cref="PlexAccount"/>s.</returns>
        public Task<Result<List<PlexAccount>>> GetAllPlexAccountsAsync(bool onlyEnabled = false)
        {
            Log.Debug(onlyEnabled ? "Returning only enabled account" : "Returning all accounts");
            return _mediator.Send(new GetAllPlexAccountsQuery(true, true, onlyEnabled));
        }

        /// <summary>
        /// Creates an <see cref="PlexAccount"/> in the Database and performs an SetupAccountAsync().
        /// </summary>
        /// <param name="plexAccount">The unique account.</param>
        /// <returns>Returns the added account after setup.</returns>
        public async Task<Result<PlexAccount>> CreatePlexAccountAsync(PlexAccount plexAccount)
        {
            Log.Debug($"Creating account with username {plexAccount.Username}");
            var result = await CheckIfUsernameIsAvailableAsync(plexAccount.Username);

            // Fail on validation errors
            if (result.IsFailed)
            {
                return result.ToResult<PlexAccount>();
            }

            if (result.Value == false)
            {
                string msg =
                    $"Account with username {plexAccount.Username} cannot be created due to an account with the same username already existing";
                Log.Warning(msg);
                return result.ToResult().WithError(msg);
            }

            // Check account for validity
            var checkAccount = await ValidatePlexAccountAsync(plexAccount.Username, plexAccount.Password);
            if (checkAccount.IsFailed || !checkAccount.Value)
            {
                return checkAccount.ToResult<PlexAccount>();
            }

            // Create PlexAccount
            var createResult = await _mediator.Send(new CreatePlexAccountCommand(plexAccount));

            if (createResult.IsFailed)
            {
                string msg = "Failed to validate the PlexAccount that will be created";
                createResult.Errors.Add(new Error(msg));
                createResult.LogError();
                return createResult.ToResult();
            }

            // Setup PlexAccount
            var accountInDb = await _mediator.Send(new GetPlexAccountByIdQuery(createResult.Value, true, true));
            if (accountInDb.IsFailed)
            {
                return accountInDb;
            }

            var isSuccessful = await SetupAccountAsync(accountInDb.Value);
            if (isSuccessful.IsFailed)
            {
                return isSuccessful.ToResult();
            }

            if (isSuccessful.Value)
            {
                return await _mediator.Send(new GetPlexAccountByIdQuery(accountInDb.Value.Id, true, true));
            }

            // Failed to setup account successfully, return errors
            return isSuccessful.ToResult<PlexAccount>();
        }

        public async Task<Result<PlexAccount>> UpdatePlexAccountAsync(dynamic plexAccountDto)
        {
            var plexAccountDb = await _mediator.Send(new GetPlexAccountByIdQuery(plexAccountDto.Id));
            if (plexAccountDb.IsFailed)
            {
                return plexAccountDb;
            }

            plexAccountDb.Value.IsEnabled = plexAccountDto.IsEnabled;
            plexAccountDb.Value.IsMain = plexAccountDto.IsMain;
            plexAccountDb.Value.DisplayName = plexAccountDto.DisplayName;
            plexAccountDb.Value.Username = plexAccountDto.Username;
            plexAccountDb.Value.Password = plexAccountDto.Password;

            return await UpdatePlexAccountAsync(plexAccountDb.Value);
        }

        public async Task<Result<PlexAccount>> UpdatePlexAccountAsync(PlexAccount plexAccount)
        {
            var result = await _mediator.Send(new UpdatePlexAccountCommand(plexAccount));
            if (result.IsFailed)
            {
                string msg = "Failed to validate the PlexAccount that will be updated";
                result.Errors.Add(new Error(msg));
                return result.ToResult().LogError();
            }

            var plexAccountDb = await _mediator.Send(new GetPlexAccountByIdQuery(plexAccount.Id));
            if (plexAccountDb.IsFailed)
            {
                return plexAccountDb;
            }

            // Re-validate if the password changed
            if (plexAccountDb.Value != null && plexAccountDb.Value.Password != plexAccount.Password)
            {
                await SetupAccountAsync(plexAccountDb.Value);
                return await GetPlexAccountAsync(plexAccountDb.Value.Id);
            }

            return plexAccountDb;
        }

        /// <summary>
        /// Hard deletes the PlexAccount from the Database.
        /// </summary>
        /// <param name="plexAccountId"></param>
        /// <returns></returns>
        public async Task<Result<bool>> DeletePlexAccountAsync(int plexAccountId)
        {
            var deleteAccountResult = await _mediator.Send(new DeletePlexAccountCommand(plexAccountId));
            if (deleteAccountResult.IsFailed)
            {
                return deleteAccountResult;
            }

            return await _plexServerService.RemoveInaccessibleServers();
        }

        #endregion
    }
}