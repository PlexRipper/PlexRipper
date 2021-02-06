using Microsoft.EntityFrameworkCore.Migrations;

namespace PlexRipper.Data.Migrations
{
    public partial class AddTvShowIdToTvShowEpisodesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TvShowId",
                table: "PlexTvShowEpisodes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodes_TvShowId",
                table: "PlexTvShowEpisodes",
                column: "TvShowId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlexTvShowEpisodes_PlexTvShow_TvShowId",
                table: "PlexTvShowEpisodes",
                column: "TvShowId",
                principalTable: "PlexTvShow",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlexTvShowEpisodes_PlexTvShow_TvShowId",
                table: "PlexTvShowEpisodes");

            migrationBuilder.DropIndex(
                name: "IX_PlexTvShowEpisodes_TvShowId",
                table: "PlexTvShowEpisodes");

            migrationBuilder.DropColumn(
                name: "TvShowId",
                table: "PlexTvShowEpisodes");
        }
    }
}
