using Microsoft.EntityFrameworkCore.Migrations;

namespace PlexRipper.Data.Migrations
{
    public partial class FullAndSortTitleToPlexMedia : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ServerToken",
                table: "DownloadTasks",
                newName: "Quality");

            migrationBuilder.AddColumn<string>(
                name: "FullTitle",
                table: "PlexTvShowSeason",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SortTitle",
                table: "PlexTvShowSeason",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullTitle",
                table: "PlexTvShows",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SortTitle",
                table: "PlexTvShows",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullTitle",
                table: "PlexTvShowEpisodes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SortTitle",
                table: "PlexTvShowEpisodes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullTitle",
                table: "PlexMovie",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SortTitle",
                table: "PlexMovie",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileLocationUrl",
                table: "DownloadTasks",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullTitle",
                table: "PlexTvShowSeason");

            migrationBuilder.DropColumn(
                name: "SortTitle",
                table: "PlexTvShowSeason");

            migrationBuilder.DropColumn(
                name: "FullTitle",
                table: "PlexTvShows");

            migrationBuilder.DropColumn(
                name: "SortTitle",
                table: "PlexTvShows");

            migrationBuilder.DropColumn(
                name: "FullTitle",
                table: "PlexTvShowEpisodes");

            migrationBuilder.DropColumn(
                name: "SortTitle",
                table: "PlexTvShowEpisodes");

            migrationBuilder.DropColumn(
                name: "FullTitle",
                table: "PlexMovie");

            migrationBuilder.DropColumn(
                name: "SortTitle",
                table: "PlexMovie");

            migrationBuilder.DropColumn(
                name: "FileLocationUrl",
                table: "DownloadTasks");

            migrationBuilder.RenameColumn(
                name: "Quality",
                table: "DownloadTasks",
                newName: "ServerToken");
        }
    }
}
