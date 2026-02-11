using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedPlexApiPrefixToIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RatingKey",
                table: "PlexTvShowSeason",
                newName: "PlexApiRatingKey");

            migrationBuilder.RenameColumn(
                name: "MetaDataKey",
                table: "PlexTvShowSeason",
                newName: "PlexApiMetaDataKey");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowSeason_RatingKey_PlexServerId",
                table: "PlexTvShowSeason",
                newName: "IX_PlexTvShowSeason_PlexApiRatingKey_PlexServerId");

            migrationBuilder.RenameColumn(
                name: "RatingKey",
                table: "PlexTvShows",
                newName: "PlexApiRatingKey");

            migrationBuilder.RenameColumn(
                name: "MetaDataKey",
                table: "PlexTvShows",
                newName: "PlexApiMetaDataKey");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShows_RatingKey_PlexServerId",
                table: "PlexTvShows",
                newName: "IX_PlexTvShows_PlexApiRatingKey_PlexServerId");

            migrationBuilder.RenameColumn(
                name: "RatingKey",
                table: "PlexTvShowEpisodes",
                newName: "PlexApiRatingKey");

            migrationBuilder.RenameColumn(
                name: "MetaDataKey",
                table: "PlexTvShowEpisodes",
                newName: "PlexApiMetaDataKey");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowEpisodes_RatingKey_PlexServerId",
                table: "PlexTvShowEpisodes",
                newName: "IX_PlexTvShowEpisodes_PlexApiRatingKey_PlexServerId");

            migrationBuilder.RenameColumn(
                name: "RatingKey",
                table: "PlexMovie",
                newName: "PlexApiRatingKey");

            migrationBuilder.RenameColumn(
                name: "MetaDataKey",
                table: "PlexMovie",
                newName: "PlexApiMetaDataKey");

            migrationBuilder.RenameIndex(
                name: "IX_PlexMovie_RatingKey_PlexServerId",
                table: "PlexMovie",
                newName: "IX_PlexMovie_PlexApiRatingKey_PlexServerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PlexApiRatingKey",
                table: "PlexTvShowSeason",
                newName: "RatingKey");

            migrationBuilder.RenameColumn(
                name: "PlexApiMetaDataKey",
                table: "PlexTvShowSeason",
                newName: "MetaDataKey");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowSeason_PlexApiRatingKey_PlexServerId",
                table: "PlexTvShowSeason",
                newName: "IX_PlexTvShowSeason_RatingKey_PlexServerId");

            migrationBuilder.RenameColumn(
                name: "PlexApiRatingKey",
                table: "PlexTvShows",
                newName: "RatingKey");

            migrationBuilder.RenameColumn(
                name: "PlexApiMetaDataKey",
                table: "PlexTvShows",
                newName: "MetaDataKey");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShows_PlexApiRatingKey_PlexServerId",
                table: "PlexTvShows",
                newName: "IX_PlexTvShows_RatingKey_PlexServerId");

            migrationBuilder.RenameColumn(
                name: "PlexApiRatingKey",
                table: "PlexTvShowEpisodes",
                newName: "RatingKey");

            migrationBuilder.RenameColumn(
                name: "PlexApiMetaDataKey",
                table: "PlexTvShowEpisodes",
                newName: "MetaDataKey");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowEpisodes_PlexApiRatingKey_PlexServerId",
                table: "PlexTvShowEpisodes",
                newName: "IX_PlexTvShowEpisodes_RatingKey_PlexServerId");

            migrationBuilder.RenameColumn(
                name: "PlexApiRatingKey",
                table: "PlexMovie",
                newName: "RatingKey");

            migrationBuilder.RenameColumn(
                name: "PlexApiMetaDataKey",
                table: "PlexMovie",
                newName: "MetaDataKey");

            migrationBuilder.RenameIndex(
                name: "IX_PlexMovie_PlexApiRatingKey_PlexServerId",
                table: "PlexMovie",
                newName: "IX_PlexMovie_RatingKey_PlexServerId");
        }
    }
}
