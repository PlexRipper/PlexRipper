using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaQualityJoinTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quality",
                table: "PlexTvShowEpisodeData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AddColumn<int>(
                name: "Quality",
                table: "PlexMovieData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.CreateTable(
                name: "PlexTvShowMediaQualities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quality = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShowMediaQualities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexTvShowMediaQualities_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexTvShowMediaQualities_PlexTvShows_PlexTvShowId",
                        column: x => x.PlexTvShowId,
                        principalTable: "PlexTvShows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexTvShowSeasonMediaQualities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quality = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShowSeasonMediaQualities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlexTvShowSeasonMediaQualities_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexTvShowSeasonMediaQualities_PlexTvShowSeason_PlexTvShowSeasonId",
                        column: x => x.PlexTvShowSeasonId,
                        principalTable: "PlexTvShowSeason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodeData_Quality",
                table: "PlexTvShowEpisodeData",
                column: "Quality");

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieData_Quality",
                table: "PlexMovieData",
                column: "Quality");

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowMediaQualities_PlexLibraryId",
                table: "PlexTvShowMediaQualities",
                column: "PlexLibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowMediaQualities_PlexTvShowId",
                table: "PlexTvShowMediaQualities",
                column: "PlexTvShowId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowSeasonMediaQualities_PlexLibraryId",
                table: "PlexTvShowSeasonMediaQualities",
                column: "PlexLibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowSeasonMediaQualities_PlexTvShowSeasonId",
                table: "PlexTvShowSeasonMediaQualities",
                column: "PlexTvShowSeasonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlexTvShowMediaQualities");

            migrationBuilder.DropTable(
                name: "PlexTvShowSeasonMediaQualities");

            migrationBuilder.DropIndex(
                name: "IX_PlexTvShowEpisodeData_Quality",
                table: "PlexTvShowEpisodeData");

            migrationBuilder.DropIndex(
                name: "IX_PlexMovieData_Quality",
                table: "PlexMovieData");

            migrationBuilder.DropColumn(
                name: "Quality",
                table: "PlexTvShowEpisodeData");

            migrationBuilder.DropColumn(
                name: "Quality",
                table: "PlexMovieData");
        }
    }
}
