using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class TEMP2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quality",
                table: "PlexTvShowSeasonMediaQualities",
                newName: "VideoQuality");

            migrationBuilder.RenameColumn(
                name: "Quality",
                table: "PlexTvShows",
                newName: "VideoQuality");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShows_Quality",
                table: "PlexTvShows",
                newName: "IX_PlexTvShows_VideoQuality");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShows_PlexLibraryId_Quality",
                table: "PlexTvShows",
                newName: "IX_PlexTvShows_PlexLibraryId_VideoQuality");

            migrationBuilder.RenameColumn(
                name: "Quality",
                table: "PlexTvShowMediaQualities",
                newName: "VideoQuality");

            migrationBuilder.RenameColumn(
                name: "Quality",
                table: "PlexTvShowEpisodeData",
                newName: "VideoQuality");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowEpisodeData_Quality",
                table: "PlexTvShowEpisodeData",
                newName: "IX_PlexTvShowEpisodeData_VideoQuality");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowEpisodeData_PlexTvShowEpisodeId_Quality",
                table: "PlexTvShowEpisodeData",
                newName: "IX_PlexTvShowEpisodeData_PlexTvShowEpisodeId_VideoQuality");

            migrationBuilder.RenameColumn(
                name: "Quality",
                table: "PlexPhotoImages",
                newName: "VideoQuality");

            migrationBuilder.RenameColumn(
                name: "Quality",
                table: "PlexPhotoData",
                newName: "VideoQuality");

            migrationBuilder.RenameColumn(
                name: "Quality",
                table: "PlexPhotoAlbums",
                newName: "VideoQuality");

            migrationBuilder.RenameColumn(
                name: "Quality",
                table: "PlexOtherVideos",
                newName: "VideoQuality");

            migrationBuilder.RenameColumn(
                name: "Quality",
                table: "PlexOtherVideoData",
                newName: "VideoQuality");

            migrationBuilder.RenameColumn(
                name: "Quality",
                table: "PlexMusicArtistMediaQualities",
                newName: "AudioQuality");

            migrationBuilder.RenameIndex(
                name: "IX_PlexMusicArtistMediaQualities_PlexLibraryId_PlexMusicArtistId_Quality",
                table: "PlexMusicArtistMediaQualities",
                newName: "IX_PlexMusicArtistMediaQualities_PlexLibraryId_PlexMusicArtistId_AudioQuality");

            migrationBuilder.RenameColumn(
                name: "Quality",
                table: "PlexMusicAlbumMediaQualities",
                newName: "AudioQuality");

            migrationBuilder.RenameIndex(
                name: "IX_PlexMusicAlbumMediaQualities_PlexLibraryId_PlexMusicAlbumId_Quality",
                table: "PlexMusicAlbumMediaQualities",
                newName: "IX_PlexMusicAlbumMediaQualities_PlexLibraryId_PlexMusicAlbumId_AudioQuality");

            migrationBuilder.RenameColumn(
                name: "Quality",
                table: "PlexMovieData",
                newName: "VideoQuality");

            migrationBuilder.RenameIndex(
                name: "IX_PlexMovieData_Quality",
                table: "PlexMovieData",
                newName: "IX_PlexMovieData_VideoQuality");

            migrationBuilder.RenameIndex(
                name: "IX_PlexMovieData_PlexMovieId_Quality",
                table: "PlexMovieData",
                newName: "IX_PlexMovieData_PlexMovieId_VideoQuality");

            migrationBuilder.RenameColumn(
                name: "Quality",
                table: "PlexMovie",
                newName: "VideoQuality");

            migrationBuilder.RenameIndex(
                name: "IX_PlexMovie_Quality",
                table: "PlexMovie",
                newName: "IX_PlexMovie_VideoQuality");

            migrationBuilder.RenameIndex(
                name: "IX_PlexMovie_PlexLibraryId_Quality",
                table: "PlexMovie",
                newName: "IX_PlexMovie_PlexLibraryId_VideoQuality");

            migrationBuilder.RenameColumn(
                name: "Quality",
                table: "DownloadTaskTvShowEpisodeFile",
                newName: "VideoQuality");

            migrationBuilder.RenameColumn(
                name: "Quality",
                table: "DownloadTaskPhotoImageFiles",
                newName: "VideoQuality");

            migrationBuilder.RenameColumn(
                name: "Quality",
                table: "DownloadTaskOtherVideoFiles",
                newName: "VideoQuality");

            migrationBuilder.RenameColumn(
                name: "Quality",
                table: "DownloadTaskMusicTrackFiles",
                newName: "VideoQuality");

            migrationBuilder.RenameColumn(
                name: "Quality",
                table: "DownloadTaskMovieFile",
                newName: "VideoQuality");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VideoQuality",
                table: "PlexTvShowSeasonMediaQualities",
                newName: "Quality");

            migrationBuilder.RenameColumn(
                name: "VideoQuality",
                table: "PlexTvShows",
                newName: "Quality");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShows_VideoQuality",
                table: "PlexTvShows",
                newName: "IX_PlexTvShows_Quality");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShows_PlexLibraryId_VideoQuality",
                table: "PlexTvShows",
                newName: "IX_PlexTvShows_PlexLibraryId_Quality");

            migrationBuilder.RenameColumn(
                name: "VideoQuality",
                table: "PlexTvShowMediaQualities",
                newName: "Quality");

            migrationBuilder.RenameColumn(
                name: "VideoQuality",
                table: "PlexTvShowEpisodeData",
                newName: "Quality");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowEpisodeData_VideoQuality",
                table: "PlexTvShowEpisodeData",
                newName: "IX_PlexTvShowEpisodeData_Quality");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowEpisodeData_PlexTvShowEpisodeId_VideoQuality",
                table: "PlexTvShowEpisodeData",
                newName: "IX_PlexTvShowEpisodeData_PlexTvShowEpisodeId_Quality");

            migrationBuilder.RenameColumn(
                name: "VideoQuality",
                table: "PlexPhotoImages",
                newName: "Quality");

            migrationBuilder.RenameColumn(
                name: "VideoQuality",
                table: "PlexPhotoData",
                newName: "Quality");

            migrationBuilder.RenameColumn(
                name: "VideoQuality",
                table: "PlexPhotoAlbums",
                newName: "Quality");

            migrationBuilder.RenameColumn(
                name: "VideoQuality",
                table: "PlexOtherVideos",
                newName: "Quality");

            migrationBuilder.RenameColumn(
                name: "VideoQuality",
                table: "PlexOtherVideoData",
                newName: "Quality");

            migrationBuilder.RenameColumn(
                name: "AudioQuality",
                table: "PlexMusicArtistMediaQualities",
                newName: "Quality");

            migrationBuilder.RenameIndex(
                name: "IX_PlexMusicArtistMediaQualities_PlexLibraryId_PlexMusicArtistId_AudioQuality",
                table: "PlexMusicArtistMediaQualities",
                newName: "IX_PlexMusicArtistMediaQualities_PlexLibraryId_PlexMusicArtistId_Quality");

            migrationBuilder.RenameColumn(
                name: "AudioQuality",
                table: "PlexMusicAlbumMediaQualities",
                newName: "Quality");

            migrationBuilder.RenameIndex(
                name: "IX_PlexMusicAlbumMediaQualities_PlexLibraryId_PlexMusicAlbumId_AudioQuality",
                table: "PlexMusicAlbumMediaQualities",
                newName: "IX_PlexMusicAlbumMediaQualities_PlexLibraryId_PlexMusicAlbumId_Quality");

            migrationBuilder.RenameColumn(
                name: "VideoQuality",
                table: "PlexMovieData",
                newName: "Quality");

            migrationBuilder.RenameIndex(
                name: "IX_PlexMovieData_VideoQuality",
                table: "PlexMovieData",
                newName: "IX_PlexMovieData_Quality");

            migrationBuilder.RenameIndex(
                name: "IX_PlexMovieData_PlexMovieId_VideoQuality",
                table: "PlexMovieData",
                newName: "IX_PlexMovieData_PlexMovieId_Quality");

            migrationBuilder.RenameColumn(
                name: "VideoQuality",
                table: "PlexMovie",
                newName: "Quality");

            migrationBuilder.RenameIndex(
                name: "IX_PlexMovie_VideoQuality",
                table: "PlexMovie",
                newName: "IX_PlexMovie_Quality");

            migrationBuilder.RenameIndex(
                name: "IX_PlexMovie_PlexLibraryId_VideoQuality",
                table: "PlexMovie",
                newName: "IX_PlexMovie_PlexLibraryId_Quality");

            migrationBuilder.RenameColumn(
                name: "VideoQuality",
                table: "DownloadTaskTvShowEpisodeFile",
                newName: "Quality");

            migrationBuilder.RenameColumn(
                name: "VideoQuality",
                table: "DownloadTaskPhotoImageFiles",
                newName: "Quality");

            migrationBuilder.RenameColumn(
                name: "VideoQuality",
                table: "DownloadTaskOtherVideoFiles",
                newName: "Quality");

            migrationBuilder.RenameColumn(
                name: "VideoQuality",
                table: "DownloadTaskMusicTrackFiles",
                newName: "Quality");

            migrationBuilder.RenameColumn(
                name: "VideoQuality",
                table: "DownloadTaskMovieFile",
                newName: "Quality");
        }
    }
}
