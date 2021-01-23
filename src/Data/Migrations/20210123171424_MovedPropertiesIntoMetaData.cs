using Microsoft.EntityFrameworkCore.Migrations;

namespace PlexRipper.Data.Migrations
{
    public partial class MovedPropertiesIntoMetaData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileLocationUrl",
                table: "DownloadTasks");

            migrationBuilder.DropColumn(
                name: "ReleaseYear",
                table: "DownloadTasks");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "DownloadTasks");

            migrationBuilder.DropColumn(
                name: "TitleTvShow",
                table: "DownloadTasks");

            migrationBuilder.DropColumn(
                name: "TitleTvShowSeason",
                table: "DownloadTasks");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileLocationUrl",
                table: "DownloadTasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReleaseYear",
                table: "DownloadTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "DownloadTasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleTvShow",
                table: "DownloadTasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleTvShowSeason",
                table: "DownloadTasks",
                type: "TEXT",
                nullable: true);
        }
    }
}
