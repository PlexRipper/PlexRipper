using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovePlexRoleAndPlexGenre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PlexMovieGenres");

            migrationBuilder.DropTable(name: "PlexMovieRoles");

            migrationBuilder.DropTable(name: "PlexTvShowGenre");

            migrationBuilder.DropTable(name: "PlexTvShowRole");

            migrationBuilder.DropTable(name: "PlexRoles");

            migrationBuilder.DropTable(name: "PlexGenres");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlexGenres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Tag = table.Column<string>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexGenres", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Tag = table.Column<string>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexRoles", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexMovieGenres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMoviesId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexGenreId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMovieId = table.Column<int>(type: "INTEGER", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexMovieGenres", x => new { x.PlexMoviesId, x.PlexGenreId });
                    table.ForeignKey(
                        name: "FK_PlexMovieGenres_PlexGenres_PlexGenreId",
                        column: x => x.PlexGenreId,
                        principalTable: "PlexGenres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexMovieGenres_PlexMovie_PlexMovieId",
                        column: x => x.PlexMovieId,
                        principalTable: "PlexMovie",
                        principalColumn: "Id"
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexTvShowGenre",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexGenreId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShowGenre", x => new { x.PlexTvShowId, x.PlexGenreId });
                    table.ForeignKey(
                        name: "FK_PlexTvShowGenre_PlexGenres_PlexGenreId",
                        column: x => x.PlexGenreId,
                        principalTable: "PlexGenres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowGenre_PlexTvShows_PlexTvShowId",
                        column: x => x.PlexTvShowId,
                        principalTable: "PlexTvShows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexTvShowRole",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexGenreId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShowRole", x => new { x.PlexTvShowId, x.PlexGenreId });
                    table.ForeignKey(
                        name: "FK_PlexTvShowRole_PlexGenres_PlexGenreId",
                        column: x => x.PlexGenreId,
                        principalTable: "PlexGenres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowRole_PlexTvShows_PlexTvShowId",
                        column: x => x.PlexTvShowId,
                        principalTable: "PlexTvShows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexMovieRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMoviesId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexGenreId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMovieId = table.Column<int>(type: "INTEGER", nullable: true),
                    PlexRoleId = table.Column<int>(type: "INTEGER", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexMovieRoles", x => new { x.PlexMoviesId, x.PlexGenreId });
                    table.ForeignKey(
                        name: "FK_PlexMovieRoles_PlexGenres_PlexGenreId",
                        column: x => x.PlexGenreId,
                        principalTable: "PlexGenres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexMovieRoles_PlexMovie_PlexMovieId",
                        column: x => x.PlexMovieId,
                        principalTable: "PlexMovie",
                        principalColumn: "Id"
                    );
                    table.ForeignKey(
                        name: "FK_PlexMovieRoles_PlexRoles_PlexRoleId",
                        column: x => x.PlexRoleId,
                        principalTable: "PlexRoles",
                        principalColumn: "Id"
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieGenres_PlexGenreId",
                table: "PlexMovieGenres",
                column: "PlexGenreId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieGenres_PlexMovieId",
                table: "PlexMovieGenres",
                column: "PlexMovieId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieRoles_PlexGenreId",
                table: "PlexMovieRoles",
                column: "PlexGenreId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieRoles_PlexMovieId",
                table: "PlexMovieRoles",
                column: "PlexMovieId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieRoles_PlexRoleId",
                table: "PlexMovieRoles",
                column: "PlexRoleId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowGenre_PlexGenreId",
                table: "PlexTvShowGenre",
                column: "PlexGenreId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowRole_PlexGenreId",
                table: "PlexTvShowRole",
                column: "PlexGenreId"
            );
        }
    }
}
