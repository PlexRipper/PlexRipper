using Microsoft.EntityFrameworkCore.Migrations;

namespace PlexRipper.Data.Migrations
{
    public partial class AddParentKeyToTvShowSeasonAndEpisode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParentKey",
                table: "PlexTvShowSeason",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentKey",
                table: "PlexTvShowEpisodes",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentKey",
                table: "PlexTvShowSeason");

            migrationBuilder.DropColumn(
                name: "ParentKey",
                table: "PlexTvShowEpisodes");
        }
    }
}
