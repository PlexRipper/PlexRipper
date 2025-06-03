using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class RestructureMediaMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlexLibraryRoles");

            migrationBuilder.DropTable(
                name: "PlexMovieRoles");

            migrationBuilder.DropTable(
                name: "PlexTvShowRoles");

            migrationBuilder.DropTable(
                name: "PlexRoles");

            migrationBuilder.DropIndex(
                name: "IX_PlexGenres_PlexKey",
                table: "PlexGenres");

            migrationBuilder.DropIndex(
                name: "IX_PlexCountries_PlexKey",
                table: "PlexCountries");

            migrationBuilder.DropColumn(
                name: "PlexKey",
                table: "PlexGenres");

            migrationBuilder.DropColumn(
                name: "PlexKey",
                table: "PlexCountries");

            migrationBuilder.AddColumn<int>(
                name: "PlexKey",
                table: "PlexLibraryGenres",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AddColumn<int>(
                name: "PlexKey",
                table: "PlexLibraryCountries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "PlexGenres",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "PlexCountries",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PlexActors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Thumb = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexActors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlexLibraryActors",
                columns: table => new
                {
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexActorId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexKey = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexLibraryActors", x => new { x.PlexActorId, x.PlexLibraryId });
                    table.ForeignKey(
                        name: "FK_PlexLibraryActors_PlexActors_PlexActorId",
                        column: x => x.PlexActorId,
                        principalTable: "PlexActors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexLibraryActors_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexMovieActors",
                columns: table => new
                {
                    PlexActorId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMovieId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexMovieActors", x => new { x.PlexActorId, x.PlexMovieId });
                    table.ForeignKey(
                        name: "FK_PlexMovieActors_PlexActors_PlexActorId",
                        column: x => x.PlexActorId,
                        principalTable: "PlexActors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexMovieActors_PlexMovie_PlexMovieId",
                        column: x => x.PlexMovieId,
                        principalTable: "PlexMovie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexTvShowActors",
                columns: table => new
                {
                    PlexActorId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShowActors", x => new { x.PlexActorId, x.PlexTvShowId });
                    table.ForeignKey(
                        name: "FK_PlexTvShowActors_PlexActors_PlexActorId",
                        column: x => x.PlexActorId,
                        principalTable: "PlexActors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexTvShowActors_PlexTvShows_PlexTvShowId",
                        column: x => x.PlexTvShowId,
                        principalTable: "PlexTvShows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlexGenres_Key",
                table: "PlexGenres",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlexCountries_Key",
                table: "PlexCountries",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlexActors_Key",
                table: "PlexActors",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlexLibraryActors_PlexLibraryId",
                table: "PlexLibraryActors",
                column: "PlexLibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieActors_PlexMovieId",
                table: "PlexMovieActors",
                column: "PlexMovieId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowActors_PlexTvShowId",
                table: "PlexTvShowActors",
                column: "PlexTvShowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlexLibraryActors");

            migrationBuilder.DropTable(
                name: "PlexMovieActors");

            migrationBuilder.DropTable(
                name: "PlexTvShowActors");

            migrationBuilder.DropTable(
                name: "PlexActors");

            migrationBuilder.DropIndex(
                name: "IX_PlexGenres_Key",
                table: "PlexGenres");

            migrationBuilder.DropIndex(
                name: "IX_PlexCountries_Key",
                table: "PlexCountries");

            migrationBuilder.DropColumn(
                name: "PlexKey",
                table: "PlexLibraryGenres");

            migrationBuilder.DropColumn(
                name: "PlexKey",
                table: "PlexLibraryCountries");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "PlexGenres");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "PlexCountries");

            migrationBuilder.AddColumn<int>(
                name: "PlexKey",
                table: "PlexGenres",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlexKey",
                table: "PlexCountries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PlexRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    PlexKey = table.Column<int>(type: "INTEGER", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: true),
                    TagKey = table.Column<string>(type: "TEXT", nullable: true),
                    Thumb = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlexLibraryRoles",
                columns: table => new
                {
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexRoleId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexLibraryRoles", x => new { x.PlexLibraryId, x.PlexRoleId });
                    table.ForeignKey(
                        name: "FK_PlexLibraryRoles_PlexLibraries_PlexLibraryId",
                        column: x => x.PlexLibraryId,
                        principalTable: "PlexLibraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexLibraryRoles_PlexRoles_PlexRoleId",
                        column: x => x.PlexRoleId,
                        principalTable: "PlexRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexMovieRoles",
                columns: table => new
                {
                    RolesId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexMovieId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexMovieRoles", x => new { x.PlexMovieId, x.RolesId });
                    table.ForeignKey(
                        name: "FK_PlexMovieRoles_PlexMovie_PlexMovieId",
                        column: x => x.PlexMovieId,
                        principalTable: "PlexMovie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexMovieRoles_PlexRoles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "PlexRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlexTvShowRoles",
                columns: table => new
                {
                    RolesId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexLibraryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlexTvShowId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlexTvShowRoles", x => new { x.PlexTvShowId, x.RolesId });
                    table.ForeignKey(
                        name: "FK_PlexTvShowRoles_PlexRoles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "PlexRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlexTvShowRoles_PlexTvShows_PlexTvShowId",
                        column: x => x.PlexTvShowId,
                        principalTable: "PlexTvShows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlexGenres_PlexKey",
                table: "PlexGenres",
                column: "PlexKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlexCountries_PlexKey",
                table: "PlexCountries",
                column: "PlexKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlexLibraryRoles_PlexRoleId",
                table: "PlexLibraryRoles",
                column: "PlexRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexMovieRoles_RolesId",
                table: "PlexMovieRoles",
                column: "RolesId");

            migrationBuilder.CreateIndex(
                name: "IX_PlexRoles_PlexKey",
                table: "PlexRoles",
                column: "PlexKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlexTvShowRoles_RolesId",
                table: "PlexTvShowRoles",
                column: "RolesId");
        }
    }
}
