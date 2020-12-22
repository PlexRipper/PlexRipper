using System.Diagnostics;
using System.Linq;
using System.Threading;
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
        private async Task<Result<PlexLibrary>> RefreshPlexTvShowLibrary(string authToken, PlexLibrary plexLibrary)
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
            await _signalRService.SendLibraryProgressUpdate(plexLibrary.Id, 0, plexLibrary.TvShows.Count);
            var timer = new Stopwatch();
            timer.Start();

            int finishedCount = 0;
            int errorCount = 0;

            plexLibrary.TvShows.AsParallel().WithDegreeOfParallelism(10).ForAll(async plexTvShow =>
            {
                plexTvShow.Seasons = await _plexServiceApi.GetSeasonsAsync(authToken, serverUrl, plexTvShow);

                // Retrieve the episodes for every season
                if (plexTvShow.Seasons.Any())
                {
                    // We use Task.WhenAll here because we need to await the season retrieval before continuing.
                    var tasks = plexTvShow.Seasons.Select(async season =>
                    {
                        season.PlexLibraryId = plexLibrary.Id;
                        season.PlexLibrary = plexLibrary;
                        season.TvShow = plexTvShow;

                        var episodes = await _plexServiceApi.GetEpisodesAsync(authToken, serverUrl, season);
                        if (episodes.IsFailed)
                        {
                            Interlocked.Increment(ref errorCount);
                            return;
                        }

                        // Set the correct plexLibraryId
                        episodes.Value.ForEach(x => x.PlexLibraryId = plexLibrary.Id);
                        season.MediaSize = episodes.Value.Sum(x => x.MediaSize);

                        season.Episodes = episodes.Value;
                    }).ToArray();

                    await Task.WhenAll(tasks);
                    plexTvShow.MediaSize = plexTvShow.Seasons.Sum(x => x.MediaSize);
                }

                SendProgress();
            });

            void SendProgress()
            {
                Interlocked.Increment(ref finishedCount);
                Log.Debug($"{Interlocked.CompareExchange(ref finishedCount, 0, 0)}");
                Task.Run(() => _signalRService.SendLibraryProgressUpdate(plexLibrary.Id, Interlocked.CompareExchange(ref finishedCount, 0, 0),
                    plexLibrary.TvShows.Count));
            }

            Log.Debug($"Finished retrieving all media for library {plexLibraryDb.Title} in {timer.Elapsed.TotalSeconds} with {errorCount} errors.");

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

            var freshPlexLibrary = await _mediator.Send(new GetPlexLibraryByIdQuery(plexLibrary.Id, false, true));

            // Complete progress update
            await _signalRService.SendLibraryProgressUpdate(plexLibrary.Id, plexLibrary.TvShows.Count, plexLibrary.TvShows.Count);
            return freshPlexLibrary;
        }

        /// <summary>
        /// Refresh the <see cref="PlexLibrary"/>, by first deleting all (related) media and the re-adding the media again.
        /// </summary>
        /// <param name="plexLibrary">The <see cref="PlexLibrary"/> to refresh.</param>
        /// <returns>The refreshed <see cref="PlexLibrary"/> with all its media and <see cref="PlexServer"/> reference.</returns>
        private async Task<Result<PlexLibrary>> RefreshPlexMovieLibrary(PlexLibrary plexLibrary)
        {
            if (plexLibrary == null)
            {
                return ResultExtensions.IsNull(nameof(plexLibrary));
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
            return await _mediator.Send(new GetPlexLibraryByIdQuery(plexLibrary.Id));
        }

        #endregion

        #region Public

        /// <inheritdoc/>
        public async Task<Result<PlexLibrary>> GetPlexLibraryAsync(int libraryId, int plexAccountId = 0)
        {
            await _signalRService.SendLibraryProgressUpdate(libraryId, 0, 1, false);

            var libraryDB = await _mediator.Send(new GetPlexLibraryByIdQuery(libraryId, true, true));

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
                    return refreshResult;
                }

                // Re-retrieve from database with all includes
                return await _mediator.Send(new GetPlexLibraryByIdQuery(refreshResult.Value.Id, true, true));
            }

            await _signalRService.SendLibraryProgressUpdate(libraryId, 1, 1, false);
            return libraryDB;
        }

        /// <inheritdoc/>
        public async Task<Result<PlexServer>> GetPlexLibraryInServerAsync(int libraryId, int plexAccountId = 0)
        {
            var plexLibrary = await GetPlexLibraryAsync(libraryId, plexAccountId);
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