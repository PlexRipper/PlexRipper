using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHighestQualityToPlexMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quality",
                table: "PlexTvShows",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 17);

            migrationBuilder.AddColumn<int>(
                name: "Quality",
                table: "PlexMovie",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 17);

            migrationBuilder.Sql(
                """
                UPDATE PlexMovie
                SET Quality = COALESCE((
                    SELECT MAX(Quality)
                    FROM PlexMovieData
                    WHERE PlexMovieData.PlexMovieId = PlexMovie.Id
                ), 0)
                """);

            migrationBuilder.Sql(
                """
                UPDATE PlexTvShows
                SET Quality = COALESCE((
                    SELECT MAX(Quality)
                    FROM PlexTvShowMediaQualities
                    WHERE PlexTvShowMediaQualities.PlexTvShowId = PlexTvShows.Id
                ), 0)
                """);

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShows_Quality",
                table: "PlexTvShows",
                column: "Quality");

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShows_PlexLibraryId_Quality",
                table: "PlexTvShows",
                columns: new[] { "PlexLibraryId", "Quality" });

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovie_Quality",
                table: "PlexMovie",
                column: "Quality");

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovie_PlexLibraryId_Quality",
                table: "PlexMovie",
                columns: new[] { "PlexLibraryId", "Quality" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlexTvShows_Quality",
                table: "PlexTvShows");

            migrationBuilder.DropIndex(
                name: "IX_PlexTvShows_PlexLibraryId_Quality",
                table: "PlexTvShows");

            migrationBuilder.DropIndex(
                name: "IX_PlexMovie_Quality",
                table: "PlexMovie");

            migrationBuilder.DropIndex(
                name: "IX_PlexMovie_PlexLibraryId_Quality",
                table: "PlexMovie");

            migrationBuilder.DropColumn(
                name: "Quality",
                table: "PlexTvShows");

            migrationBuilder.DropColumn(
                name: "Quality",
                table: "PlexMovie");
        }
    }
}
