using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddParentGUIDToSeasonAndEpisode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParentGuid",
                table: "PlexTvShowSeason",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "ParentGuid",
                table: "PlexTvShowEpisodes",
                type: "TEXT",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ParentGuid", table: "PlexTvShowSeason");

            migrationBuilder.DropColumn(name: "ParentGuid", table: "PlexTvShowEpisodes");
        }
    }
}
