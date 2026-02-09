using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenamedToPlexApiRatingKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Key",
                table: "PlexTvShowSeason",
                newName: "RatingKey");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowSeason_Key_PlexServerId",
                table: "PlexTvShowSeason",
                newName: "IX_PlexTvShowSeason_RatingKey_PlexServerId");

            migrationBuilder.RenameColumn(
                name: "Key",
                table: "PlexTvShows",
                newName: "RatingKey");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShows_Key_PlexServerId",
                table: "PlexTvShows",
                newName: "IX_PlexTvShows_RatingKey_PlexServerId");

            migrationBuilder.RenameColumn(
                name: "Key",
                table: "PlexTvShowEpisodes",
                newName: "RatingKey");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowEpisodes_Key_PlexServerId",
                table: "PlexTvShowEpisodes",
                newName: "IX_PlexTvShowEpisodes_RatingKey_PlexServerId");

            migrationBuilder.RenameColumn(
                name: "RatingKey",
                table: "PlexTvShowEpisodeData",
                newName: "PlexApiRatingKey");

            migrationBuilder.RenameColumn(
                name: "PlexPartId",
                table: "PlexTvShowEpisodeData",
                newName: "PlexApiPartId");

            migrationBuilder.RenameColumn(
                name: "PlexMediaId",
                table: "PlexTvShowEpisodeData",
                newName: "PlexApiMediaId");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowEpisodeData_RatingKey",
                table: "PlexTvShowEpisodeData",
                newName: "IX_PlexTvShowEpisodeData_PlexApiRatingKey");

            migrationBuilder.RenameColumn(
                name: "RatingKey",
                table: "PlexMovieData",
                newName: "PlexApiRatingKey");

            migrationBuilder.RenameColumn(
                name: "PlexPartId",
                table: "PlexMovieData",
                newName: "PlexApiPartId");

            migrationBuilder.RenameColumn(
                name: "PlexMediaId",
                table: "PlexMovieData",
                newName: "PlexApiMediaId");

            migrationBuilder.RenameIndex(
                name: "IX_PlexMovieData_RatingKey",
                table: "PlexMovieData",
                newName: "IX_PlexMovieData_PlexApiRatingKey");

            migrationBuilder.RenameColumn(
                name: "Key",
                table: "PlexMovie",
                newName: "RatingKey");

            migrationBuilder.RenameIndex(
                name: "IX_PlexMovie_Key_PlexServerId",
                table: "PlexMovie",
                newName: "IX_PlexMovie_RatingKey_PlexServerId");

            migrationBuilder.RenameColumn(
                name: "Key",
                table: "DownloadTaskTvShowSeason",
                newName: "PlexApiRatingKey");

            migrationBuilder.RenameColumn(
                name: "Key",
                table: "DownloadTaskTvShowEpisodeFile",
                newName: "PlexApiRatingKey");

            migrationBuilder.RenameIndex(
                name: "IX_DownloadTaskTvShowEpisodeFile_PlexLibraryId_PlexServerId_Key",
                table: "DownloadTaskTvShowEpisodeFile",
                newName: "IX_DownloadTaskTvShowEpisodeFile_PlexLibraryId_PlexServerId_PlexApiRatingKey");

            migrationBuilder.RenameColumn(
                name: "Key",
                table: "DownloadTaskTvShowEpisode",
                newName: "PlexApiRatingKey");

            migrationBuilder.RenameColumn(
                name: "Key",
                table: "DownloadTaskTvShow",
                newName: "PlexApiRatingKey");

            migrationBuilder.RenameIndex(
                name: "IX_DownloadTaskTvShow_PlexServerId_Key",
                table: "DownloadTaskTvShow",
                newName: "IX_DownloadTaskTvShow_PlexServerId_PlexApiRatingKey");

            migrationBuilder.RenameColumn(
                name: "Key",
                table: "DownloadTaskMovieFile",
                newName: "PlexApiRatingKey");

            migrationBuilder.RenameIndex(
                name: "IX_DownloadTaskMovieFile_PlexLibraryId_PlexServerId_Key",
                table: "DownloadTaskMovieFile",
                newName: "IX_DownloadTaskMovieFile_PlexLibraryId_PlexServerId_PlexApiRatingKey");

            migrationBuilder.RenameColumn(
                name: "Key",
                table: "DownloadTaskMovie",
                newName: "PlexApiRatingKey");

            migrationBuilder.AlterColumn<int>(
                name: "PlexApiRatingKey",
                table: "PlexTvShowEpisodeData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 1)
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<int>(
                name: "PlexApiPartId",
                table: "PlexTvShowEpisodeData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 3)
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<long>(
                name: "PlexApiMediaId",
                table: "PlexTvShowEpisodeData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 2)
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "PlexApiRatingKey",
                table: "PlexMovieData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 1)
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<int>(
                name: "PlexApiPartId",
                table: "PlexMovieData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 3)
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<long>(
                name: "PlexApiMediaId",
                table: "PlexMovieData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 2)
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AddColumn<long>(
                name: "PlexApiMediaId",
                table: "DownloadTaskTvShowEpisodeFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "PlexApiPartId",
                table: "DownloadTaskTvShowEpisodeFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "PlexApiMediaId",
                table: "DownloadTaskMovieFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "PlexApiPartId",
                table: "DownloadTaskMovieFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlexApiMediaId",
                table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropColumn(
                name: "PlexApiPartId",
                table: "DownloadTaskTvShowEpisodeFile");

            migrationBuilder.DropColumn(
                name: "PlexApiMediaId",
                table: "DownloadTaskMovieFile");

            migrationBuilder.DropColumn(
                name: "PlexApiPartId",
                table: "DownloadTaskMovieFile");

            migrationBuilder.RenameColumn(
                name: "RatingKey",
                table: "PlexTvShowSeason",
                newName: "Key");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowSeason_RatingKey_PlexServerId",
                table: "PlexTvShowSeason",
                newName: "IX_PlexTvShowSeason_Key_PlexServerId");

            migrationBuilder.RenameColumn(
                name: "RatingKey",
                table: "PlexTvShows",
                newName: "Key");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShows_RatingKey_PlexServerId",
                table: "PlexTvShows",
                newName: "IX_PlexTvShows_Key_PlexServerId");

            migrationBuilder.RenameColumn(
                name: "RatingKey",
                table: "PlexTvShowEpisodes",
                newName: "Key");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowEpisodes_RatingKey_PlexServerId",
                table: "PlexTvShowEpisodes",
                newName: "IX_PlexTvShowEpisodes_Key_PlexServerId");

            migrationBuilder.RenameColumn(
                name: "PlexApiRatingKey",
                table: "PlexTvShowEpisodeData",
                newName: "RatingKey");

            migrationBuilder.RenameColumn(
                name: "PlexApiPartId",
                table: "PlexTvShowEpisodeData",
                newName: "PlexPartId");

            migrationBuilder.RenameColumn(
                name: "PlexApiMediaId",
                table: "PlexTvShowEpisodeData",
                newName: "PlexMediaId");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShowEpisodeData_PlexApiRatingKey",
                table: "PlexTvShowEpisodeData",
                newName: "IX_PlexTvShowEpisodeData_RatingKey");

            migrationBuilder.RenameColumn(
                name: "PlexApiRatingKey",
                table: "PlexMovieData",
                newName: "RatingKey");

            migrationBuilder.RenameColumn(
                name: "PlexApiPartId",
                table: "PlexMovieData",
                newName: "PlexPartId");

            migrationBuilder.RenameColumn(
                name: "PlexApiMediaId",
                table: "PlexMovieData",
                newName: "PlexMediaId");

            migrationBuilder.RenameIndex(
                name: "IX_PlexMovieData_PlexApiRatingKey",
                table: "PlexMovieData",
                newName: "IX_PlexMovieData_RatingKey");

            migrationBuilder.RenameColumn(
                name: "RatingKey",
                table: "PlexMovie",
                newName: "Key");

            migrationBuilder.RenameIndex(
                name: "IX_PlexMovie_RatingKey_PlexServerId",
                table: "PlexMovie",
                newName: "IX_PlexMovie_Key_PlexServerId");

            migrationBuilder.RenameColumn(
                name: "PlexApiRatingKey",
                table: "DownloadTaskTvShowSeason",
                newName: "Key");

            migrationBuilder.RenameColumn(
                name: "PlexApiRatingKey",
                table: "DownloadTaskTvShowEpisodeFile",
                newName: "Key");

            migrationBuilder.RenameIndex(
                name: "IX_DownloadTaskTvShowEpisodeFile_PlexLibraryId_PlexServerId_PlexApiRatingKey",
                table: "DownloadTaskTvShowEpisodeFile",
                newName: "IX_DownloadTaskTvShowEpisodeFile_PlexLibraryId_PlexServerId_Key");

            migrationBuilder.RenameColumn(
                name: "PlexApiRatingKey",
                table: "DownloadTaskTvShowEpisode",
                newName: "Key");

            migrationBuilder.RenameColumn(
                name: "PlexApiRatingKey",
                table: "DownloadTaskTvShow",
                newName: "Key");

            migrationBuilder.RenameIndex(
                name: "IX_DownloadTaskTvShow_PlexServerId_PlexApiRatingKey",
                table: "DownloadTaskTvShow",
                newName: "IX_DownloadTaskTvShow_PlexServerId_Key");

            migrationBuilder.RenameColumn(
                name: "PlexApiRatingKey",
                table: "DownloadTaskMovieFile",
                newName: "Key");

            migrationBuilder.RenameIndex(
                name: "IX_DownloadTaskMovieFile_PlexLibraryId_PlexServerId_PlexApiRatingKey",
                table: "DownloadTaskMovieFile",
                newName: "IX_DownloadTaskMovieFile_PlexLibraryId_PlexServerId_Key");

            migrationBuilder.RenameColumn(
                name: "PlexApiRatingKey",
                table: "DownloadTaskMovie",
                newName: "Key");

            migrationBuilder.AlterColumn<int>(
                name: "RatingKey",
                table: "PlexTvShowEpisodeData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 3)
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<long>(
                name: "PlexPartId",
                table: "PlexTvShowEpisodeData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 2)
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<long>(
                name: "PlexMediaId",
                table: "PlexTvShowEpisodeData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 1)
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<int>(
                name: "RatingKey",
                table: "PlexMovieData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 3)
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<long>(
                name: "PlexPartId",
                table: "PlexMovieData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 2)
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<long>(
                name: "PlexMediaId",
                table: "PlexMovieData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 1)
                .OldAnnotation("Relational:ColumnOrder", 2);
        }
    }
}
