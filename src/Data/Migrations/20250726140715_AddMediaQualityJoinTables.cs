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
            migrationBuilder.CreateTable(
                name: "PlexMediaQuality",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Quality = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexMediaQuality", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexMovieMediaQuality",
                columns: table => new
                {
                    PlexMediaQualityId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMovieId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_PlexMovieMediaQuality",
                        x => new
                        {
                            x.PlexMediaQualityId,
                            x.PlexLibraryId,
                            x.PlexMovieId,
                        }
                    );
                    table.ForeignKey(
                        name: "FK_PlexMovieMediaQuality_PlexMediaQuality_PlexMediaQualityId",
                        column: x => x.PlexMediaQualityId,
                        principalTable: "PlexMediaQuality",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexMovieMediaQuality_PlexMovie_PlexMovieId",
                        column: x => x.PlexMovieId,
                        principalTable: "PlexMovie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexTvShowEpisodeMediaQuality",
                columns: table => new
                {
                    PlexMediaQualityId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowEpisodeId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_PlexTvShowEpisodeMediaQuality",
                        x => new
                        {
                            x.PlexMediaQualityId,
                            x.PlexLibraryId,
                            x.PlexTvShowEpisodeId,
                        }
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodeMediaQuality_PlexMediaQuality_PlexMediaQualityId",
                        column: x => x.PlexMediaQualityId,
                        principalTable: "PlexMediaQuality",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowEpisodeMediaQuality_PlexTvShowEpisodes_PlexTvShowEpisodeId",
                        column: x => x.PlexTvShowEpisodeId,
                        principalTable: "PlexTvShowEpisodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexTvShowMediaQuality",
                columns: table => new
                {
                    PlexMediaQualityId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_PlexTvShowMediaQuality",
                        x => new
                        {
                            x.PlexMediaQualityId,
                            x.PlexLibraryId,
                            x.PlexTvShowId,
                        }
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowMediaQuality_PlexMediaQuality_PlexMediaQualityId",
                        column: x => x.PlexMediaQualityId,
                        principalTable: "PlexMediaQuality",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowMediaQuality_PlexTvShows_PlexTvShowId",
                        column: x => x.PlexTvShowId,
                        principalTable: "PlexTvShows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexTvShowSeasonMediaQuality",
                columns: table => new
                {
                    PlexMediaQualityId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_PlexTvShowSeasonMediaQuality",
                        x => new
                        {
                            x.PlexMediaQualityId,
                            x.PlexLibraryId,
                            x.PlexTvShowSeasonId,
                        }
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowSeasonMediaQuality_PlexMediaQuality_PlexMediaQualityId",
                        column: x => x.PlexMediaQualityId,
                        principalTable: "PlexMediaQuality",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowSeasonMediaQuality_PlexTvShowSeason_PlexTvShowSeasonId",
                        column: x => x.PlexTvShowSeasonId,
                        principalTable: "PlexTvShowSeason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMediaQuality_Quality",
                table: "PlexMediaQuality",
                column: "Quality"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieMediaQuality_PlexMovieId",
                table: "PlexMovieMediaQuality",
                column: "PlexMovieId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowEpisodeMediaQuality_PlexTvShowEpisodeId",
                table: "PlexTvShowEpisodeMediaQuality",
                column: "PlexTvShowEpisodeId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowMediaQuality_PlexTvShowId",
                table: "PlexTvShowMediaQuality",
                column: "PlexTvShowId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowSeasonMediaQuality_PlexTvShowSeasonId",
                table: "PlexTvShowSeasonMediaQuality",
                column: "PlexTvShowSeasonId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PlexMovieMediaQuality");

            migrationBuilder.DropTable(name: "PlexTvShowEpisodeMediaQuality");

            migrationBuilder.DropTable(name: "PlexTvShowMediaQuality");

            migrationBuilder.DropTable(name: "PlexTvShowSeasonMediaQuality");

            migrationBuilder.DropTable(name: "PlexMediaQuality");
        }
    }
}
