using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesGenersToPlexLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlexLibraryCountries",
                columns: table => new
                {
                    CountryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibrariesId = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexLibraryCountries", x => new { x.CountryId, x.PlexLibrariesId });
                    table.ForeignKey(
                        name: "FK_PlexLibraryCountries_PlexCountry_CountryId",
                        column: x => x.CountryId,
                        principalTable: "PlexCountry",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PlexLibraryCountries");

            migrationBuilder.DropTable(name: "PlexLibraryGenres");

            migrationBuilder.DropTable(name: "PlexLibraryRoles");
        }
    }
}
