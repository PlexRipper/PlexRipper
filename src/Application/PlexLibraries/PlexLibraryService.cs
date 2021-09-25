using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentResultExtensions.lib;
using FluentResults;
using Logging;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.WebApi;
using PlexRipper.Application.PlexMovies;
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
        /// Retrieves all TvShow, season and episode data and stores it in the database.
        /// </summary>
        /// <param name="authToken"></param>
        /// <param name="plexLibrary"></param>
        /// <param name="progressAction"></param>
        /// <returns></returns>
        private async Task<Result> RefreshPlexTvShowLibrary(string authToken, PlexLibrary plexLibrary, Action<LibraryProgress> progressAction = null)
        {
            if (plexLibrary == null)
                return ResultExtensions.IsNull("plexLibrary").LogError();

            if (plexLibrary.Type != PlexMediaType.TvShow)
                return Result.Fail("PlexLibrary is not of type TvShow").LogError();

            if (plexLibrary.TvShows.Count == 0)
                return Result.Fail(
                        $"PlexLibrary {plexLibrary.Name} with id {plexLibrary.Id} does not contain any TvShows and thus cannot request the corresponding media")
                    .LogError();

            // Send progress
            void SendProgress(int index, int count)
            {
                progressAction?.Invoke(new LibraryProgress(plexLibrary.Id, index, count));
                _signalRService.SendLibraryProgressUpdate(plexLibrary.Id, index, count);
            }

            var result = await _mediator.Send(new GetPlexLibraryByIdWithServerQuery(plexLibrary.Id));
            if (result.IsFailed)
            {
                return result;
            }

            // Request seasons and episodes for every tv show
            var plexLibraryDb = result.Value;
            var serverUrl = plexLibraryDb.PlexServer.ServerUrl;
            SendProgress(0, 4);

            var timer = new Stopwatch();
            timer.Start();

            var rawSeasonDataResult = await _plexServiceApi.GetAllSeasonsAsync(authToken, serverUrl, plexLibrary.Key);
            if (rawSeasonDataResult.IsFailed)
            {
                return rawSeasonDataResult.ToResult();
            }

            // Phase 1 of 4: Season data was retrieved successfully.
            SendProgress(1, 4);

            var rawEpisodesDataResult = await _plexServiceApi.GetAllEpisodesAsync(authToken, serverUrl, plexLibrary.Key);
            if (rawEpisodesDataResult.IsFailed)
            {
                return rawEpisodesDataResult.ToResult();
            }

            // Phase 2 of 4: Episode data was retrieved successfully.
            SendProgress(2, 4);

            foreach (var plexTvShow in plexLibrary.TvShows)
            {
                plexTvShow.Seasons = rawSeasonDataResult.Value.FindAll(x => x.ParentKey == plexTvShow.Key);
                plexTvShow.ChildCount = plexTvShow.Seasons.Count;

                foreach (var plexTvShowSeason in plexTvShow.Seasons)
                {
                    plexTvShowSeason.PlexLibraryId = plexLibrary.Id;
                    plexTvShowSeason.PlexLibrary = plexLibrary;
                    plexTvShowSeason.TvShow = plexTvShow;
                    plexTvShowSeason.Episodes = rawEpisodesDataResult.Value.FindAll(x => x.ParentKey == plexTvShowSeason.Key);
                    plexTvShowSeason.ChildCount = plexTvShowSeason.Episodes.Count;

                    // Assume the season started on the year of the first episode
                    if (plexTvShowSeason.Year == 0 && plexTvShowSeason.Episodes.Any())
                    {
                        plexTvShowSeason.Year = plexTvShowSeason.Episodes.First().Year;
                    }

                    // Set libraryId in each episode
                    plexTvShowSeason.Episodes.ForEach(x => x.PlexLibraryId = plexLibrary.Id);
                    plexTvShowSeason.MediaSize = plexTvShowSeason.Episodes.Sum(x => x.MediaSize);
                }

                plexTvShow.MediaSize = plexTvShow.Seasons.Sum(x => x.MediaSize);
            }

            // Phase 3 of 4: PlexLibrary media data was parsed successfully.
            SendProgress(3, 4);
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

            var createResult = await _mediator.Send(new CreateUpdateOrDeletePlexTvShowsCommand(plexLibrary));
            if (createResult.IsFailed)
            {
                return createResult.ToResult();
            }

            Log.Debug($"Finished updating all media in the database for library {plexLibraryDb.Title} in {timer.Elapsed.TotalSeconds}");

            // Phase 4 of 4: Database has been successfully updated with new library data.
            SendProgress(4, 4);
            return Result.Ok();
        }

        /// <summary>
        /// Refresh the <see cref="PlexLibrary"/>, by first deleting all (related) media and the re-adding the media again.
        /// </summary>
        /// <param name="plexLibrary">The <see cref="PlexLibrary"/> to refresh.</param>
        /// <param name="progressAction"></param>
        /// <returns></returns>
        private async Task<Result> RefreshPlexMovieLibrary(PlexLibrary plexLibrary, Action<LibraryProgress> progressAction = null)
        {
            if (plexLibrary == null)
            {
                return ResultExtensions.IsNull(nameof(plexLibrary));
            }

            // Send progress
            void SendProgress(int index, int count)
            {
                progressAction?.Invoke(new LibraryProgress(plexLibrary.Id, index, count));
                _signalRService.SendLibraryProgressUpdate(plexLibrary.Id, index, count);
            }

            SendProgress(0, 3);

            // Update the MetaData of this library
            var updateMetaDataResult = plexLibrary.UpdateMetaData();
            if (updateMetaDataResult.IsFailed)
            {
                return updateMetaDataResult;
            }

            SendProgress(1, 3);

            var updateResult = await _mediator.Send(new UpdatePlexLibraryByIdCommand(plexLibrary));
            if (updateResult.IsFailed)
            {
                return updateResult.ToResult();
            }

            SendProgress(2, 3);

            var createResult = await _mediator.Send(new CreateUpdateOrDeletePlexMoviesCommand(plexLibrary));
            if (createResult.IsFailed)
            {
                return createResult.ToResult();
            }

            SendProgress(3, 3);
            return Result.Ok();
        }

        #endregion

        #region Public

        /// <inheritdoc/>
        public async Task<Result<PlexLibrary>> GetPlexLibraryAsync(int libraryId, bool topLevelMediaOnly = false)
        {
            var libraryDB = await _mediator.Send(new GetPlexLibraryByIdQuery(libraryId));

            if (libraryDB.IsFailed)
            {
                return libraryDB;
            }

            if (!libraryDB.Value.HasMedia)
            {
                Log.Information($"PlexLibrary with id {libraryId} has no media, forcing refresh from the PlexApi");

                var refreshResult = await RefreshLibraryMediaAsync(libraryId);
                if (refreshResult.IsFailed)
                {
                    return refreshResult.ToResult();
                }
            }

            return await _mediator.Send(new GetPlexLibraryByIdQuery(libraryId, true, true, topLevelMediaOnly));
        }

        public async Task<Result<List<PlexLibrary>>> GetAllPlexLibrariesAsync()
        {
            return await _mediator.Send(new GetAllPlexLibrariesQuery());
        }

        /// <inheritdoc/>
        public async Task<Result<PlexServer>> GetPlexLibraryInServerAsync(int libraryId, bool topLevelMediaOnly = false)
        {
            var plexLibrary = await GetPlexLibraryAsync(libraryId, topLevelMediaOnly);
            if (plexLibrary.IsFailed)
            {
                return plexLibrary.ToResult();
            }

            var plexServer = plexLibrary.Value.PlexServer;
            plexServer.PlexLibraries.Clear();
            plexServer.PlexLibraries.Add(plexLibrary.Value);
            return Result.Ok(plexServer);
        }

        public async Task<Result> UpdateDefaultDestinationLibrary(int libraryId, int folderPathId)
        {
            return await _mediator.Send(new UpdatePlexLibraryDefaultDestinationByIdCommand(libraryId, folderPathId));
        }

        #region RefreshLibrary

        /// <inheritdoc/>
        public async Task<Result> RetrieveAccessibleLibrariesAsync(PlexAccount plexAccount, PlexServer plexServer)
        {
            if (plexServer == null)
            {
                return Result.Fail("plexServer was null").LogWarning();
            }

            Log.Debug($"Refreshing PlexLibraries for plexServer: {plexServer.Name}");

            var authToken = await _plexAuthenticationService.GetPlexServerTokenAsync(plexServer.Id);

            if (authToken.IsFailed)
            {
                return Result.Fail(new Error("Failed to retrieve the server auth token"));
            }

            var libraries = await _plexServiceApi.GetLibrarySectionsAsync(authToken.Value, plexServer.ServerUrl);
            if (libraries.IsFailed)
            {
                return libraries.ToResult();
            }

            if (!libraries.Value.Any())
            {
                string msg = $"plexLibraries returned for server {plexServer.Name} - {plexServer.ServerUrl} was empty";
                return Result.Fail(msg).LogWarning();
            }

            return await _mediator.Send(new AddOrUpdatePlexLibrariesCommand(plexAccount, plexServer, libraries.Value));
        }

        /// <inheritdoc/>
        public async Task<Result<PlexLibrary>> RefreshLibraryMediaAsync(int plexLibraryId, Action<LibraryProgress> progressAction = null)
        {
            var plexLibraryResult = await _mediator.Send(new GetPlexLibraryByIdQuery(plexLibraryId, true));
            if (plexLibraryResult.IsFailed)
            {
                return plexLibraryResult;
            }

            var plexLibrary = plexLibraryResult.Value;

            if (plexLibrary.Type != PlexMediaType.Movie && plexLibrary.Type != PlexMediaType.TvShow)
            {
                // TODO Remove this if all media types are supported
                return Result.Fail($"Library type {plexLibrary.Type} is currently not supported by PlexRipper").LogWarning();
            }

            // Get plexServer authToken
            var authToken = await _plexAuthenticationService.GetPlexServerTokenAsync(plexLibrary.PlexServer.Id);
            if (authToken.IsFailed)
            {
                return authToken.ToResult();
            }

            // Retrieve overview of all media belonging to this PlexLibrary
            var newPlexLibraryResult = await _plexServiceApi.GetLibraryMediaAsync(plexLibrary, authToken.Value);
            if (newPlexLibraryResult.IsFailed)
            {
                return newPlexLibraryResult;
            }

            var newPlexLibrary = newPlexLibraryResult.Value;

            // Get the default folder path id for the destination
            newPlexLibrary.DefaultDestinationId = newPlexLibrary.Type.ToDefaultDestinationFolderId();

            switch (newPlexLibrary.Type)
            {
                case PlexMediaType.Movie:
                    return await RefreshPlexMovieLibrary(newPlexLibrary, progressAction);
                case PlexMediaType.TvShow:
                    return await RefreshPlexTvShowLibrary(authToken.Value, newPlexLibrary, progressAction);
            }

            Log.Information($"Successfully refreshed library {newPlexLibrary.Title} with id: {newPlexLibrary.Id}");
            return Result.Ok(newPlexLibrary);
        }

        #endregion

        #endregion

        #endregion
    }
}