using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesGenresAndCountries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlexCountry",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    PlexId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexCountry", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexGenres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Key = table.Column<int>(type: "INTEGER", nullable: false),
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
                    PlexId = table.Column<int>(type: "INTEGER", nullable: false),
                    TagKey = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexRoles", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexMovieCountries",
                columns: table => new
                {
                    CountryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMovieCountriesId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexMovieCountries", x => new { x.CountryId, x.PlexMovieCountriesId });
                    table.ForeignKey(
                        name: "FK_PlexMovieCountries_PlexCountry_CountryId",
                        column: x => x.CountryId,
                        principalTable: "PlexCountry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexMovieCountries_PlexMovie_PlexMovieCountriesId",
                        column: x => x.PlexMovieCountriesId,
                        principalTable: "PlexMovie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexTvShowCountries",
                columns: table => new
                {
                    CountryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowCountriesId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShowCountries", x => new { x.CountryId, x.PlexTvShowCountriesId });
                    table.ForeignKey(
                        name: "FK_PlexTvShowCountries_PlexCountry_CountryId",
                        column: x => x.CountryId,
                        principalTable: "PlexCountry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowCountries_PlexTvShows_PlexTvShowCountriesId",
                        column: x => x.PlexTvShowCountriesId,
                        principalTable: "PlexTvShows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexMovieGenres",
                columns: table => new
                {
                    GenresId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMovieGenresId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexMovieGenres", x => new { x.GenresId, x.PlexMovieGenresId });
                    table.ForeignKey(
                        name: "FK_PlexMovieGenres_PlexGenres_GenresId",
                        column: x => x.GenresId,
                        principalTable: "PlexGenres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexMovieGenres_PlexMovie_PlexMovieGenresId",
                        column: x => x.PlexMovieGenresId,
                        principalTable: "PlexMovie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexTvShowGenres",
                columns: table => new
                {
                    GenresId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowGenresId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShowGenres", x => new { x.GenresId, x.PlexTvShowGenresId });
                    table.ForeignKey(
                        name: "FK_PlexTvShowGenres_PlexGenres_GenresId",
                        column: x => x.GenresId,
                        principalTable: "PlexGenres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowGenres_PlexTvShows_PlexTvShowGenresId",
                        column: x => x.PlexTvShowGenresId,
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
                    PlexMovieRolesId = table.Column<int>(type: "INTEGER", nullable: false),
                    RolesId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexMovieRoles", x => new { x.PlexMovieRolesId, x.RolesId });
                    table.ForeignKey(
                        name: "FK_PlexMovieRoles_PlexMovie_PlexMovieRolesId",
                        column: x => x.PlexMovieRolesId,
                        principalTable: "PlexMovie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexMovieRoles_PlexRoles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "PlexRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexTvShowRoles",
                columns: table => new
                {
                    PlexTvShowRolesId = table.Column<int>(type: "INTEGER", nullable: false),
                    RolesId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShowRoles", x => new { x.PlexTvShowRolesId, x.RolesId });
                    table.ForeignKey(
                        name: "FK_PlexTvShowRoles_PlexRoles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "PlexRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexTvShowRoles_PlexTvShows_PlexTvShowRolesId",
                        column: x => x.PlexTvShowRolesId,
                        principalTable: "PlexTvShows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieCountries_PlexMovieCountriesId",
                table: "PlexMovieCountries",
                column: "PlexMovieCountriesId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieGenres_PlexMovieGenresId",
                table: "PlexMovieGenres",
                column: "PlexMovieGenresId"
            );

            migrationBuilder.CreateIndex(name: "IX_PlexMovieRoles_RolesId", table: "PlexMovieRoles", column: "RolesId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowCountries_PlexTvShowCountriesId",
                table: "PlexTvShowCountries",
                column: "PlexTvShowCountriesId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowGenres_PlexTvShowGenresId",
                table: "PlexTvShowGenres",
                column: "PlexTvShowGenresId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowRoles_RolesId",
                table: "PlexTvShowRoles",
                column: "RolesId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PlexMovieCountries");

            migrationBuilder.DropTable(name: "PlexMovieGenres");

            migrationBuilder.DropTable(name: "PlexMovieRoles");

            migrationBuilder.DropTable(name: "PlexTvShowCountries");

            migrationBuilder.DropTable(name: "PlexTvShowGenres");

            migrationBuilder.DropTable(name: "PlexTvShowRoles");

            migrationBuilder.DropTable(name: "PlexCountry");

            migrationBuilder.DropTable(name: "PlexGenres");

            migrationBuilder.DropTable(name: "PlexRoles");
        }
    }
}
