using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.PlexMovies;
using PlexRipper.Application.PlexServers;
using PlexRipper.Application.PlexTvShows;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexLibraries
{
    public class PlexLibraryService : IPlexLibraryService
    {
        #region Fields

        private readonly IMediator _mediator;

        private readonly IPlexAuthenticationService _plexAuthenticationService;

        private readonly IPlexApiService _plexServiceApi;

        private readonly ISignalRService _signalRService;

        #endregion

        #region Constructors

        public PlexLibraryService(
            IMediator mediator,
            IPlexAuthenticationService plexAuthenticationService,
            IPlexApiService plexServiceApi,
            ISignalRService signalRService)
        {
            _mediator = mediator;
            _plexAuthenticationService = plexAuthenticationService;
            _plexServiceApi = plexServiceApi;
            _signalRService = signalRService;
        }

        #endregion

        #region Methods

        #region Private

        /// <summary>
        /// Retrieves all tvshow, season and episode data and stores it in the database.
        /// </summary>
        /// <param name="authToken"></param>
        /// <param name="plexLibrary"></param>
        /// <returns></returns>
        private async Task<Result> RefreshPlexTvShowLibrary(string authToken, PlexLibrary plexLibrary)
        {
            if (plexLibrary == null)
                return ResultExtensions.IsNull("plexLibrary").LogError();

            if (plexLibrary.Type != PlexMediaType.TvShow)
                return Result.Fail("PlexLibrary is not of type TvShow").LogError();

            if (plexLibrary.TvShows.Count == 0)
                return Result.Fail("PlexLibrary does not contain any TvShows and thus cannot request the corresponding media").LogError();

            var result = await _mediator.Send(new GetPlexLibraryByIdWithServerQuery(plexLibrary.Id));
            if (result.IsFailed)
            {
                return result;
            }

            // Request seasons and episodes for every tv show
            var plexLibraryDb = result.Value;
            var serverUrl = plexLibraryDb.PlexServer.ServerUrl;
            await _signalRService.SendLibraryProgressUpdate(plexLibrary.Id, 0, 3);
            var timer = new Stopwatch();
            timer.Start();

            var rawSeasonDataResult = await _plexServiceApi.GetAllSeasonsAsync(authToken, serverUrl, plexLibrary.Key);
            if (rawSeasonDataResult.IsFailed)
            {
                return rawSeasonDataResult.ToResult();
            }

            // Phase 1 of 4: Season data was retrieved successfully.
            await _signalRService.SendLibraryProgressUpdate(plexLibrary.Id, 1, 4);

            var rawEpisodesDataResult = await _plexServiceApi.GetAllEpisodesAsync(authToken, serverUrl, plexLibrary.Key);
            if (rawEpisodesDataResult.IsFailed)
            {
                return rawEpisodesDataResult.ToResult();
            }

            // Phase 2 of 4: Episode data was retrieved successfully.
            await _signalRService.SendLibraryProgressUpdate(plexLibrary.Id, 2, 4);

            foreach (var plexTvShow in plexLibrary.TvShows)
            {
                plexTvShow.Seasons = rawSeasonDataResult.Value.FindAll(x => x.ParentKey == plexTvShow.Key);
                foreach (var plexTvShowSeason in plexTvShow.Seasons)
                {
                    plexTvShowSeason.PlexLibraryId = plexLibrary.Id;
                    plexTvShowSeason.PlexLibrary = plexLibrary;
                    plexTvShowSeason.TvShow = plexTvShow;
                    plexTvShowSeason.Episodes = rawEpisodesDataResult.Value.FindAll(x => x.ParentKey == plexTvShowSeason.Key);

                    // Set libraryId in each episode
                    plexTvShowSeason.Episodes.ForEach(x => x.PlexLibraryId = plexLibrary.Id);
                    plexTvShowSeason.MediaSize = plexTvShowSeason.Episodes.Sum(x => x.MediaSize);
                }

                plexTvShow.MediaSize = plexTvShow.Seasons.Sum(x => x.MediaSize);
            }

            // Phase 3 of 4: PlexLibrary media data was parsed successfully.
            await _signalRService.SendLibraryProgressUpdate(plexLibrary.Id, 3, 4);
            Log.Debug($"Finished retrieving all media for library {plexLibraryDb.Title} in {timer.Elapsed.TotalSeconds}");
            timer.Restart();

            // Update the MetaData of this library
            var updateMetaDataResult = plexLibrary.UpdateMetaData();
            if (updateMetaDataResult.IsFailed)
            {
                return updateMetaDataResult;
            }

            var updateResult = await _mediator.Send(new UpdatePlexLibraryByIdCommand(plexLibrary));
            if (updateResult.IsFailed)
            {
                return updateResult.ToResult();
            }

            var deleteResult = await _mediator.Send(new DeleteMediaFromPlexLibraryCommand(plexLibrary.Id));
            if (deleteResult.IsFailed)
            {
                return deleteResult.ToResult();
            }

            var createResult = await _mediator.Send(new CreateUpdateOrDeletePlexTvShowsCommand(plexLibrary));
            if (createResult.IsFailed)
            {
                return createResult.ToResult();
            }

            Log.Debug($"Finished updating all media in the database for library {plexLibraryDb.Title} in {timer.Elapsed.TotalSeconds}");

            // Phase 4 of 4: Database has been successfully updated with new library data.
            await _signalRService.SendLibraryProgressUpdate(plexLibrary.Id, 4, 4);
            return Result.Ok();
        }

        /// <summary>
        /// Refresh the <see cref="PlexLibrary"/>, by first deleting all (related) media and the re-adding the media again.
        /// </summary>
        /// <param name="plexLibrary">The <see cref="PlexLibrary"/> to refresh.</param>
        /// <returns></returns>
        private async Task<Result> RefreshPlexMovieLibrary(PlexLibrary plexLibrary)
        {
            if (plexLibrary == null)
            {
                return ResultExtensions.IsNull(nameof(plexLibrary));
            }

            // Update the MetaData of this library
            var updateMetaDataResult = plexLibrary.UpdateMetaData();
            if (updateMetaDataResult.IsFailed)
            {
                return updateMetaDataResult;
            }

            var updateResult = await _mediator.Send(new UpdatePlexLibraryByIdCommand(plexLibrary));
            if (updateResult.IsFailed)
            {
                return updateResult.ToResult();
            }

            var deleteResult = await _mediator.Send(new DeleteMediaFromPlexLibraryCommand(plexLibrary.Id));
            if (deleteResult.IsFailed)
            {
                return deleteResult.ToResult();
            }

            var createResult = await _mediator.Send(new CreateUpdateOrDeletePlexMoviesCommand(plexLibrary));
            if (createResult.IsFailed)
            {
                return createResult.ToResult();
            }

            await _signalRService.SendLibraryProgressUpdate(plexLibrary.Id, plexLibrary.MediaCount, plexLibrary.MediaCount);
            return Result.Ok();
        }

        #endregion

        #region Public

        /// <inheritdoc/>
        public async Task<Result<PlexLibrary>> GetPlexLibraryAsync(int libraryId, int plexAccountId = 0, bool topLevelMediaOnly = false)
        {
            await _signalRService.SendLibraryProgressUpdate(libraryId, 0, 1, false);

            var libraryDB = await _mediator.Send(new GetPlexLibraryByIdQuery(libraryId));

            if (libraryDB.IsFailed)
            {
                await _signalRService.SendLibraryProgressUpdate(libraryId, 1, 1, false);
                return libraryDB;
            }

            if (!libraryDB.Value.HasMedia)
            {
                Log.Information($"PlexLibrary with id {libraryId} has no media, forcing refresh from the PlexApi");

                var refreshResult = await RefreshLibraryMediaAsync(libraryId, plexAccountId);
                if (refreshResult.IsFailed)
                {
                    return refreshResult.ToResult();
                }
            }

            await _signalRService.SendLibraryProgressUpdate(libraryId, 1, 1, false);
            return await _mediator.Send(new GetPlexLibraryByIdQuery(libraryId, true, true, topLevelMediaOnly));
        }

        /// <inheritdoc/>
        public async Task<Result<PlexServer>> GetPlexLibraryInServerAsync(int libraryId, int plexAccountId = 0, bool topLevelMediaOnly = false)
        {
            var plexLibrary = await GetPlexLibraryAsync(libraryId, plexAccountId, topLevelMediaOnly);
            if (plexLibrary.IsFailed)
            {
                return plexLibrary.ToResult();
            }

            var plexServer = plexLibrary.Value.PlexServer;
            plexServer.PlexLibraries.Clear();
            plexServer.PlexLibraries.Add(plexLibrary.Value);
            return Result.Ok(plexServer);
        }

        #region RefreshLibrary

        /// <inheritdoc/>
        public async Task<Result<bool>> RefreshLibrariesAsync(PlexAccount plexAccount, PlexServer plexServer)
        {
            if (plexServer == null)
            {
                string msg = "plexServer was null";
                Log.Warning(msg);
                return Result.Fail(msg);
            }

            Log.Debug($"Refreshing PlexLibraries for plexServer: {plexServer.Name}");

            var authToken = await _plexAuthenticationService.GetPlexServerTokenAsync(plexServer.Id, plexAccount.Id);

            if (authToken.IsFailed)
            {
                return Result.Fail(new Error("Failed to retrieve the server auth token"));
            }

            var libraries = await _plexServiceApi.GetLibrarySectionsAsync(authToken.Value, plexServer.ServerUrl);

            if (!libraries.Any())
            {
                string msg = $"plexLibraries returned for server {plexServer.Name} - {plexServer.ServerUrl} was empty";
                Log.Warning(msg);
                return Result.Fail(msg);
            }

            return await _mediator.Send(new AddOrUpdatePlexLibrariesCommand(plexAccount, plexServer, libraries));
        }

        /// <inheritdoc/>
        public async Task<Result<PlexLibrary>> RefreshLibraryMediaAsync(int plexLibraryId, int plexAccountId = 0)
        {
            var plexLibraryResult = await _mediator.Send(new GetPlexLibraryByIdQuery(plexLibraryId, true));
            if (plexLibraryResult.IsFailed)
            {
                return plexLibraryResult;
            }

            var plexLibrary = plexLibraryResult.Value;

            await _signalRService.SendLibraryProgressUpdate(plexLibraryId, 0, plexLibrary.MediaCount);

            if (plexLibrary.Type != PlexMediaType.Movie && plexLibrary.Type != PlexMediaType.TvShow)
            {
                // TODO Remove this if all media types are supported
                string msg = $"Library type {plexLibrary.Type} is currently not supported by PlexRipper";
                Log.Warning(msg);
                return Result.Fail(msg);
            }

            // Get plexServer authToken
            var authToken = await _plexAuthenticationService.GetPlexServerTokenAsync(plexLibrary.PlexServer.Id, plexAccountId);
            if (authToken.IsFailed)
            {
                return authToken.ToResult();
            }

            // Retrieve overview of all media belonging to this PlexLibrary
            var newPlexLibrary = await _plexServiceApi.GetLibraryMediaAsync(plexLibrary, authToken.Value);
            var result = Result.Fail($"Failed to refresh library {plexLibrary.Id}");
            if (newPlexLibrary == null)
            {
                return result;
            }

            switch (newPlexLibrary.Type)
            {
                case PlexMediaType.Movie:
                    return await RefreshPlexMovieLibrary(newPlexLibrary);
                case PlexMediaType.TvShow:
                    return await RefreshPlexTvShowLibrary(authToken.Value, newPlexLibrary);
            }

            if (result.IsFailed)
            {
                return result.ToResult<PlexLibrary>();
            }

            Log.Information($"Successfully refreshed library {newPlexLibrary.Title} with id: {newPlexLibrary.Id}");
            return Result.Ok(newPlexLibrary);
        }

        #endregion

        public async Task<Result<byte[]>> GetThumbnailImage(int mediaId, PlexMediaType mediaType, int width = 0, int height = 0)
        {
            var thumbUrl = await _mediator.Send(new GetThumbUrlByPlexMediaIdQuery(mediaId, mediaType));
            if (thumbUrl.IsFailed)
            {
                return thumbUrl.ToResult();
            }

            var plexServer = await _mediator.Send(new GetPlexServerByPlexMediaIdQuery(mediaId, mediaType));
            if (plexServer.IsFailed)
            {
                return plexServer.ToResult();
            }

            var token = await _plexAuthenticationService.GetPlexServerTokenAsync(plexServer.Value.Id);
            if (token.IsFailed)
            {
                return token.ToResult();
            }

            byte[] image = await _plexServiceApi.GetThumbnailAsync(thumbUrl.Value, token.Value, width, height);
            if (image == null || image.Length == 0)
            {
                return Result.Fail("Failed to retrieve image.");
            }

            return Result.Ok(image);
        }

        #endregion

        #endregion
    }
}