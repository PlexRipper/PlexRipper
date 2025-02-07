using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class Changes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasBanner",
                table: "PlexTvShowSeason");

            migrationBuilder.DropColumn(
                name: "HasBanner",
                table: "PlexTvShows");

            migrationBuilder.DropColumn(
                name: "HasBanner",
                table: "PlexTvShowEpisodes");

            migrationBuilder.DropColumn(
                name: "HasBanner",
                table: "PlexMovie");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasBanner",
                table: "PlexTvShowSeason",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 20);

            migrationBuilder.AddColumn<bool>(
                name: "HasBanner",
                table: "PlexTvShows",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 20);

            migrationBuilder.AddColumn<bool>(
                name: "HasBanner",
                table: "PlexTvShowEpisodes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 20);

            migrationBuilder.AddColumn<bool>(
                name: "HasBanner",
                table: "PlexMovie",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 20);
        }
    }
}
