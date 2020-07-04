using FluentResults;
using MediatR;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.PlexApi;
using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Application.PlexLibraries.Commands;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlexRipper.Application.PlexLibraries
{
    public class PlexLibraryService : IPlexLibraryService
    {
        private readonly IMediator _mediator;
        private readonly IPlexLibraryRepository _plexLibraryRepository;
        private readonly IPlexMovieService _plexMovieService;
        private readonly IPlexSerieService _plexSerieService;
        private readonly IPlexApiService _plexServiceApi;

        public PlexLibraryService(
            IMediator mediator,
            IPlexApiService plexServiceApi,
            IPlexLibraryRepository plexLibraryRepository,
            IPlexMovieService plexMovieService,
            IPlexSerieService plexSerieService)
        {
            _mediator = mediator;
            _plexLibraryRepository = plexLibraryRepository;
            _plexMovieService = plexMovieService;
            _plexSerieService = plexSerieService;
            _plexServiceApi = plexServiceApi;
        }

        /// <summary>
        /// Retrieve the latest <see cref="PlexLibrary">PlexLibraries</see> for this <see cref="PlexServer"/> which the <see cref="PlexAccount"/> has access to and update the database.
        /// </summary>
        /// <param name="plexAccount"></param>
        /// <param name="plexServer"></param>
        /// <returns>If successful</returns>
        public async Task<Result<bool>> RefreshLibrariesAsync(PlexAccount plexAccount, PlexServer plexServer)
        {
            if (plexServer == null)
            {
                string msg = "plexServer was null";
                Log.Warning(msg);
                return Result.Fail(msg);
            }

            Log.Debug($"Refreshing PlexLibraries for plexServer: {plexServer.Name}");

            var libraries = await _plexServiceApi.GetLibrarySectionsAsync(plexServer.AccessToken, plexServer.BaseUrl);

            if (!libraries.Any())
            {
                string msg = $"plexLibraries returned for server {plexServer.Name} - {plexServer.BaseUrl} was empty";
                Log.Warning(msg);
                return Result.Fail(msg);
            }

            return await _mediator.Send(new AddOrUpdatePlexLibrariesCommand(plexAccount, plexServer, libraries));
        }

        public Task<PlexMediaMetaData> GetMetaDataAsync(PlexMovie movie)
        {
            var plexServer = movie.PlexLibrary.PlexServer;
            return _plexServiceApi.GetMediaMetaDataAsync(plexServer.AccessToken, movie.MetaDataUrl);
        }

        /// <summary>
        /// Returns the <see cref="PlexLibrary"/> with the media content.
        /// </summary>
        /// <param name="plexServer"></param>
        /// <param name="libraryKey"></param>
        /// <returns></returns>
        public Task<PlexLibrary> GetLibraryMediaAsync(PlexServer plexServer, string libraryKey, bool refresh = false)
        {
            var plexLibrary = plexServer.PlexLibraries.ToList().Find(x => x.Key == libraryKey);
            return GetLibraryMediaAsync(plexLibrary, refresh);
        }

        /// <summary>
        /// Returns the <see cref="PlexLibrary"/> with the media content.
        /// </summary>
        /// <param name="plexLibrary"></param>
        /// <param name="refresh"></param>
        /// <returns></returns>
        public async Task<PlexLibrary> GetLibraryMediaAsync(PlexLibrary plexLibrary, bool refresh = false)
        {
            if (refresh || !plexLibrary.HasMedia)
            {
                await RefreshLibraryMediaAsync(plexLibrary);
            }
            return plexLibrary;

            // Create library

            //var metaData = await _plexServiceApi.GetMediaMetaDataAsync(plexServer.AccessToken, plexServer.BaseUrl, 5516);
            // string downloadUrl = _plexApi.GetDownloadUrl(plexServer, metaData);
            //string filename = _plexApi.GetDownloadFilename(plexServer, metaData);
            //_plexApi.DownloadMedia(plexServer.AccessToken, downloadUrl, filename);
        }


        /// <summary>
        /// Retrieves the new media metadata from the PlexApi and stores it in the database.
        /// </summary>
        /// <param name="plexLibrary">The <see cref="PlexLibrary"/> to retrieve</param>
        /// <returns>Returns the PlexLibrary with the containing media</returns>
        public async Task<Result<PlexLibrary>> RefreshLibraryMediaAsync(PlexLibrary plexLibrary)
        {
            if (plexLibrary == null)
            {
                Log.Warning($"{nameof(RefreshLibraryMediaAsync)} => The plexLibrary was null");
                return null;
            }

            plexLibrary = await _plexServiceApi.GetLibraryMediaAsync(plexLibrary, plexLibrary.PlexServer.AccessToken, plexLibrary.PlexServer.BaseUrl);

            switch (plexLibrary.GetMediaType)
            {
                case PlexMediaType.Movie:
                    await _plexMovieService.AddOrUpdatePlexMoviesAsync(plexLibrary, plexLibrary.Movies);
                    break;
                case PlexMediaType.Serie:
                    await _plexSerieService.AddOrUpdatePlexSeriesAsync(plexLibrary, plexLibrary.Series);
                    break;
            }

            var library = await _plexLibraryRepository.GetAsync(plexLibrary.Id);
            return Result.Ok(library);
        }


        #region CRUD

        /// <summary>
        /// Return the PlexLibrary by the Id, will refresh if the library has no media assigned.
        /// </summary>
        /// <param name="libraryId"></param>
        /// <returns></returns>
        public async Task<PlexLibrary> GetPlexLibraryAsync(int libraryId, bool refresh = false)
        {
            if (libraryId <= 0)
            {
                Log.Warning($"{nameof(GetPlexLibrariesAsync)} => The PlexLibraryId was invalid: {libraryId}");
                return null;
            }

            var libraryDB = await _plexLibraryRepository.GetAsync(libraryId);

            if (libraryDB == null)
            {
                Log.Warning($"{nameof(GetPlexLibrariesAsync)} => A PlexLibrary with id {libraryId} does not exist in the database");
                return null;
            }

            if (!libraryDB.HasMedia)
            {
                Log.Debug($"{nameof(GetPlexLibrariesAsync)} => PlexLibrary with id {libraryId} has no media, forcing refresh from the PlexApi");
                var result = await RefreshLibraryMediaAsync(libraryDB);
                // TODO Function needs to be converted
                libraryDB = result.Value;
            }

            return libraryDB;
        }




        public async Task<List<PlexLibrary>> GetPlexLibrariesAsync(PlexAccount plexAccount, PlexServer plexServer, bool refresh = false)
        {
            if (plexServer == null)
            {
                Log.Warning($"{nameof(GetPlexLibrariesAsync)} => The PlexLibrary was null");
                return new List<PlexLibrary>();
            }

            // Retrieve all plexLibraries
            var plexLibraryList = await _plexLibraryRepository.GetLibraries(plexServer.Id);

            if (refresh || !plexLibraryList.Any())
            {
                if (!plexLibraryList.Any())
                {
                    Log.Warning($"{nameof(GetPlexLibrariesAsync)} => PlexAccount {plexServer.Id} did not have any PlexServers assigned. Forcing {nameof(RefreshLibrariesAsync)} was null");
                }

                var refreshSuccess = await RefreshLibrariesAsync(plexAccount, plexServer);
                if (refreshSuccess.IsSuccess && refreshSuccess.Value)
                {
                    plexLibraryList = await _plexLibraryRepository.GetLibraries(plexServer.Id);
                }
            }

            return plexLibraryList.ToList();
        }

        private async Task AddOrUpdatePlexLibrariesAsync(PlexServer plexServer, List<PlexLibrary> libraries)
        {

            if (plexServer == null)
            {
                Log.Warning("plexServer was null");
                return;
            }

            if (libraries.Count == 0)
            {
                Log.Warning("libraries list was empty");
                return;
            }

            Log.Debug("Starting adding or updating plex libraries");

            // Add or update the plex libraries
            foreach (var plexLibrary in libraries)
            {
                // Create
                var plexLibraryDB = await _plexLibraryRepository.FindAsync(x => x.PlexServerId == plexServer.Id && x.Uuid == plexLibrary.Uuid);

                plexLibrary.PlexServerId = plexServer.Id;

                if (plexLibraryDB != null)
                {
                    // Update
                    plexLibrary.Id = plexLibraryDB.Id;
                    await _plexLibraryRepository.UpdateAsync(plexLibrary);
                }
                else
                {
                    //Create
                    await _plexLibraryRepository.AddAsync(plexLibrary);
                }
            }

            // TODO Add check if it was successful


        }



        #endregion
    }
}
