using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlexGenreRoleAndCountryToPlexMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "HasBanner", table: "PlexTvShowSeason");

            migrationBuilder.DropColumn(name: "HasBanner", table: "PlexTvShows");

            migrationBuilder.DropColumn(name: "HasBanner", table: "PlexTvShowEpisodes");

            migrationBuilder.DropColumn(name: "HasBanner", table: "PlexMovie");

            migrationBuilder.CreateTable(
                name: "PlexCountries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    PlexKey = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexCountries", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexGenres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    PlexKey = table.Column<int>(type: "INTEGER", nullable: false),
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
                    PlexKey = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexRoles", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "PlexLibraryCountries",
                columns: table => new
                {
                    CountriesId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibrariesId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexLibraryCountries", x => new { x.CountriesId, x.PlexLibrariesId });
                    table.ForeignKey(
                        name: "FK_PlexLibraryCountries_PlexCountries_CountriesId",
                        column: x => x.CountriesId,
                        principalTable: "PlexCountries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexLibraryCountries_PlexLibraries_PlexLibrariesId",
                        column: x => x.PlexLibrariesId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
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
                        name: "FK_PlexMovieCountries_PlexCountries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "PlexCountries",
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
                        name: "FK_PlexTvShowCountries_PlexCountries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "PlexCountries",
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
                name: "PlexLibraryGenres",
                columns: table => new
                {
                    GenresId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibrariesId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexLibraryGenres", x => new { x.GenresId, x.PlexLibrariesId });
                    table.ForeignKey(
                        name: "FK_PlexLibraryGenres_PlexGenres_GenresId",
                        column: x => x.GenresId,
                        principalTable: "PlexGenres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexLibraryGenres_PlexLibraries_PlexLibrariesId",
                        column: x => x.PlexLibrariesId,
                        principalTable: "PlexLibraries",
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
                name: "PlexLibraryRoles",
                columns: table => new
                {
                    PlexLibrariesId = table.Column<int>(type: "INTEGER", nullable: false),
                    RolesId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexLibraryRoles", x => new { x.PlexLibrariesId, x.RolesId });
                    table.ForeignKey(
                        name: "FK_PlexLibraryRoles_PlexLibraries_PlexLibrariesId",
                        column: x => x.PlexLibrariesId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_PlexLibraryRoles_PlexRoles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "PlexRoles",
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

            migrationBuilder.CreateIndex(name: "IX_PlexCountries_PlexKey", table: "PlexCountries", column: "PlexKey");

            migrationBuilder.CreateIndex(name: "IX_PlexGenres_PlexKey", table: "PlexGenres", column: "PlexKey");

            migrationBuilder.CreateIndex(
                name: "IX_PlexLibraryCountries_PlexLibrariesId",
                table: "PlexLibraryCountries",
                column: "PlexLibrariesId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexLibraryGenres_PlexLibrariesId",
                table: "PlexLibraryGenres",
                column: "PlexLibrariesId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlexLibraryRoles_RolesId",
                table: "PlexLibraryRoles",
                column: "RolesId"
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

            migrationBuilder.CreateIndex(name: "IX_PlexRoles_PlexKey", table: "PlexRoles", column: "PlexKey");

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
            migrationBuilder.DropTable(name: "PlexLibraryCountries");

            migrationBuilder.DropTable(name: "PlexLibraryGenres");

            migrationBuilder.DropTable(name: "PlexLibraryRoles");

            migrationBuilder.DropTable(name: "PlexMovieCountries");

            migrationBuilder.DropTable(name: "PlexMovieGenres");

            migrationBuilder.DropTable(name: "PlexMovieRoles");

            migrationBuilder.DropTable(name: "PlexTvShowCountries");

            migrationBuilder.DropTable(name: "PlexTvShowGenres");

            migrationBuilder.DropTable(name: "PlexTvShowRoles");

            migrationBuilder.DropTable(name: "PlexCountries");

            migrationBuilder.DropTable(name: "PlexGenres");

            migrationBuilder.DropTable(name: "PlexRoles");

            migrationBuilder
                .AddColumn<bool>(
                    name: "HasBanner",
                    table: "PlexTvShowSeason",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: false
                )
                .Annotation("Relational:ColumnOrder", 20);

            migrationBuilder
                .AddColumn<bool>(
                    name: "HasBanner",
                    table: "PlexTvShows",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: false
                )
                .Annotation("Relational:ColumnOrder", 20);

            migrationBuilder
                .AddColumn<bool>(
                    name: "HasBanner",
                    table: "PlexTvShowEpisodes",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: false
                )
                .Annotation("Relational:ColumnOrder", 20);

            migrationBuilder
                .AddColumn<bool>(
                    name: "HasBanner",
                    table: "PlexMovie",
                    type: "INTEGER",
                    nullable: false,
                    defaultValue: false
                )
                .Annotation("Relational:ColumnOrder", 20);
        }
    }
}
