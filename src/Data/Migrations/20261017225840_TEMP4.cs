using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class TEMP4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlexTvShowSeason_TvShowId",
                table: "PlexTvShowSeason");

            migrationBuilder.DropIndex(
                name: "IX_PlexMusicTrackComparisons_RemotePlexLibraryId_OwnedPlexLibraryId",
                table: "PlexMusicTrackComparisons");

            migrationBuilder.DropIndex(
                name: "IX_PlexMusicArtistComparisons_RemotePlexLibraryId_OwnedPlexLibraryId",
                table: "PlexMusicArtistComparisons");

            migrationBuilder.DropIndex(
                name: "IX_PlexMusicAlbumComparisons_RemotePlexLibraryId_OwnedPlexLibraryId",
                table: "PlexMusicAlbumComparisons");

            migrationBuilder.DropColumn(
                name: "VideoResolution",
                table: "PlexTvShowEpisodeData");

            migrationBuilder.DropColumn(
                name: "VideoResolution",
                table: "PlexPhotoData");

            migrationBuilder.DropColumn(
                name: "OriginalFilePath",
                table: "PlexOtherVideoData");

            migrationBuilder.DropColumn(
                name: "SourceRelativePath",
                table: "PlexOtherVideoData");

            migrationBuilder.DropColumn(
                name: "VideoResolution",
                table: "PlexOtherVideoData");

            migrationBuilder.DropColumn(
                name: "OriginalFilePath",
                table: "PlexMusicTrackData");

            migrationBuilder.DropColumn(
                name: "SourceRelativePath",
                table: "PlexMusicTrackData");

            migrationBuilder.DropColumn(
                name: "VideoResolution",
                table: "PlexMovieData");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VideoResolution",
                table: "PlexTvShowEpisodeData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.AddColumn<int>(
                name: "VideoResolution",
                table: "PlexPhotoData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFilePath",
                table: "PlexOtherVideoData",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceRelativePath",
                table: "PlexOtherVideoData",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VideoResolution",
                table: "PlexOtherVideoData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFilePath",
                table: "PlexMusicTrackData",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceRelativePath",
                table: "PlexMusicTrackData",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VideoResolution",
                table: "PlexMovieData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowSeason_TvShowId",
                table: "PlexTvShowSeason",
                column: "TvShowId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexMusicTrackComparisons_RemotePlexLibraryId_OwnedPlexLibraryId",
                table: "PlexMusicTrackComparisons",
                columns: new[] { "RemotePlexLibraryId", "OwnedPlexLibraryId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexMusicArtistComparisons_RemotePlexLibraryId_OwnedPlexLibraryId",
                table: "PlexMusicArtistComparisons",
                columns: new[] { "RemotePlexLibraryId", "OwnedPlexLibraryId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexMusicAlbumComparisons_RemotePlexLibraryId_OwnedPlexLibraryId",
                table: "PlexMusicAlbumComparisons",
                columns: new[] { "RemotePlexLibraryId", "OwnedPlexLibraryId" });
        }
    }
}
