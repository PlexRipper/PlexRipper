using Microsoft.EntityFrameworkCore.Migrations;

namespace PlexRipper.Data.Migrations
{
    public partial class RemoveMetaDataFromDownloadTask : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShow_PlexLibraries_PlexLibraryId",
                table: "PlexTvShow");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShow_PlexServers_PlexServerId",
                table: "PlexTvShow");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowEpisodes_PlexTvShow_TvShowId",
                table: "PlexTvShowEpisodes");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowGenre_PlexTvShow_PlexTvShowId",
                table: "PlexTvShowGenre");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowRole_PlexTvShow_PlexTvShowId",
                table: "PlexTvShowRole");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowSeason_PlexTvShow_TvShowId",
                table: "PlexTvShowSeason");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlexTvShow",
                table: "PlexTvShow");

            migrationBuilder.RenameTable(
                name: "PlexTvShow",
                newName: "PlexTvShows");

            migrationBuilder.RenameColumn(
                name: "MetaData",
                table: "DownloadTasks",
                newName: "FullTitle");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShow_PlexServerId",
                table: "PlexTvShows",
                newName: "IX_PlexTvShows_PlexServerId");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShow_PlexLibraryId",
                table: "PlexTvShows",
                newName: "IX_PlexTvShows_PlexLibraryId");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "DownloadTasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "DownloadTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlexTvShows",
                table: "PlexTvShows",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowEpisodes_PlexTvShows_TvShowId",
                table: "PlexTvShowEpisodes",
                column: "TvShowId",
                principalTable: "PlexTvShows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowGenre_PlexTvShows_PlexTvShowId",
                table: "PlexTvShowGenre",
                column: "PlexTvShowId",
                principalTable: "PlexTvShows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowRole_PlexTvShows_PlexTvShowId",
                table: "PlexTvShowRole",
                column: "PlexTvShowId",
                principalTable: "PlexTvShows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShows_PlexLibraries_PlexLibraryId",
                table: "PlexTvShows",
                column: "PlexLibraryId",
                principalTable: "PlexLibraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShows_PlexServers_PlexServerId",
                table: "PlexTvShows",
                column: "PlexServerId",
                principalTable: "PlexServers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowSeason_PlexTvShows_TvShowId",
                table: "PlexTvShowSeason",
                column: "TvShowId",
                principalTable: "PlexTvShows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowEpisodes_PlexTvShows_TvShowId",
                table: "PlexTvShowEpisodes");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowGenre_PlexTvShows_PlexTvShowId",
                table: "PlexTvShowGenre");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowRole_PlexTvShows_PlexTvShowId",
                table: "PlexTvShowRole");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShows_PlexLibraries_PlexLibraryId",
                table: "PlexTvShows");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShows_PlexServers_PlexServerId",
                table: "PlexTvShows");

            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowSeason_PlexTvShows_TvShowId",
                table: "PlexTvShowSeason");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlexTvShows",
                table: "PlexTvShows");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "DownloadTasks");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "DownloadTasks");

            migrationBuilder.RenameTable(
                name: "PlexTvShows",
                newName: "PlexTvShow");

            migrationBuilder.RenameColumn(
                name: "FullTitle",
                table: "DownloadTasks",
                newName: "MetaData");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShows_PlexServerId",
                table: "PlexTvShow",
                newName: "IX_PlexTvShow_PlexServerId");

            migrationBuilder.RenameIndex(
                name: "IX_PlexTvShows_PlexLibraryId",
                table: "PlexTvShow",
                newName: "IX_PlexTvShow_PlexLibraryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlexTvShow",
                table: "PlexTvShow",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShow_PlexLibraries_PlexLibraryId",
                table: "PlexTvShow",
                column: "PlexLibraryId",
                principalTable: "PlexLibraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShow_PlexServers_PlexServerId",
                table: "PlexTvShow",
                column: "PlexServerId",
                principalTable: "PlexServers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowEpisodes_PlexTvShow_TvShowId",
                table: "PlexTvShowEpisodes",
                column: "TvShowId",
                principalTable: "PlexTvShow",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowGenre_PlexTvShow_PlexTvShowId",
                table: "PlexTvShowGenre",
                column: "PlexTvShowId",
                principalTable: "PlexTvShow",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowRole_PlexTvShow_PlexTvShowId",
                table: "PlexTvShowRole",
                column: "PlexTvShowId",
                principalTable: "PlexTvShow",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowSeason_PlexTvShow_TvShowId",
                table: "PlexTvShowSeason",
                column: "TvShowId",
                principalTable: "PlexTvShow",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
